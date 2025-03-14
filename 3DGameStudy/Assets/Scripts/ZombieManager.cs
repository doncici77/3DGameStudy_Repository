using System.Collections;
using System.Xml.Serialization;
using UnityEngine;

public enum EZombieState
{
    Patrol, // 순찰모드
    Chase, //추적
    Attack, // 공격
    Evade, // 도망
    Idle, // 서있는 상태
    Die // 죽음
}

public class ZombieManager : MonoBehaviour
{
    private EZombieState currentState;
    public EZombieState defaultState = EZombieState.Idle;
    public Transform target;
    public float attackRange = 1.0f; // 공격 범위
    public float attackDelay = 2.0f; // 공격 딜레이
    private float nextAttackTime = 0.0f; // 다음 공격 시간관리
    public Transform[] patrolPoints; // 순찰 경로 지점들
    private int currentPoint = 0; // 현재 순찰 경로 지점 인덱스
    public float moveSpeed = 2.0f; // 이동속도
    private float currentMoveSpeed = 0;
    public float trackingRange = 3.0f; // 추적 범위 설정
    private bool isAttack = false; // 공격 상태
    private float evadeRange = 5.0f; // 도망 상태 회피 거리
    private float zombieHp = 10.0f; // 좀비 체력
    private float distanceTotarget; // target과의 거리 계산 값
    private bool isWaiting = false; // 상태 전환 후 대기상태 여부
    public float idleTime = 2.0f; // 각 상태 전환 후 대기시간
    private Coroutine stateRoutine; // 코루틴의 진행상태를 저장하는 변수

    private Animator animator;
    public AudioClip zombieAttackSound;
    public AudioClip zombieChaseSound;
    public AudioClip zombieTakeDamageSound;
    public AudioClip zombieDieSound;
    private AudioSource audioSource;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        currentState = defaultState;
        ChangeState(currentState); // 상태 초기화
        currentMoveSpeed = moveSpeed;
    }

    void Update()
    {
        if(target != null)
        {
            distanceTotarget = Vector3.Distance(transform.position, target.position);
        }

    }

    void AttackSoundOn()
    {
        audioSource.PlayOneShot(zombieAttackSound);
    }

    public void ChangeState(EZombieState newState)
    {
        if(stateRoutine != null) // 현재 상태 종료, 저장 되어있는 코루틴 종료
        {
            StopCoroutine(stateRoutine);
        }

        currentState = newState; // 새로운 현재 상태 변경

        switch(currentState) // 변경된 상태의 코루틴 실행
        {
            case EZombieState.Idle:
                stateRoutine = StartCoroutine(Idle()); // stateRoutine에 현재 진행중인 코루틴 함수 저장
                break;

            case EZombieState.Patrol:
                stateRoutine = StartCoroutine(Patrol());
                break;

            case EZombieState.Chase:
                stateRoutine = StartCoroutine(Chase(target));
                break;

            case EZombieState.Attack:
                stateRoutine = StartCoroutine(Attack());
                break;

            case EZombieState.Evade:
                stateRoutine = StartCoroutine(Evade());
                break;

            case EZombieState.Die:
                stateRoutine = StartCoroutine(Die());
                break;
        }
    }

    private IEnumerator Idle()
    {
        Debug.Log(gameObject.name + " : 대기중");

        animator.SetBool("isMove", false);
        animator.Play("Zombie_Idle");

        while (currentState == EZombieState.Idle)
        {
            // 상태 확인, 변경
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
            }
            else if (distance < trackingRange)
            {
                ChangeState(EZombieState.Chase);
            }

            yield return null;
        }
    }

    private IEnumerator Patrol()
    {
        Debug.Log(gameObject.name + " : 순찰중");

        animator.SetBool("isMove", true);

        while (currentState == EZombieState.Patrol)
        {
            if(patrolPoints.Length > 0) // 순찰포인트가 2개 이상일때
            {
                // 순찰
                Transform targetPoint = patrolPoints[currentPoint];
                Vector3 direction = (targetPoint.position - transform.position).normalized;
                transform.position += direction * currentMoveSpeed * Time.deltaTime;
                transform.LookAt(targetPoint.position);

                if (Vector3.Distance(transform.position, targetPoint.position) < 0.3)
                {
                    currentPoint = (currentPoint + 1) % patrolPoints.Length;
                }

                // 상태 확인, 변경
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance < attackRange)
                {
                    ChangeState(EZombieState.Attack);
                }
                else if (distance < trackingRange)
                {
                    ChangeState(EZombieState.Chase);
                }
            }

            yield return null;
        }
    }

    private IEnumerator Chase(Transform target)
    {
        Debug.Log(gameObject.name + " : 추격중");

        animator.SetBool("isMove", true);
        audioSource.PlayOneShot(zombieChaseSound);

        while (currentState == EZombieState.Chase)
        {
            // 추적 코드
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * currentMoveSpeed * Time.deltaTime;
            transform.LookAt(target.position);

            // 상태 확인, 변경
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
            }
            else if (distance > trackingRange * 1.5f)
            {
                ChangeState(defaultState);
            }

            yield return null;
        }
    }

    private IEnumerator Attack()
    {
        // 공격 코드
        currentMoveSpeed = 0;
        Debug.Log(gameObject.name + " : 공격!!!!");
        transform.LookAt(target.position);
        animator.SetTrigger("isAttack");

        yield return new WaitForSeconds(attackDelay); // 공격후 딜레이 방생
        currentMoveSpeed = moveSpeed;

        // 상태 확인, 변경
        float distance = Vector3.Distance(transform.position, target.position);

        if(distance > trackingRange)
        {
            ChangeState(defaultState);
        }
        else if(distance > attackRange)
        {
            ChangeState(EZombieState.Chase);
        }
        else
        {
            ChangeState(EZombieState.Attack);
        }
    }

    private IEnumerator Evade()
    {
        Debug.Log(gameObject.name + " : 도망중");

        Vector3 evadeDirection = (transform.position - target.position).normalized; // 플레이어의 반대 방향
        animator.SetBool("isMove", false);
        float evadeTime = 3.0f;
        float timer = 0.0f;

        Quaternion targetRotation = Quaternion.LookRotation(evadeDirection);
        transform.rotation = targetRotation;

        while(currentState == EZombieState.Evade && timer < evadeTime)
        {
            transform.position += evadeDirection * currentMoveSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        ChangeState(EZombieState.Idle);
    }

    public void TakeDamage(float damage)
    {
        // 데미지 피해 코드
        Debug.Log(gameObject.name + " : " + damage + " : 데미지 받음");
        zombieHp -= damage;

        // 상태 확인, 변경
        float distance = Vector3.Distance(transform.position, target.position);

        if (zombieHp <= 0)
        {
            ChangeState(EZombieState.Die);
        }
        else
        {
            animator.SetTrigger("isTakeDamage");
            audioSource.PlayOneShot(zombieTakeDamageSound);
            if(distance > trackingRange)
            {
                ChangeState(defaultState);
            }
            else
            {
                ChangeState(EZombieState.Chase);
            }
        }
    }

    private IEnumerator Die()
    {
        currentMoveSpeed = 0;
        Debug.Log(gameObject.name + " : 죽음");
        animator.SetTrigger("isDie");
        audioSource.PlayOneShot(zombieDieSound);
        yield return new WaitForSeconds(3.0f);
        gameObject.SetActive(false);
    }
}

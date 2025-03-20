using System.Collections;
using Unity.AI.Navigation;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.AI;

public enum EZombieState
{
    Patrol, // 순찰모드
    Chase, //추적
    Attack, // 공격
    Evade, // 도망
    Idle, // 서있는 상태
    Stop, // 멈춤상태
    Die // 죽음
}

public class ZombieManager : MonoBehaviour
{
    private EZombieState currentState;
    public EZombieState defaultState = EZombieState.Idle;
    public float attackRange = 1.0f; // 공격 범위
    public float attackDelay = 2.0f; // 공격 딜레이
    private float nextAttackTime = 0.0f; // 다음 공격 시간관리
    public Transform[] patrolPoints; // 순찰 경로 지점들
    private int currentPoint = 0; // 현재 순찰 경로 지점 인덱스
    public float moveSpeed = 2.0f; // 이동속도
    public float trackingRange = 3.0f; // 추적 범위 설정
    private bool isAttack = false; // 공격 상태
    private float evadeRange = 5.0f; // 도망 상태 회피 거리
    private float zombieHp = 10.0f; // 좀비 체력
    private float distanceTotarget; // target과의 거리 계산 값
    private bool isWaiting = false; // 상태 전환 후 대기상태 여부
    public float idleTime = 2.0f; // 각 상태 전환 후 대기시간
    private Coroutine stateRoutine; // 코루틴의 진행상태를 저장하는 변수

    private Animator animator;

    private NavMeshAgent agent;

    private bool isJumping = false;
    private Rigidbody rb;
    public float jumpHeight = 2.0f;
    public float jumpDuration = 1.0f;
    private NavMeshLink[] navMeshLinks;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        currentState = defaultState;
        ChangeState(currentState); // 상태 초기화
        rb = GetComponent<Rigidbody>();
        if(rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;

        navMeshLinks = FindObjectsOfType<NavMeshLink>();
    }

    void Update()
    {
        if(PlayerManager.Instance.transform.position != null)
        {
            distanceTotarget = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);
        }
    }

    void AttackSoundOn()
    {
        SoundManager.Instance.PlaySFX("ZombieAttackSound");
        Animation animation = GetComponent<Animation>();
    }

    public void ChangeState(EZombieState newState)
    {
        if (isJumping) return;

        if (stateRoutine != null) // 현재 상태 종료, 저장 되어있는 코루틴 종료
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
                stateRoutine = StartCoroutine(Chase(PlayerManager.Instance.transform));
                break;

            case EZombieState.Attack:
                stateRoutine = StartCoroutine(Attack());
                break;

            case EZombieState.Evade:
                stateRoutine = StartCoroutine(Evade());
                break;

            case EZombieState.Stop:
                stateRoutine = StartCoroutine(Stop());
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

        agent.isStopped = true;

        while (currentState == EZombieState.Idle)
        {
            // 상태 확인, 변경
            float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);

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
                agent.speed = moveSpeed;
                agent.isStopped = false;
                agent.destination = targetPoint.position;

                //transform.position += direction * currentMoveSpeed * Time.deltaTime;
                //transform.LookAt(targetPoint.position);

                if(agent.isOnOffMeshLink)
                {
                    StartCoroutine(JumpAcrossLink());
                }

                if (Vector3.Distance(transform.position, targetPoint.position) < 0.3)
                {
                    currentPoint = (currentPoint + 1) % patrolPoints.Length;
                }

                // 상태 확인, 변경
                float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);
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
        SoundManager.Instance.PlaySFX("ZombieChaseSound");

        while (currentState == EZombieState.Chase)
        {
            // 추적 코드
            Vector3 direction = (target.position - transform.position).normalized;
            agent.speed = moveSpeed;
            agent.isStopped = false;
            agent.destination = target.position;

            //transform.position += direction * currentMoveSpeed * Time.deltaTime;
            //transform.LookAt(target.position);

            // 상태 확인, 변경
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
            }
            else if (distance > trackingRange)
            {
                ChangeState(defaultState);
            }

            yield return null;
        }
    }

    private IEnumerator Attack()
    {
        // 공격 코드
        Debug.Log(gameObject.name + " : 공격!!!!");
        //transform.LookAt(target.position);agent.speed = moveSpeed;
        agent.isStopped = true;
        agent.destination = PlayerManager.Instance.transform.position;
        animator.SetTrigger("isAttack");

        yield return new WaitForSeconds(attackDelay); // 공격후 딜레이 방생

        // 상태 확인, 변경
        float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);

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

        Vector3 evadeDirection = (transform.position - PlayerManager.Instance.transform.position).normalized; // 플레이어의 반대 방향
        animator.SetBool("isMove", false);
        float evadeTime = 3.0f;
        float timer = 0.0f;

        Quaternion targetRotation = Quaternion.LookRotation(evadeDirection);
        transform.rotation = targetRotation;

        while(currentState == EZombieState.Evade && timer < evadeTime)
        {
            //transform.position += evadeDirection * currentMoveSpeed * Time.deltaTime;
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
        float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);

        if (zombieHp <= 0)
        {
            ChangeState(EZombieState.Die);
        }
        else
        {
            animator.SetTrigger("isTakeDamage");
            StartCoroutine(TakeDamageSequence(distance));
        }
    }

    private IEnumerator TakeDamageSequence(float distance)
    {
        ChangeState(EZombieState.Stop);
        yield return StartCoroutine(Stop());  // Stop 코루틴이 끝날 때까지 기다림

        if (distance > trackingRange)
        {
            ChangeState(defaultState);
        }
        else
        {
            ChangeState(EZombieState.Chase);
        }
    }

    private IEnumerator Stop()
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(1.0f);
    }

    private IEnumerator Die()
    {
        agent.isStopped = true;
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.enabled = false;

        Debug.Log(gameObject.name + " : 죽음");
        animator.SetTrigger("isDie");
        SoundManager.Instance.PlaySFX("ZombieDieSound");
        yield return new WaitForSeconds(3.0f);
        gameObject.SetActive(false);
    }

    private IEnumerator JumpAcrossLink()
    {
        Debug.Log(gameObject.name + " 좀비 점프");

        isJumping = true;

        // NavMeshAgent의 이동을 멈춤.
        agent.isStopped = true;

        // 좀비가 점프해야 하는 시작 위치(startPos)와 끝 위치(endPos)를 가져옴.
        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        Vector3 startPos = linkData.startPos;
        Vector3 endPos = linkData.endPos;

        // 점프 경로 계산 (포물선을 그리며 점프)
        float elsapsedTime = 0;
        while(elsapsedTime < jumpDuration)
        {
            float t = elsapsedTime / jumpDuration;
            Vector3 currentPosition = Vector3.Lerp(startPos, endPos, t);
            currentPosition.y += Mathf.Sin(t * Mathf.PI) * jumpHeight; // 포물선 경로
            transform.position = currentPosition;

            elsapsedTime += Time.deltaTime;
            yield return null;
        }

        //도착점의 위치
        transform.position = endPos;

        //NavMeshAgent 경로 재개
        agent.CompleteOffMeshLink();
        agent.isStopped = false;
        isJumping = false;
    }
}

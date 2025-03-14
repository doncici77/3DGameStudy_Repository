using System.Xml.Serialization;
using UnityEngine;

public enum EZombieState
{
    Patol, // 순찰모드
    Chase, //추적
    Attack, // 공격
    Evade, // 도망
    TakeDamage, // 데미지를 받음
    Idle, // 서있는 상태
    Die // 죽음
}

public class ZombieManager : MonoBehaviour
{
    public EZombieState currentState = EZombieState.Idle;
    public Transform target;
    public float attackRange = 1.0f; // 공격 범위
    public float attackDelay = 2.0f; // 공격 딜레이
    private float nextAttackTime = 0.0f; // 다음 공격 시간관리
    public Transform[] patrolPoints; // 순찰 경로 지점들
    private int currentPoint = 0; // 현재 순찰 경로 지점 인덱스
    public float moveSpeed = 2.0f; // 이동속도
    private float trackingRange = 3.0f; // 추적 범위 설정
    private bool isAttack = false; // 공격 상태
    private float evadeRange = 5.0f; // 도망 상태 회피 거리
    private float zombieHp = 10.0f; // 좀비 체력
    private float distanceTotarget; // target과의 거리 계산 값
    private bool isWaiting = false; // 상태 전환 후 대기상태 여부
    public float idleTime = 2.0f; // 각 상태 전환 후 대기시간

    void Start()
    {
        
    }

    void Update()
    {
        distanceTotarget = Vector3.Distance(transform.position, target.position);

        UpdateState();
        StartState();
    }

    void UpdateState()
    {
        if (distanceTotarget < attackRange)
        {
            currentState = EZombieState.Attack;
        }
        else if (distanceTotarget < trackingRange)
        {
            currentState = EZombieState.Chase;
        }
        else
        {
            currentState = EZombieState.Patol;
        }
    }

    void StartState()
    {
        switch (currentState)
        {
            case EZombieState.Idle:
                Idle();
                break;

            case EZombieState.Patol:
                Patrol();
                break;

            case EZombieState.Chase:
                Chase(target);
                break;

            case EZombieState.Attack:
                Attack();
                break;

            case EZombieState.Evade:
                Evade();
                break;

            case EZombieState.TakeDamage:
                TakeDamage();
                break;

            case EZombieState.Die:
                Die();
                break;
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length > 0)
        {
            Debug.Log(gameObject.name + " : 순찰중");
            Transform targetPoint = patrolPoints[currentPoint];
            Vector3 direction = (targetPoint.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(targetPoint.position);

            if (Vector3.Distance(transform.position, targetPoint.position) < 0.3)
            {
                currentPoint = (currentPoint + 1) % patrolPoints.Length;
            }
        }
    }

    void Chase(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.LookAt(target.position);
        Debug.Log(gameObject.name + " : 추격중");
    }

    void Evade()
    {
        Debug.Log(gameObject.name + " : 도망중");
    }

    void Idle()
    {
        Debug.Log(gameObject.name + " : 대기중");
    }

    void Attack()
    {
        Debug.Log(gameObject.name + " : 공격!!!!");
    }

    void Die()
    {
        Debug.Log(gameObject.name + " : 죽음");
    }

    void TakeDamage()
    {
        Debug.Log(gameObject.name + " : 데미지 받음");
    }
}

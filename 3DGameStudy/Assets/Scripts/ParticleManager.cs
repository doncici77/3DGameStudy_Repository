using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ParticleType
{
    DamageExplosion,
    WeaponFire,
    WeaponSmoke,
    Healing
}

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance
    {
        get; 
        private set;
    }

    private Dictionary<ParticleType, GameObject> particleSystemDic = new Dictionary<ParticleType, GameObject>();
    private Dictionary<ParticleType, Queue<GameObject>> particlePools = new Dictionary<ParticleType, Queue<GameObject>>();

    public GameObject weaponExplosionParticle;
    public GameObject weaponSmokeParticle;
    public GameObject weaponFireParticle;
    public GameObject HealingParticle;

    public int poolSize = 30;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        particleSystemDic.Add(ParticleType.DamageExplosion, weaponExplosionParticle);
        particleSystemDic.Add(ParticleType.WeaponFire, weaponFireParticle);
        particleSystemDic.Add(ParticleType.WeaponSmoke, weaponSmokeParticle);
        particleSystemDic.Add(ParticleType.Healing, HealingParticle);

        foreach (var type in particleSystemDic.Keys)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            for(int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(particleSystemDic[type]);
                obj.transform.SetParent(this.transform); // 풀링된 오브젝트를 ParticleManager의 자식으로 설정
                obj.gameObject.SetActive(false);
                pool.Enqueue(obj);
            }

            if (!particlePools.ContainsKey(type))
            {
                particlePools[type] = pool; // Dictionary에 추가
            }
            else
            {
                Debug.LogWarning($"Particle pool already contains type: {type}");
            }
        }
    }

    public void ParticlePlay(ParticleType type, Transform position, Vector3 scale)
    {
        if(particlePools.ContainsKey(type))
        {
            GameObject particleObj = particlePools[type].Dequeue();

            if(particleObj != null)
            {
                particleObj.transform.SetParent(position); // 부모 설정
                particleObj.transform.localPosition = Vector3.zero; // 부모 기준 위치 초기화
                particleObj.transform.localScale = scale; // 크기 설정

                ParticleSystem particleSystem = particleObj.GetComponentInChildren<ParticleSystem>();

                if(particleSystem.isPlaying)
                {
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                particleObj.SetActive(true);
                particleSystem.Play();
                StartCoroutine(particleEnd(type, particleObj, particleSystem));
            }
        }
    }

    IEnumerator particleEnd(ParticleType type, GameObject particleObj, ParticleSystem particleSystem)
    {
        while(particleSystem.isPlaying)
        {
            yield return null;
        }

        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particleObj.SetActive(false);

        // 다시 풀에 넣기 전에 부모를 ParticleManager로 변경 (안전하게 관리하기 위함)
        particleObj.transform.SetParent(this.transform);

        particlePools[type].Enqueue(particleObj);
    }
}

using UnityEngine;
using UnityEngine.UI; 
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("스탯")]
    public float speed = 6f;
    public float hp = 100f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    public float attackRange = 1f;
    private float lastAttackTime;
    private float maxHp;

    [Header("이동 관련")]
    public float scatterRange = 0.6f; // 길이 2x2 사이즈이므로 0.6 정도로 주면 길 위에 적당히 퍼집니다.
    private Vector3 pathOffset; // 이 몬스터만의 고유한 삐딱한 경로 값

    private List<Vector3> waypoints;
    private int currentWaypointIndex = 0;

    private GameObject currentTarget;

    // 넉백 상태를 체크하는 변수 추가
    private bool isKnockedBack = false;

    private bool isDead = false;

    [Header("UI")]
    public Image hpBarFill; // 빨간색 체력바 이미지 (Filled)
    public Image cdBarFill; // 하얀색 쿨다운바 이미지 (Filled)
    public GameObject damageTextPrefab; // 데미지 텍스트 프리팹
    public GameObject hitEffectPrefab; //피격 이펙트 프리팹

    [Header("드랍 재화 설정")]
    public int dropCoinAmount = 10; // 확정 코인
    public int dropScrapAmount = 5; // 고철량
    [Range(0f, 1f)] public float scrapDropChance = 0.3f;
    private float currentScrapBonus = 0f;

    [Header("애니메이션")]
    protected Animator anim;


    protected float baseSpeed;
    protected float baseDamage;

    public class DebuffAuraData
    {
        public DebuffTower source;
        public DebuffTower.DebuffStatType type;
        public int level;
        public float amount;
    }
    private List<DebuffAuraData> activeDebuffs = new List<DebuffAuraData>();

    void Start()
    {
        baseSpeed = speed;
        baseDamage = attackDamage;
        maxHp = hp;
        if (cdBarFill != null) cdBarFill.fillAmount = 0f; // 시작할 때 쿨다운은 0

        //스폰될 때 자신만의 고유한 위치 오프셋을 랜덤으로 가집니다.
        pathOffset = new Vector3(Random.Range(-scatterRange, scatterRange), Random.Range(-scatterRange, scatterRange), 0f);
    
        anim = GetComponentInChildren<Animator>();
    }

    public void InitStats(float roundHp, float roundSpeed, float roundDamage)
    {
        // 원래 가지고 있던 변수명에 맞게 대입해줍니다.
        hp = roundHp;
        maxHp = roundHp;
        speed = roundSpeed;
        baseSpeed = roundSpeed; 
        attackDamage = roundDamage;
        baseDamage = roundDamage;
    }

    public void SetWaypoints(List<Vector3> pathWaypoints)
    {
        waypoints = new List<Vector3>(pathWaypoints);
    }

    void Update()
    {
        if (isKnockedBack || isDead) return;

        bool isMoving = false;
        bool isIdle = false;

        // 1. 타겟이 죽거나 사거리를 벗어나면 타겟 해제
        if (currentTarget != null)
        {
            float dist = Vector2.Distance(transform.position, currentTarget.transform.position);
            if (dist > attackRange || !currentTarget.activeInHierarchy)
            {
                currentTarget = null;
            }
        }

        // 2. 타겟이 없으면 내 사거리 안으로 들어온 아군 탐색
        if (currentTarget == null)
        {
            FindTarget();
        }

        if (currentTarget != null)
        {
            isIdle = true;

            // 3. 타겟(아군)을 바라보기
            RotateTowards(currentTarget.transform.position);

            // 전투 중: 공격 쿨다운 바(하양) 게이지 채우기
            float cdRatio = (Time.time - lastAttackTime) / attackCooldown;
            if (cdBarFill != null) cdBarFill.fillAmount = Mathf.Clamp01(cdRatio);

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Defender defender = currentTarget.GetComponent<Defender>();
                if (defender != null) defender.TakeDamage(attackDamage);
                
                lastAttackTime = Time.time;
                if (cdBarFill != null) cdBarFill.fillAmount = 0f; // 공격 후 초기화
            }
        }
        else
        {
            // 4. 이동 중: 쿨다운 바 숨기기
            if (cdBarFill != null) cdBarFill.fillAmount = 0f;

            if (waypoints != null && currentWaypointIndex < waypoints.Count)
            {
                isMoving = true;

                Vector3 targetPos = waypoints[currentWaypointIndex] + pathOffset;
                
                // 이동할 웨이포인트 방향 바라보기
                RotateTowards(targetPos);

                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                if (Vector3.Distance(transform.position, targetPos) < 0.1f)
                {
                    currentWaypointIndex++;
                }
            }
            else if (waypoints != null && currentWaypointIndex >= waypoints.Count)
            {
                // 기지에 도달했을 때: 플레이어 체력을 깎고 소멸
                isIdle = true; 
                if (!isDead) 
                {
                    isDead = true; 
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.TakeBaseDamage(hp);
                    }

                    if (SoundManager.Instance != null) SoundManager.Instance.PlayHPSound();

                    Die(); 
                }
            }
        }
        if (anim != null)
        {
            anim.SetBool("Run", isMoving);
            anim.SetBool("Idle", isIdle);
        }
    }

    // 사거리 내에서 가장 가까운 아군 탐색
    private void FindTarget()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        float closestDist = Mathf.Infinity;
        GameObject closest = null;

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Defender")) 
            {
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = col.gameObject;
                }
            }
        }
        currentTarget = closest;
    }

    // 목표를 향해 회전하고 UI는 고정하는 함수
    private void RotateTowards(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // HP바 고정
        Canvas uiCanvas = GetComponentInChildren<Canvas>();
        if (uiCanvas != null)
        {
            uiCanvas.transform.rotation = Quaternion.identity;
        }
    }

    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(KnockbackRoutine(direction, force, duration));
        }
    }

    // 코루틴을 통해 일정 시간 동안 적을 뒤로 밀어냄
    private System.Collections.IEnumerator KnockbackRoutine(Vector3 direction, float force, float duration)
    {
        isKnockedBack = true;
        float elapsed = 0f;
        
        // 유닛의 몸통 크기(반지름)를 설정하여 벽을 미리 감지하도록 합니다.
        float bodyRadius = 0.8f; 

        while (elapsed < duration)
        {
            Vector3 step = direction * force * Time.deltaTime;
            Vector3 nextPos = transform.position + step;

            // 다음 이동할 위치보다 유닛의 몸통 크기(bodyRadius)만큼 더 뒤쪽을 미리 검사합니다.
            Vector3 checkPos = nextPos + (direction * bodyRadius);

            if (RoundManager.Instance != null && RoundManager.Instance.IsOnPath(checkPos))
            {
                // 뒤쪽이 길이라면 정상적으로 밀려납니다.
                transform.position = nextPos;
            }
            else
            {
                // 벽(건설 구역)에 닿았다면! 
                // 더 이상 뒤로 밀리지 않도록 넉백 힘(force)을 0으로 만듭니다. (남은 시간 동안 스턴만 걸림)
                force = 0f; 
            }

            elapsed += Time.deltaTime;
            yield return null; 
        }

        isKnockedBack = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        hp -= damage;


        SoundManager.Instance.PlayHitSound();
        
        // 체력바 UI 업데이트 (Fill Amount는 0~1 사이의 값)
        if (hpBarFill != null) hpBarFill.fillAmount = Mathf.Clamp01(hp / maxHp);
        
        // 데미지 팝업 생성
        if (damageTextPrefab != null)
        {
            GameObject txtObj = Instantiate(damageTextPrefab, transform.position + new Vector3(0, 0.5f, -1f), Quaternion.identity);
            txtObj.GetComponent<DamageText>().Setup(damage);
        }

        //피격 이펙트 생성
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        if (anim != null) anim.SetTrigger("Damage");

        if (hp <= 0) 
        {
            isDead = true;
            // 사망 시 재화 지급 로직
            float finalScrapChance = scrapDropChance + currentScrapBonus;
            int gotScrap = (Random.value <= finalScrapChance) ? dropScrapAmount : 0;
            
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddResources(dropCoinAmount, gotScrap);
            }

            if (damageTextPrefab != null)
            {
                // 코인은 노란색
                GameObject coinTxtObj = Instantiate(damageTextPrefab, transform.position + new Vector3(0, 0.5f, -1f), Quaternion.identity);
                coinTxtObj.GetComponent<DamageText>().Setup(dropCoinAmount, Color.yellow, "+");

                // 고철은 회색
                if (gotScrap > 0)
                {
                    GameObject scrapTxtObj = Instantiate(damageTextPrefab, transform.position + new Vector3(0, 1.0f, -1f), Quaternion.identity);
                    scrapTxtObj.GetComponent<DamageText>().Setup(gotScrap, Color.gray, "+");
                }
            }

            Die();
        }
    }

    private void Die()
    {
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.OnEnemyDestroyed();
        }
        Destroy(gameObject);
    }



    //디버프 관리 함수들 추가
    public void AddDebuff(DebuffTower source, DebuffTower.DebuffStatType type, int level, float amount)
    {
        activeDebuffs.RemoveAll(d => d.source == source);
        activeDebuffs.Add(new DebuffAuraData { source = source, type = type, level = level, amount = amount });
        RecalculateDebuffs();
    }

    public void RemoveDebuff(DebuffTower source)
    {
        activeDebuffs.RemoveAll(d => d.source == source);
        RecalculateDebuffs();
    }

    private void RecalculateDebuffs()
    {
        // 1. 베이스 스탯으로 초기화
        speed = baseSpeed;
        attackDamage = baseDamage; 

        // 2. 종류별 최고 레벨 디버프 찾기
        int maxSpdLevel = 0; float spdDecreaseAmount = 0f;
        int maxAtkLevel = 0; float atkDecreaseAmount = 0f;
        int maxScrapLevel = 0; float scrapBonusAmount = 0f;

        foreach (var debuff in activeDebuffs)
        {
            if (debuff.type == DebuffTower.DebuffStatType.SpeedDecrease)
            {
                if (debuff.level > maxSpdLevel || (debuff.level == maxSpdLevel && debuff.amount > spdDecreaseAmount))
                {
                    maxSpdLevel = debuff.level;
                    spdDecreaseAmount = debuff.amount;
                }
            }
            else if (debuff.type == DebuffTower.DebuffStatType.AttackDecrease)
            {
                if (debuff.level > maxAtkLevel || (debuff.level == maxAtkLevel && debuff.amount > atkDecreaseAmount))
                {
                    maxAtkLevel = debuff.level;
                    atkDecreaseAmount = debuff.amount;
                }
            }
            else if (debuff.type == DebuffTower.DebuffStatType.ScrapDropIncrease)
            {
                if (debuff.level > maxScrapLevel || (debuff.level == maxScrapLevel && debuff.amount > scrapBonusAmount))
                {
                    maxScrapLevel = debuff.level;
                    scrapBonusAmount = debuff.amount;
                }
            }
        }

        if (spdDecreaseAmount > 0) speed = baseSpeed * (1f - spdDecreaseAmount);
        if (atkDecreaseAmount > 0) attackDamage = baseDamage * (1f - atkDecreaseAmount); 

        currentScrapBonus = scrapBonusAmount;
    }

}
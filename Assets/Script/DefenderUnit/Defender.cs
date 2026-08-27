using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Defender : MonoBehaviour
{
    [Header("무기 + 종료 따른 스탯")]
    protected float baseSpeed;
    protected float baseAttackDamage;
    protected float baseAttackCooldown;
    protected float baseAttackRange;
    protected float baseKnockbackForce;

    [Header("적용 스탯")]
    public float speed = 2f;
    public float hp = 50f;
    public float maxHp; 
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    public float attackRange = 1.5f;
    public float knockbackForce = 1f;
    
    protected float lastAttackTime;
    protected List<Vector3> waypoints; 
    protected int currentWaypointIndex; 
    protected GameObject currentTarget; 

    [Header("무기")]
    public InventoryManager.WeaponStat myWeaponStat;

    [Header("UI")]
    public Image hpBarFill; 
    public Image cdBarFill; 
    public GameObject damageTextPrefab; 

    [Header("애니메이션")]
    public Animator anim;

    [Header("타입별 애니메이터 컨트롤러")]
    public RuntimeAnimatorController normalAnimController;
    public RuntimeAnimatorController tankAnimController;
    public RuntimeAnimatorController fasterAnimController;

    // 사거리 표시용 라인 렌더러
    private LineRenderer rangeCircle;


    //버프 타워가 주는 버프를 관리하기 위한 클래스
    public class AuraData
    {
        public BuffTower source;
        public BuffTower.BuffStatType type;
        public int level;
        public float amount;
    }
    private List<AuraData> activeAuras = new List<AuraData>();


    void Start()
    {
        maxHp = hp;
        if (cdBarFill != null) cdBarFill.fillAmount = 0f; 

        if (rangeCircle == null)
        {
            SetupRangeCircle();
        }

        anim = GetComponentInChildren<Animator>();
    }

    // 사거리를 그리는 함수
    private void SetupRangeCircle()
    {
        if(rangeCircle != null)
            return;
        rangeCircle = gameObject.AddComponent<LineRenderer>();
        rangeCircle.startWidth = 0.05f;
        rangeCircle.endWidth = 0.05f;
        rangeCircle.positionCount = 51; // 부드러운 원을 위해 50개의 점 사용
        rangeCircle.useWorldSpace = false; // 내 위치를 기준으로 그림
        rangeCircle.loop = true;

        // 머티리얼 및 색상 설정 (반투명한 빨간색)
        rangeCircle.material = new Material(Shader.Find("Sprites/Default"));
        rangeCircle.startColor = new Color(1f, 0f, 0f, 1f);
        rangeCircle.endColor = new Color(1f, 0f, 0f, 1f);
        rangeCircle.sortingOrder = 5; // 맵(타일)보다 위에 그려지도록 설정

        UpdateRangeCircleRadius();
        rangeCircle.enabled = false; 
    }

    public void SetUnitVisuals(PreviewDropZone.UnitType type)
    {
        // anim 변수가 아직 할당되지 않았다면 찾아옵니다.
        if (anim == null) anim = GetComponentInChildren<Animator>();

        if (anim != null)
        {
            switch (type)
            {
                case PreviewDropZone.UnitType.Normal:
                    if (normalAnimController != null) anim.runtimeAnimatorController = normalAnimController;
                    break;
                case PreviewDropZone.UnitType.Tank:
                    if (tankAnimController != null) anim.runtimeAnimatorController = tankAnimController;
                    break;
                case PreviewDropZone.UnitType.Faster:
                    if (fasterAnimController != null) anim.runtimeAnimatorController = fasterAnimController;
                    break;
            }
        }
    }


    public void SetWaypoints(List<Vector3> pathWaypoints)
    {
        waypoints = new List<Vector3>(pathWaypoints);
        waypoints.Reverse(); 
        currentWaypointIndex = 0;
    }

    void Update()
    {
        //게임 매니저의 디버그 모드 상태에 따라 사거리 원을 켜고 끕니다
        if (GameManager.Instance != null && rangeCircle != null)
        {
            rangeCircle.enabled = GameManager.Instance.isDebugMode;
        }

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

        // 2. 타겟이 없으면 내 사거리 안으로 들어온 적을 탐색
        if (currentTarget == null)
        {
            FindTarget();
        }

        if (currentTarget != null)
        {
            isIdle = true;
            // 3. 타겟(적)을 바라보기
            RotateTowards(currentTarget.transform.position);

            // 전투 중
            float cdRatio = (Time.time - lastAttackTime) / attackCooldown;
            if (cdBarFill != null) cdBarFill.fillAmount = Mathf.Clamp01(cdRatio);

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PerformAttack(); 

                if (anim != null) anim.SetTrigger("Attack");
                
                lastAttackTime = Time.time;
                if (cdBarFill != null) cdBarFill.fillAmount = 0f;
            }
        }
        else
        {
            // 4. 이동 중
            if (cdBarFill != null) cdBarFill.fillAmount = 0f;

            if (waypoints != null && currentWaypointIndex < waypoints.Count)
            {
                isMoving = true;
                
                Vector3 targetPos = waypoints[currentWaypointIndex];
                
                // 이동할 웨이포인트 방향 바라보기
                RotateTowards(targetPos);

                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                if (Vector3.Distance(transform.position, targetPos) < 0.1f)
                {
                    currentWaypointIndex++;
                }
            }
            else
            {
                isIdle = true;
            }
        }

        if (anim != null)
        {
            anim.SetBool("Run", isMoving);
            anim.SetBool("Idle", isIdle); 
        }
    }

    // 사거리 내에서 가장 가까운 적 탐색 함수
    protected void FindTarget()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        float closestDist = Mathf.Infinity;
        GameObject closest = null;

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy"))
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

    protected void RotateTowards(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // 몸체가 회전할 때 HP바(Canvas)가 같이 뒤집히지 않도록 각도 0으로 고정
        Canvas uiCanvas = GetComponentInChildren<Canvas>();
        if (uiCanvas != null)
        {
            uiCanvas.transform.rotation = Quaternion.identity;
        }
    }

    // 자식 클래스(리볼버, 샷건)에서 이 함수를 덮어씌워(Override) 자신만의 공격을 구현합니다.
    protected virtual void PerformAttack()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayBulletSound();
        // 기본 디펜더의 근접 공격 로직
        Enemy enemy = currentTarget.GetComponent<Enemy>();
        if (enemy != null) enemy.TakeDamage(attackDamage);
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;

        if (hpBarFill != null) hpBarFill.fillAmount = Mathf.Clamp01(hp / maxHp);
        
        if (damageTextPrefab != null)
        {
            GameObject txtObj = Instantiate(damageTextPrefab, transform.position + new Vector3(0, 0.5f, -1f), Quaternion.identity);
            txtObj.GetComponent<DamageText>().Setup(damage);
        }

        if (hp <= 0) 
        {
            // 사망 시 무기가 인벤토리로 되돌아갑니다!
            if (InventoryManager.Instance != null && myWeaponStat != null)
            {
                InventoryManager.Instance.AddExistingWeapon(myWeaponStat);
            }
            Destroy(gameObject);
        }
    }

    






// GameManager에서 소환 직후 기준 스탯을 세팅할 때 호출합니다.
    public void SetupBaseStats(float spd, float health, float dmg, float cd, float range, float kb)
    {
        baseSpeed = spd; speed = spd;
        maxHp = health; hp = health;
        baseAttackDamage = dmg; attackDamage = dmg;
        baseAttackCooldown = cd; attackCooldown = cd;
        baseAttackRange = range; attackRange = range;
        baseKnockbackForce = kb; knockbackForce = kb;

        if (hpBarFill != null) hpBarFill.fillAmount = 1f;
        UpdateRangeCircleRadius();
    }

    // 타워 사거리에 들어왔을 때 호출
    public void AddAura(BuffTower source, BuffTower.BuffStatType type, int level, float amount)
    {
        // 동일한 소스의 오라가 있다면 지우고 갱신합니다.
        activeAuras.RemoveAll(a => a.source == source);
        activeAuras.Add(new AuraData { source = source, type = type, level = level, amount = amount });
        RecalculateStats();
    }

    // 타워 사거리에서 나갔을 때 호출
    public void RemoveAura(BuffTower source)
    {
        activeAuras.RemoveAll(a => a.source == source);
        RecalculateStats();
    }

    // 현재 받고 있는 오라들을 비교하여 최상위 레벨만 적용합니다.
    private void RecalculateStats()
    {
        // 1. 계산 전 베이스 스탯으로 초기화
        attackRange = baseAttackRange;
        attackDamage = baseAttackDamage;
        attackCooldown = baseAttackCooldown;

        // 2. 종류별 최고 레벨 버프 찾기
        int maxRangeLevel = 0; float rangeBonus = 0f;
        int maxDmgLevel = 0; float dmgBonus = 0f;
        int maxSpdLevel = 0; float spdBonus = 0f;

        foreach (var aura in activeAuras)
        {
            if (aura.type == BuffTower.BuffStatType.Range)
            {
                if (aura.level > maxRangeLevel || (aura.level == maxRangeLevel && aura.amount > rangeBonus)) 
                {
                    maxRangeLevel = aura.level;
                    rangeBonus = aura.amount;
                }
            }
            else if (aura.type == BuffTower.BuffStatType.AttackDamage)
            {
                if (aura.level > maxDmgLevel || (aura.level == maxDmgLevel && aura.amount > dmgBonus))
                {
                    maxDmgLevel = aura.level;
                    dmgBonus = aura.amount;
                }
            }
            else if (aura.type == BuffTower.BuffStatType.AttackSpeed)
            {
                if (aura.level > maxSpdLevel || (aura.level == maxSpdLevel && aura.amount > spdBonus))
                {
                    maxSpdLevel = aura.level;
                    spdBonus = aura.amount;
                }
            }
        }

        // 3. 최종 스탯에 산출된 최고 보너스만 합산
        attackRange += rangeBonus;
        attackDamage += dmgBonus;
        if (spdBonus > 0) attackCooldown /= (1f + spdBonus); 

        UpdateRangeCircleRadius(); 
    }

    public void Heal(float amount)
    {
        hp += amount;
        if (hp > maxHp) hp = maxHp;
        if (hpBarFill != null) hpBarFill.fillAmount = Mathf.Clamp01(hp / maxHp);

        if (damageTextPrefab != null)
        {
            GameObject txtObj = Instantiate(damageTextPrefab, transform.position + new Vector3(0, 0.5f, -1f), Quaternion.identity);
            txtObj.GetComponent<DamageText>().Setup(amount, Color.green, "+");
        }
    }


    private void UpdateRangeCircleRadius()
    {
        if (rangeCircle == null) return;

        float scaleX = Mathf.Abs(transform.localScale.x);
        float scaleY = Mathf.Abs(transform.localScale.y);

        float adjustedRangeX = attackRange / scaleX;
        float adjustedRangeY = attackRange / scaleY;

        float angle = 0f;
        for (int i = 0; i < 51; i++)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * adjustedRangeX;
            float y = Mathf.Sin(Mathf.Deg2Rad * angle) * adjustedRangeY;
            rangeCircle.SetPosition(i, new Vector3(x, y, 0));
            angle += (360f / 50f);
        }
    }

}
using UnityEngine;
using System.Collections; 
using UnityEngine.EventSystems;
public class TurretTower : Tower, IDropHandler
{
    [Header("무기 장착 상태")]
    public bool hasWeapon = false;
    public InventoryManager.WeaponStat equippedWeapon;

    [Header("발사체 프리팹 (각 무기 유닛과 동일하게 연결)")]
    public GameObject bulletPrefab;  // 리볼버, 라이플 공용
    public GameObject blastPrefab;   // 샷건 폭발 판정용
    public GameObject rocketPrefab;  // 바주카 폭발 로켓용

    [Header("라이플 연사 설정")]
    public int burstCount = 6;       
    public float burstInterval = 0.1f; 

    protected override void Start()
    {
        base.Start(); 
        if (cdBarFill != null) cdBarFill.fillAmount = 0f;
    }

    protected override void Update()
    {
        if (!hasWeapon) return;
        base.Update();
    }
    
    //여기다가 Weapon떨구기
    public void OnDrop(PointerEventData eventData)
    {
        // 무기가 없는 빈 타워일 때만 작동
        if (!hasWeapon && eventData.pointerDrag != null)
        {
            // 드롭된 오브젝트가 WeaponDrag 스크립트를 가지고 있는지 확인
            WeaponDrag droppedWeapon = eventData.pointerDrag.GetComponent<WeaponDrag>();
            
            if (droppedWeapon != null && droppedWeapon.myWeaponStat != null)
            {
                // 무기 장착 실행
                EquipWeapon(droppedWeapon.myWeaponStat);
                
                // 인벤토리에서 무기 제거 (제거 시 UI도 갱신되며 드래그하던 아이콘은 자동 파괴됨)
                InventoryManager.Instance.RemoveWeapon(droppedWeapon.myWeaponStat);
            }
        }
    }

    public void EquipWeapon(InventoryManager.WeaponStat weapon)
    {
        equippedWeapon = weapon;
        hasWeapon = true;

        attackDamage = weapon.damage;
        attackCooldown = 1f / weapon.attackSpeed; 
        attackRange = weapon.attackRange;
        
        UpdateRangeCircle();
        Debug.Log($"거치용 타워에 {weapon.type} 장착 완료! (DMG: {attackDamage}, RNG: {attackRange})");
    }

    private void UpdateRangeCircle()
    {
        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr != null)
        {
            float angle = 0f;
            for (int i = 0; i < 51; i++)
            {
                float x = Mathf.Cos(Mathf.Deg2Rad * angle) * attackRange;
                float y = Mathf.Sin(Mathf.Deg2Rad * angle) * attackRange;
                lr.SetPosition(i, new Vector3(x, y, 0));
                angle += (360f / 50f);
            }
        }
    }

    protected override void PerformAction()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance && distanceToEnemy <= attackRange)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
        {
            FireWeapon(nearestEnemy);
            lastAttackTime = Time.time;
            if (cdBarFill != null) cdBarFill.fillAmount = 0f;
        }
    }

    private void FireWeapon(GameObject target)
    {
        if (equippedWeapon == null || target == null) return;

        Vector3 direction = (target.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        switch (equippedWeapon.type)
        {
            case InventoryManager.WeaponType.Revolver:
                if (bulletPrefab != null)
                {
                    GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                    bulletObj.transform.rotation = Quaternion.Euler(0, 0, angle);
                    bulletObj.GetComponent<Bullet>().Setup(direction, attackDamage, equippedWeapon.knockback);
                }
                break;
            
            case InventoryManager.WeaponType.Shotgun:
                if (blastPrefab != null)
                {
                    // 샷건 방식을 그대로 채용: 살짝 앞쪽에 폭발 판정 생성
                    Vector3 spawnPos = transform.position + direction * 0.5f; 
                    GameObject blastObj = Instantiate(blastPrefab, spawnPos, Quaternion.Euler(0, 0, angle));
                    blastObj.GetComponent<ShotgunBlast>().Setup(attackDamage);
                }
                break;

            case InventoryManager.WeaponType.Rifle:
                // 코루틴 연사 호출
                StartCoroutine(FireBurst(target));
                break;

            case InventoryManager.WeaponType.Bazooka:
                if (rocketPrefab != null)
                {
                    // 바주카 방식을 그대로 채용: 로켓 발사
                    GameObject rocketObj = Instantiate(rocketPrefab, transform.position, Quaternion.identity);
                    rocketObj.transform.rotation = Quaternion.Euler(0, 0, angle);
                    rocketObj.GetComponent<Rocket>().Setup(direction, attackDamage, equippedWeapon.knockback);
                }
                break;
        }
    }

    private IEnumerator FireBurst(GameObject target)
    {
        for (int i = 0; i < burstCount; i++)
        {
            // 도중에 적이 죽었거나 없으면 중단
            if (target == null || !target.activeInHierarchy) break;

            if (bulletPrefab != null)
            {
                Vector3 direction = (target.transform.position - transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                
                GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                bulletObj.transform.rotation = Quaternion.Euler(0, 0, angle);
                bulletObj.GetComponent<Bullet>().Setup(direction, attackDamage, equippedWeapon.knockback);
            }
            
            yield return new WaitForSeconds(burstInterval);
        }
    }
}
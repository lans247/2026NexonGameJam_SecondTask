using UnityEngine;

public class ShotgunDefender : Defender
{
    [Header("샷건 전용 설정")]
    public GameObject blastPrefab; // 팍 터지는 판정용 프리팹

    protected override void PerformAttack()
    {
        if (blastPrefab != null && currentTarget != null)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayBulletSound();
            // 적이 있는 방향으로 살짝 앞쪽에 폭발 판정을 생성합니다.
            Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
            
            //샷건 이펙트 회전값 적용 (부채꼴 이펙트 등이 방향에 맞게 터지도록)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            GameObject blastObj = Instantiate(blastPrefab, transform.position, Quaternion.Euler(0, 0, angle));
            blastObj.GetComponent<ShotgunBlast>().Setup(attackDamage, attackRange, direction);
        }
    }
}
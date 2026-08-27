using UnityEngine;

public class BazookaDefender : Defender
{
    [Header("바주카 전용 설정")]
    public GameObject rocketPrefab;

    protected override void PerformAttack()
    {
        if (rocketPrefab != null && currentTarget != null)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayBulletSound();
            // 로켓 발사
            GameObject rocketObj = Instantiate(rocketPrefab, transform.position, Quaternion.identity);
            
            // 방향 계산
            Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // Y축 기준 스프라이트
            rocketObj.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            // 데미지 전달
            rocketObj.GetComponent<Rocket>().Setup(direction, attackDamage, knockbackForce);
        }
    }
}
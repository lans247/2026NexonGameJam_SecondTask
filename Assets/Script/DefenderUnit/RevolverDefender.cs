using UnityEngine;

// Defender 클래스를 상속받습니다.
public class RevolverDefender : Defender
{
    [Header("리볼버 전용 설정")]
    public GameObject bulletPrefab;

    // 부모의 공격 로직을 덮어씁니다.
    protected override void PerformAttack()
    {
        if (bulletPrefab != null && currentTarget != null)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayBulletSound();
            // 총알을 생성합니다.
            GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            
            // 적을 향하는 방향을 계산합니다.
            Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
            
            //총알의 회전값 적용 (총알이 날아가는 방향을 바라보게 함)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            bulletObj.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            // 총알에 방향과 데미지 정보를 넘겨줍니다.
            bulletObj.GetComponent<Bullet>().Setup(direction, attackDamage, knockbackForce);
        }
    }
}
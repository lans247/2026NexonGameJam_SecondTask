using UnityEngine;
using System.Collections; // 코루틴(IEnumerator)을 사용하기 위해 필수!

public class RifleDefender : Defender
{
    [Header("라이플 전용 설정")]
    public GameObject bulletPrefab;
    public int burstCount = 6;       // 한 번에 쏠 총알 개수
    public float burstInterval = 0.1f; // 연사 간격 (0.1초마다 1발씩)

    protected override void PerformAttack()
    {
        if (bulletPrefab != null && currentTarget != null)
        {
            // 코루틴을 실행하여 연사를 시작합니다.
            StartCoroutine(FireBurst());
        }
    }

    private IEnumerator FireBurst()
    {
        for (int i = 0; i < burstCount; i++)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayBulletSound();
            
            // 연사 도중에 적이 죽었거나 범위를 벗어났다면 사격을 중지합니다.
            if (currentTarget == null) 
                break;

            // 1. 총알 생성
            GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            
            // 2. 방향 및 회전 계산 (Y축 위쪽 기준 스프라이트 보정)
            Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            bulletObj.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            // 3. 데미지 적용
            bulletObj.GetComponent<Bullet>().Setup(direction, attackDamage, knockbackForce);

            // 4. 다음 탄환 발사까지 burstInterval(0.1초) 만큼 대기합니다.
            yield return new WaitForSeconds(burstInterval);
        }
    }
}
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public float speed = 8f;
    public float explosionRadius = 2.5f; // 광역 폭발 범위
    public float knockbackForce;    // 밀어내는 힘
    public float knockbackDuration = 0.3f; // 스턴 시간
    public GameObject explosionEffectPrefab; // 터질 때 나오는 이펙트(Pop 등을 쓰셔도 됩니다)

    private Vector3 moveDirection;
    private float damage;

    private bool hasExploded = false;

    public void Setup(Vector3 dir, float dmg, float kb)
    {
        moveDirection = dir;
        damage = dmg;
        knockbackForce = kb;
        Destroy(gameObject, 5f); // 허공에 날아가면 5초 뒤 자동 삭제
    }

    void Update()
    {
        // 지정된 방향으로 일직선으로 날아감
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;
        if (other.CompareTag("Enemy"))
        {
            Explode();
        }
    }

    void Explode()
    {
        hasExploded = true;
        // 1. 시각적 이펙트 생성
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2. 물리 엔진을 사용해 폭발 반경 안의 모든 콜라이더(적) 찾기
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    // 광역 데미지 주기
                    enemy.TakeDamage(damage);
                    
                    //폭발 중심(로켓)에서 적(enemy)을 향하는 넉백 방향 계산
                    Vector3 knockbackDir = (enemy.transform.position - transform.position).normalized;
                    
                    // 넉백 적용
                    enemy.ApplyKnockback(knockbackDir, knockbackForce, knockbackDuration);
                }
            }
        }

        // 3. 로켓 자신은 폭발 후 파괴
        Destroy(gameObject);
    }

    // 에디터에서 폭발 범위(사거리)를 노란 원으로 보여줍니다.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
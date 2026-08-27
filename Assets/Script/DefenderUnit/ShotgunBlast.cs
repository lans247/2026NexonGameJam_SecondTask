using UnityEngine;

public class ShotgunBlast : MonoBehaviour
{
    private float damage;

    [Header("넉백 설정")]
    public float knockbackForce = 10f;      // 샷건은 밀어내는 힘이 강함!
    public float knockbackDuration = 0.15f; 

    public void Setup(float dmg, float attackRange, Vector3 direction)
    {
        damage = dmg;

        transform.localScale = new Vector3(attackRange, attackRange, 1f);

        Destroy(gameObject, 0.1f); 
    }

    public void Setup(float dmg)
    {
        damage = dmg;
        Destroy(gameObject, 0.1f); 
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            //범위 내 모든 적에게 데미지
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null) 
            {
                enemy.TakeDamage(damage);
                
                // 폭발 중심(샷건)에서 적을 향하는 방향을 계산하여 밀어내기
                Vector3 pushDir = (other.transform.position - transform.position).normalized;
                enemy.ApplyKnockback(pushDir, knockbackForce, knockbackDuration);
            }
        }
    }
}
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    
    [Header("넉백 설정")]
    public float knockbackForce;       // 밀어내는 힘
    public float knockbackDuration = 0.3f;  // 밀려나는 시간(스턴 시간)

    private Vector3 moveDirection;
    private float damage;

    public void Setup(Vector3 dir, float dmg, float kb)
    {
        moveDirection = dir;
        damage = dmg;
        knockbackForce = kb;
        Destroy(gameObject, 3f); 
    }

    void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy")) 
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null) 
            {
                enemy.TakeDamage(damage);
                
                //총알이 날아가는 방향(moveDirection)으로 적을 넉백시킵니다.
                enemy.ApplyKnockback(moveDirection, knockbackForce, knockbackDuration);
            }
            
            Destroy(gameObject); 
        }
    }
}
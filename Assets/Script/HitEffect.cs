using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [Header("이펙트 설정")]
    public float destroyTime = 0.15f; // 0.15초 뒤 사라짐

    void Start()
    {
        //살짝 랜덤한 위치
        float randomX = Random.Range(-0.1f, 0.1f);
        float randomY = Random.Range(-0.1f, 0.1f);
        transform.position = new Vector2(transform.position.x + randomX, transform.position.y + randomY);

        // 살짝 랜덤한 크기로 띄워서 타격감을 더합니다.
        float randomScale = Random.Range(0.8f, 1.3f);
        transform.localScale = new Vector3(randomScale, randomScale, 1f);

        // 이펙트가 매번 같은 방향이면 심심하므로 회전값도 랜덤하게 줍니다.
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        // 이펙트가 캐릭터나 UI 뒤에 가려지지 않게 순서를 맨 앞으로 당깁니다.
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 10;
        }

        // 지정된 시간(0.15초) 뒤에 메모리에서 삭제합니다.
        Destroy(gameObject, destroyTime);
    }
}
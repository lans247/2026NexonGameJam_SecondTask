using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Tower : MonoBehaviour
{
    public float attackRange = 3f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    
    // 자식 클래스(BuffTower 등)에서 쿨다운 변수에 접근할 수 있도록 protected로 변경합니다.
    protected float lastAttackTime;

    private LineRenderer rangeCircle;

    [Header("UI 연결")]
    public Image cdBarFill;
    public Image mainImage;

    [Header("건설 연출 설정")]
    public float buildDuration = 0.5f; // 건설에 걸리는 시간
    protected bool isBuilding = true;

    // 자식 클래스에서 필요 시 Start를 덮어쓸 수 있도록 virtual 추가
    protected virtual void Start()
    {
        SetupRangeCircle();

        if (cdBarFill != null) cdBarFill.fillAmount = 0f;
        if (mainImage != null)
        {
            // 코드로 직접 Filled 타입 설정 (아래에서 위로 차오르게)
            mainImage.type = Image.Type.Filled;
            mainImage.fillMethod = Image.FillMethod.Vertical;
            mainImage.fillOrigin = (int)Image.OriginVertical.Bottom;
            mainImage.fillAmount = 0f;
            
            StartCoroutine(BuildRoutine());
        }
        else
        {
            // 이미지가 할당되지 않았다면 즉시 건설 완료 처리
            isBuilding = false; 
        }
    }

    private IEnumerator BuildRoutine()
    {
        isBuilding = true;
        float elapsed = 0f;
        
        while (elapsed < buildDuration)
        {
            elapsed += Time.deltaTime;
            if (mainImage != null)
            {
                mainImage.fillAmount = Mathf.Clamp01(elapsed / buildDuration);
            }
            yield return null; // 다음 프레임까지 대기
        }
        
        if (mainImage != null) mainImage.fillAmount = 1f;
        isBuilding = false; // 건설 완료
    }

    private void SetupRangeCircle()
    {
        rangeCircle = gameObject.AddComponent<LineRenderer>();
        rangeCircle.startWidth = 0.05f;
        rangeCircle.endWidth = 0.05f;
        rangeCircle.positionCount = 51; 
        rangeCircle.useWorldSpace = false;
        rangeCircle.loop = true;

        rangeCircle.material = new Material(Shader.Find("Sprites/Default"));
        rangeCircle.startColor = new Color(1f, 0f, 0f, 1f);
        rangeCircle.endColor = new Color(1f, 0f, 0f, 1f);
        rangeCircle.sortingOrder = 5;

        float angle = 0f;
        for (int i = 0; i < 51; i++)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * attackRange;
            float y = Mathf.Sin(Mathf.Deg2Rad * angle) * attackRange;
            rangeCircle.SetPosition(i, new Vector3(x, y, 0));
            angle += (360f / 50f);
        }

        rangeCircle.enabled = false;
    }

    // 자식 클래스에서 base.Update()를 호출하고 기능을 추가할 수 있도록 virtual 설정
    protected virtual void Update()
    {
        if (isBuilding) return;
        
        if (GameManager.Instance != null && rangeCircle != null)
        {
            rangeCircle.enabled = GameManager.Instance.isDebugMode;
        }

        float cdRatio = (Time.time - lastAttackTime) / attackCooldown;
        if (cdBarFill != null) cdBarFill.fillAmount = Mathf.Clamp01(cdRatio);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PerformAction(); // 직접 공격하는 대신 행동 수행 함수를 호출합니다.
        }
    }

    // 자식 클래스에서 이 함수를 덮어씌워(override) 공격 대신 버프/힐을 수행하게 만듭니다.
    protected virtual void PerformAction()
    {
        FindAndAttackTarget();
    }

    // 기본 공격 로직 (자식 클래스에서도 쓸 수 있게 protected 설정)
    protected void FindAndAttackTarget()
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
            nearestEnemy.GetComponent<Enemy>().TakeDamage(attackDamage);
            lastAttackTime = Time.time;
            
            if (cdBarFill != null) cdBarFill.fillAmount = 0f;
            
            Debug.DrawLine(transform.position, nearestEnemy.transform.position, Color.yellow, 0.1f);
        }
    }
}
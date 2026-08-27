using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro; // 텍스트 UI 제어용

public class TowerDragIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // 이제 인스펙터에서 고정하지 않고 스크립트가 스스로 랜덤 결정합니다.
    public TowerManager.TowerType currentType;
    public int currentLevel;
    
    [Header("UI 연결")]
    public Image cooldownOverlay; 
    public TextMeshProUGUI typeText;  // 타워 종류를 표시할 텍스트
    public TextMeshProUGUI levelText; // 타워 레벨을 표시할 텍스트

    [Header("아이콘 이미지 설정")]
    public Image previewImage; 
    public Sprite buffRangeSprite;
    public Sprite buffHpSprite;
    public Sprite buffSpeedSprite;
    public Sprite buffDamageSprite;
    public Sprite debuffSpeedSprite;
    public Sprite debuffAttackSprite;
    public Sprite debuffDoTSprite;
    public Sprite turretSprite;
    public Sprite debuffScrapSprite;

    [Header("쿨타임 설정")]
    public float cooldownTime = 5f; 
    private float currentCooldown = 0f;

    private Vector3 startPosition;
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    
    private bool isDragging = false;
    private bool isCoolingDown = false; // 쿨타임 진행 상태를 추적합니다.

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        // 게임 시작 시 첫 랜덤 타워를 배정받습니다.
        RollNewTower();
    }

    void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = currentCooldown / cooldownTime;
            }

            // 쿨타임이 막 0초가 되어 끝난 순간, 새로운 타워로 갱신합니다.
            if (currentCooldown <= 0f && isCoolingDown)
            {
                isCoolingDown = false;
                RollNewTower();
            }
        }
    }

    // 새로운 종류와 레벨을 무작위로 뽑고 UI를 업데이트하는 함수
    private void RollNewTower()
    {
        currentType = TowerManager.Instance.GetRandomTowerType();
        currentLevel = Random.Range(1, 4); // 1, 2, 3 중 하나를 무작위로 뽑습니다.

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (previewImage != null)
        {
            switch (currentType)
            {
                case TowerManager.TowerType.BuffRange: previewImage.sprite = buffRangeSprite; break;
                case TowerManager.TowerType.BuffHP: previewImage.sprite = buffHpSprite; break;
                case TowerManager.TowerType.BuffAttackSpeed: previewImage.sprite = buffSpeedSprite; break;
                case TowerManager.TowerType.BuffDamage: previewImage.sprite = buffDamageSprite; break;
                case TowerManager.TowerType.DebuffSpeed: previewImage.sprite = debuffSpeedSprite; break;
                case TowerManager.TowerType.DebuffAttack: previewImage.sprite = debuffAttackSprite; break;
                case TowerManager.TowerType.DebuffDoT: previewImage.sprite = debuffDoTSprite; break;
                case TowerManager.TowerType.DebuffScrap: previewImage.sprite = debuffScrapSprite; break;
                case TowerManager.TowerType.Turret: previewImage.sprite = turretSprite; break;
            }
        }

        if (currentType == TowerManager.TowerType.Turret)
        {
            if (typeText != null) typeText.text = "Turret";
            if (levelText != null) levelText.text = ""; 
            return; 
        }

        if (typeText != null)
        {
            string typeName = currentType.ToString();
            typeName = typeName.Replace("Buff", "").Replace("Debuff", "");
            typeText.text = typeName; 
        }
        
        if (levelText != null)
        {
            string arrowSymbol = currentType.ToString().Contains("Debuff") ? "▼" : "▲"; 
            string levelString = "";
            for (int i = 0; i < currentLevel; i++)
            {
                levelString += arrowSymbol;
            }
            levelText.text = levelString;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentCooldown > 0) return; 

        isDragging = true;
        startPosition = transform.position;
        originalParent = transform.parent;
        
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        
        // 건설 시도 시, 결정된 종류와 레벨을 Manager로 전달합니다.
        bool success = TowerManager.Instance.TryBuildTower(currentType, currentLevel, Input.mousePosition);

        // 건설에 성공했다면 쿨타임을 시작합니다.
        if (success)
        {
            currentCooldown = cooldownTime;
            isCoolingDown = true;
        }

        transform.SetParent(originalParent);
        transform.position = startPosition;
    }
}
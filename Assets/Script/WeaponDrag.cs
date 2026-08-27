using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public InventoryManager.WeaponStat myWeaponStat;
    
    private Vector3 startPosition;
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    
    private bool isDragging = false;
    private float pointerDownTime; // 마우스를 누른 시간 기록
    public TextMeshProUGUI LevelText;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) 
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        LevelText.text = myWeaponStat.totalLevel.ToString();
    }

    public void UpdateLevelUI()
    {
        if (LevelText != null && myWeaponStat != null)
        {
            LevelText.text = "Lv." + myWeaponStat.totalLevel;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownTime = Time.unscaledTime; // 누른 시간 저장
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //드래그 중이 아니었고 & 누른 지 0.2초 이내에 뗐다면 '클릭'으로 판정!
        if (!isDragging && (Time.unscaledTime - pointerDownTime) < 0.2f)
        {
            InventoryManager.Instance.OpenUpgradePanel(myWeaponStat);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        startPosition = transform.position;
        originalParent = transform.parent;
        
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        if (!InventoryManager.Instance.myWeapons.Contains(myWeaponStat))
        {
            Destroy(gameObject); 
            return;
        }

        if (transform.parent == transform.root)
        {
            transform.SetParent(originalParent);
            transform.position = startPosition;
        }
    }
}
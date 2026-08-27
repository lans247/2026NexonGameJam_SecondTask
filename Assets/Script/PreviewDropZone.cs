using UnityEngine;
using UnityEngine.EventSystems;

public class PreviewDropZone : MonoBehaviour, IDropHandler
{
    // 어떤 종류의 용병 대기열인지 설정하는 열거형
    public enum UnitType { Normal, Tank, Faster }
    public UnitType myUnitType = UnitType.Normal;

    public void OnDrop(PointerEventData eventData)
    {
        WeaponDrag droppedWeapon = eventData.pointerDrag.GetComponent<WeaponDrag>();
        
        if (droppedWeapon != null)
        {
            // 무기의 고유 스탯과 함께 용병의 종류도 GameManager로 넘깁니다.
            bool success = GameManager.Instance.SpawnUnitWithWeapon(droppedWeapon.myWeaponStat, myUnitType);
            
            if (success)
            {
                InventoryManager.Instance.RemoveWeapon(droppedWeapon.myWeaponStat);
                Destroy(droppedWeapon.gameObject);
            }
            else
            {
                droppedWeapon.transform.SetParent(InventoryManager.Instance.scrollContent);
            }
        }
    }
}
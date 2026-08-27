using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; 

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public enum WeaponType { Revolver, Shotgun, Rifle, Bazooka }

    [System.Serializable]
    public class WeaponStat
    {
        public WeaponType type; 
        
        [Header("Total Level & Cost")]
        public int totalLevel = 1;
        public float upgradeCost = 10f; 

        [Header("Damage Stat")]
        public int damageLevel = 1;
        public float damage;

        [Header("Attack Speed Stat")]
        public int attackSpeedLevel = 1;
        public float attackSpeed; 

        [Header("Knockback Stat")]
        public int knockbackLevel = 1;
        public float knockback;

        [Header("Attack Range Stat")]
        public int rangeLevel = 1;
        public float attackRange; 

        //무기를 새로 만들 때 스탯을 독립적으로 복사
        public WeaponStat Clone()
        {
            return (WeaponStat)this.MemberwiseClone();
        }
    }
    [Header("무기 초기 스탯 설정")]
    public List<WeaponStat> startingWeaponStats = new List<WeaponStat>();

    public List<WeaponStat> myWeapons = new List<WeaponStat>();
    private WeaponStat currentSelectedWeaponStat;

    public Dictionary<WeaponType, WeaponStat> weaponStats = new Dictionary<WeaponType, WeaponStat>();
    public Dictionary<WeaponType, int> weaponCounts = new Dictionary<WeaponType, int>();

    [Header("UI 연결")]
    public Transform scrollContent; 
    public GameObject weaponUIPrefab; 

    [Header("강화 UI 패널 연결")]
    public GameObject upgradePanel;       
    public TextMeshProUGUI upgradeTitleText; 
    public TextMeshProUGUI upgradeStatDMG;
    public TextMeshProUGUI upgradeStatASP;
    public TextMeshProUGUI upgradeStatKNG;
    public TextMeshProUGUI upgradeStatRNG; 
    public TextMeshProUGUI upgradeCostText;  
    private WeaponType currentSelectedWeapon; 

    [Header("무기 아이콘 스프라이트")]
    public Sprite revolverIcon;
    public Sprite shotgunIcon;
    public Sprite rifleIcon;
    public Sprite bazookaIcon;

    [Header("무기 제작 비용 (Scrap)")]
    public int revolverCraftCost = 5;
    public int shotgunCraftCost = 10;
    public int rifleCraftCost = 15;
    public int bazookaCraftCost = 20;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        weaponCounts[WeaponType.Revolver] = 0;
        weaponCounts[WeaponType.Shotgun] = 0;
        weaponCounts[WeaponType.Rifle] = 0;
        weaponCounts[WeaponType.Bazooka] = 0;

        foreach (WeaponStat stat in startingWeaponStats)
        {
            weaponStats[stat.type] = stat;
        }

        if (upgradePanel != null) upgradePanel.SetActive(false); 
    }

    // --- 제작 로직 ---
    public void CraftRevolver() { if (ResourceManager.Instance.SpendResources(0, revolverCraftCost)) CraftWeapon(WeaponType.Revolver); }
    public void CraftShotgun()  { if (ResourceManager.Instance.SpendResources(0, shotgunCraftCost)) CraftWeapon(WeaponType.Shotgun); }
    public void CraftRifle()    { if (ResourceManager.Instance.SpendResources(0, rifleCraftCost)) CraftWeapon(WeaponType.Rifle); }
    public void CraftBazooka()  { if (ResourceManager.Instance.SpendResources(0, bazookaCraftCost)) CraftWeapon(WeaponType.Bazooka); }

    private void CraftWeapon(WeaponType type)
    {
        foreach (WeaponStat baseStat in startingWeaponStats)
        {
            if (baseStat.type == type)
            {
                myWeapons.Add(baseStat.Clone());
                RefreshInventoryUI();
                return;
            }
        }
    }

    //인벤에서 제거 = 출격
    public void RemoveWeapon(WeaponStat stat)
    {
        if (myWeapons.Contains(stat))
        {
            myWeapons.Remove(stat);
            RefreshInventoryUI();
        }
    }

    //죽어서 회수한 경우
    public void AddExistingWeapon(WeaponStat stat)
    {
        myWeapons.Add(stat);
        RefreshInventoryUI();
    }

    //인벤토리에 무기 추가
    public void RefreshInventoryUI()
    {
        foreach (Transform child in scrollContent) Destroy(child.gameObject);

        foreach (WeaponStat stat in myWeapons)
        {
            GameObject itemObj = Instantiate(weaponUIPrefab, scrollContent);
            WeaponDrag itemDrag = itemObj.GetComponent<WeaponDrag>();
            
            //Drag에 정보를 저장함
            itemDrag.myWeaponStat = stat; 
            itemDrag.UpdateLevelUI();

            Transform spriteObj = itemObj.transform.Find("WeaponSprite");

            if (spriteObj != null)
            {
                Image img = spriteObj.GetComponent<Image>();
    
                switch (stat.type)
                {
                    case WeaponType.Revolver: img.sprite = revolverIcon; break;
                    case WeaponType.Shotgun: img.sprite = shotgunIcon; break;
                    case WeaponType.Rifle: img.sprite = rifleIcon; break;
                    case WeaponType.Bazooka: img.sprite = bazookaIcon; break;
                }
            }
            else
            {
                Debug.LogWarning("WeaponSprite 자식 오브젝트를 찾을 수 없습니다. 무기 UI 프리팹 구조를 확인해 주세요.");
            }
        }
    }

    // --- 강화 로직 ---
    public void OpenUpgradePanel(WeaponStat stat)
    {
        currentSelectedWeaponStat = stat;
        upgradePanel.SetActive(true);
        UpdateUpgradeUI();
    }

    public void UIBtn_CloseUpgradePanel()
    {
        upgradePanel.SetActive(false);
    }

    // 1. 데미지 강화 버튼
    public void UIBtn_UpgradeDamage()
    {
        int cost = Mathf.RoundToInt(currentSelectedWeaponStat.upgradeCost); 
        if (ResourceManager.Instance.SpendResources(0, cost))
        {
            currentSelectedWeaponStat.totalLevel++;   
            currentSelectedWeaponStat.damageLevel++;
            currentSelectedWeaponStat.damage += 1f; 
            currentSelectedWeaponStat.upgradeCost *= 1.10f; 
            UpdateUpgradeUI();
        }
    }

    // 2. 공격속도 강화 버튼
    public void UIBtn_UpgradeAttackSpeed()
    {
        int cost = Mathf.RoundToInt(currentSelectedWeaponStat.upgradeCost); 
        if (ResourceManager.Instance.SpendResources(0, cost))
        {
            currentSelectedWeaponStat.totalLevel++;
            currentSelectedWeaponStat.attackSpeedLevel++;
            currentSelectedWeaponStat.attackSpeed += 0.1f; 
            currentSelectedWeaponStat.upgradeCost *= 1.10f; 
            UpdateUpgradeUI();
        }
    }

    // 3. 넉백 강화 버튼
    public void UIBtn_UpgradeKnockback()
    {
        int cost = Mathf.RoundToInt(currentSelectedWeaponStat.upgradeCost); 
        if (ResourceManager.Instance.SpendResources(0, cost))
        {
            currentSelectedWeaponStat.totalLevel++;
            currentSelectedWeaponStat.knockbackLevel++;
            currentSelectedWeaponStat.knockback += 0.5f; 
            currentSelectedWeaponStat.upgradeCost *= 1.10f; 
            UpdateUpgradeUI();
        }
    }

    // 4. 사거리 강화 버튼
    public void UIBtn_UpgradeRange()
    {
        int cost = Mathf.RoundToInt(currentSelectedWeaponStat.upgradeCost); 
        if (ResourceManager.Instance.SpendResources(0, cost))
        {
            currentSelectedWeaponStat.totalLevel++;
            currentSelectedWeaponStat.rangeLevel++;
            currentSelectedWeaponStat.attackRange += 0.5f; 
            currentSelectedWeaponStat.upgradeCost *= 1.10f; 
            UpdateUpgradeUI();
        }
    }

    // UI 화면 갱신
    private void UpdateUpgradeUI()
    {
        RefreshInventoryUI();
        
        if(currentSelectedWeaponStat == null) return;

        if (upgradeTitleText != null) 
            upgradeTitleText.text = $"{currentSelectedWeaponStat.type} 튜닝 (총 레벨: {currentSelectedWeaponStat.totalLevel})"; 
        
        if(upgradeStatDMG != null)
            upgradeStatDMG.text = $"LV.{currentSelectedWeaponStat.damageLevel} : DMG {currentSelectedWeaponStat.damage:F2}";
        if(upgradeStatASP != null)
            upgradeStatASP.text = $"LV.{currentSelectedWeaponStat.attackSpeedLevel} : AttackSpeed {currentSelectedWeaponStat.attackSpeed:F2}";
        if(upgradeStatRNG != null)
            upgradeStatRNG.text = $"LV.{currentSelectedWeaponStat.rangeLevel} : Range {currentSelectedWeaponStat.attackRange:F2}";
        if(upgradeStatKNG != null)
            upgradeStatKNG.text = $"LV.{currentSelectedWeaponStat.knockbackLevel} : Knockback {currentSelectedWeaponStat.knockback:F2}";
            
        if (upgradeCostText != null)
        {
            upgradeCostText.text = $"COST: {Mathf.RoundToInt(currentSelectedWeaponStat.upgradeCost)} Scrap";
        }
    }
}
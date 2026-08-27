using UnityEngine;

public class TowerManager : MonoBehaviour
{
    public static TowerManager Instance;

    public enum TowerType { BuffRange, BuffHP, BuffAttackSpeed, BuffDamage, DebuffSpeed, DebuffAttack, DebuffDoT, DebuffScrap,Turret }

    [Header("버프 타워 프리팹")]
    public GameObject buffRangeTowerPrefab;
    public GameObject buffHpTowerPrefab;
    public GameObject buffSpeedTowerPrefab;
    public GameObject buffDamageTowerPrefab;

    [Header("디버프 타워 프리팹")]
    public GameObject debuffSpeedTowerPrefab;
    public GameObject debuffAttackTowerPrefab;
    public GameObject debuffDoTTowerPrefab;
    public GameObject debuffScrapTowerPrefab;

    [Header("특수 타워 프리팹")]
    public GameObject turretTowerPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 아이콘이 쿨타임이 끝날 때마다 호출하여 무작위 종류를 받아가는 함수
    public TowerType GetRandomTowerType()
    {
        System.Array values = System.Enum.GetValues(typeof(TowerType));
        return (TowerType)values.GetValue(Random.Range(0, values.Length));
    }

    // 매개변수로 설치할 타워의 level을 함께 받습니다.
    public bool TryBuildTower(TowerType type, int level, Vector3 mouseScreenPos)
    {
        if (RoundManager.Instance == null) return false;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        
        int x = Mathf.RoundToInt(worldPos.x / RoundManager.Instance.tileSize);
        int y = Mathf.RoundToInt(worldPos.y / RoundManager.Instance.tileSize);

        if (x >= 0 && x < RoundManager.Instance.mapWidth && y >= 0 && y < RoundManager.Instance.mapHeight)
        {
            if (RoundManager.Instance.mapGrid[x, y] == 0) 
            {
                GameObject prefabToBuild = null;
                
                switch(type)
                {
                    case TowerType.BuffRange: prefabToBuild = buffRangeTowerPrefab; break;
                    case TowerType.BuffHP: prefabToBuild = buffHpTowerPrefab; break;
                    case TowerType.BuffAttackSpeed: prefabToBuild = buffSpeedTowerPrefab; break;
                    case TowerType.BuffDamage: prefabToBuild = buffDamageTowerPrefab; break;
                    case TowerType.DebuffSpeed: prefabToBuild = debuffSpeedTowerPrefab; break;
                    case TowerType.DebuffAttack: prefabToBuild = debuffAttackTowerPrefab; break;
                    case TowerType.DebuffDoT: prefabToBuild = debuffDoTTowerPrefab; break;
                    case TowerType.DebuffScrap: prefabToBuild = debuffScrapTowerPrefab; break;
                    case TowerType.Turret: prefabToBuild = turretTowerPrefab; break;
                }
                
                if(prefabToBuild != null)
                {
                    GameObject spawnedTower = Instantiate(prefabToBuild, new Vector3(x * RoundManager.Instance.tileSize, y * RoundManager.Instance.tileSize, 0), Quaternion.identity);
                    
                    // 버프 타워 스탯 적용
                    BuffTower buffTower = spawnedTower.GetComponent<BuffTower>();
                    if (buffTower != null)
                    {
                        buffTower.buffLevel = level;
                        buffTower.buffAmount = buffTower.buffAmount * level; 
                    }

                    // 디버프 타워 스탯 적용
                    DebuffTower debuffTower = spawnedTower.GetComponent<DebuffTower>();
                    if (debuffTower != null)
                    {
                        debuffTower.debuffLevel = level;
                        debuffTower.debuffAmount = debuffTower.debuffAmount * level; 
                    }

                    RoundManager.Instance.mapGrid[x, y] = 2; 
                    return true; 
                }
            }
        }
        
        return false; 
    }
}
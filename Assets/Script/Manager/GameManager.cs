using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; 

    public GameObject revolverPrefab; 
    public GameObject shotgunPrefab;  
    public GameObject riflePrefab;
    public GameObject bazookaPrefab;

    [Header("유닛 소환 비용")]
    public int normalCoinCost = 20;
    public int tankCoinCost = 30;
    public int fasterCoinCost = 25;

    [Header("게임 배속 설정")]
    private float[] speedSteps = { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f, 3f, 4f };
    private int currentSpeedIndex = 3; 
    private bool isPaused = false;
    public TMPro.TextMeshProUGUI speedText;

    [Header("Debug Settings")]
    public bool isDebugMode = false;

    [Header("기지(플레이어) 체력 설정")]
    public float maxBaseHp = 100f;
    public float currentBaseHp;

    [Header("결과 UI 패널 연결")]
    public GameObject clearPanel; // 라운드 클리어 시 띄울 패널
    public TextMeshProUGUI clearMessageText;
    public TextMeshProUGUI baseHpText;
    

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentBaseHp = maxBaseHp;
        
        if (clearPanel != null) clearPanel.SetActive(false);

        UpdateBaseHpUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            isDebugMode = !isDebugMode;
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            RoundManager.Instance.StartRound(RoundManager.Instance.currentRoundIndex + 1);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddResources(1000, 10000);
            }
        }
    }


    // 기지 체력 감소 함수
    public void TakeBaseDamage(float damage)
    {
        if (currentBaseHp <= 0) return; // 이미 파괴되었다면 무시

        currentBaseHp -= damage;

        UpdateBaseHpUI();

        if (currentBaseHp <= 0)
        {
            GameOver();
        }
    }

    private void UpdateBaseHpUI()
    {
        if (baseHpText != null)
        {
            baseHpText.text = $"상자 내구도: {currentBaseHp} / {maxBaseHp}";
        }
    }

    private void GameOver()
    {
        Time.timeScale = 1f;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayFailSound();
        }
        
        SceneManager.LoadScene("FailScene");
    }

    public void ShowClearPanel(string message) 
    {
        Time.timeScale = 0f; 
        
        if (clearMessageText != null)
        {
            clearMessageText.text = message;
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayClearSound();
        }

        if (clearPanel != null) clearPanel.SetActive(true);
    }

    // ---------- UI 버튼 연결용 함수 (씬 전환) ---------- //

    // 1. 게임 오버 창에서 StartScene으로 돌아가는 버튼
    public void UIBtn_GoToStartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene");
    }

    // 2. 클리어 창에서 RoundScene(라운드 선택창)으로 가는 버튼
    public void UIBtn_GoToRoundScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("RoundScene");
    }

    // 3. 클리어 창에서 바로 다음 라운드로 진입하는 버튼
    public void UIBtn_NextRound()
    {
        Time.timeScale = 1f;
        int nextRound = RoundManager.Instance.currentRoundIndex + 1;
        
        //만약 방금 깬 라운드가 마지막 라운드였다면 ClearScene으로 이동!
        if (nextRound >= RoundManager.Instance.roundList.Count)
        {
            SceneManager.LoadScene("ClearScene");
        }
        else
        {
            // 다음 라운드 번호를 저장하고 현재 GameScene을 다시 로드합니다.
            PlayerPrefs.SetInt("SelectedRound", nextRound);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
        }
    }



    public void UIBtn_PauseToggle()
    {
        isPaused = !isPaused; 
        if (isPaused)
        {
            Time.timeScale = 0f; 
            UpdateSpeedUI("정지");
        }
        else
        {
            Time.timeScale = speedSteps[currentSpeedIndex]; 
            UpdateSpeedUI(speedSteps[currentSpeedIndex] + "x");
        }
    }

    public void UIBtn_SpeedDown()
    {
        if (currentSpeedIndex > 0) currentSpeedIndex--;
        ApplySpeed();
    }

    public void UIBtn_SpeedUp()
    {
        if (currentSpeedIndex < speedSteps.Length - 1) currentSpeedIndex++;
        ApplySpeed();
    }

    private void ApplySpeed()
    {
        isPaused = false; 
        Time.timeScale = speedSteps[currentSpeedIndex];
        UpdateSpeedUI(speedSteps[currentSpeedIndex] + "x");
    }

    private void UpdateSpeedUI(string text)
    {
        if (speedText != null) speedText.text = text;
    }

    public bool SpawnUnitWithWeapon(InventoryManager.WeaponStat weaponStat, PreviewDropZone.UnitType unitType)
    {
        // RoundManager에서 웨이포인트 정보를 가져옵니다.
        List<Vector3> wps = RoundManager.Instance.worldWaypoints;
        if (wps.Count == 0) return false;

        int requiredCoin = normalCoinCost;
        if (unitType == PreviewDropZone.UnitType.Tank) requiredCoin = tankCoinCost;
        else if (unitType == PreviewDropZone.UnitType.Faster) requiredCoin = fasterCoinCost;

        if (ResourceManager.Instance.SpendResources(requiredCoin, 0))
        {
            GameObject prefabToSpawn = null;
            
            switch (weaponStat.type)
            {
                case InventoryManager.WeaponType.Revolver: prefabToSpawn = revolverPrefab; break;
                case InventoryManager.WeaponType.Shotgun: prefabToSpawn = shotgunPrefab; break;
                case InventoryManager.WeaponType.Rifle: prefabToSpawn = riflePrefab; break;
                case InventoryManager.WeaponType.Bazooka: prefabToSpawn = bazookaPrefab; break;
            }

            if (prefabToSpawn != null)
            {
                Vector3 spawnPos = wps[wps.Count - 1];
                // RoundManager가 관리하는 폴더(Container) 하위에 소환합니다.
                GameObject defenderObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, RoundManager.Instance.defenderContainer);
                
                Defender def = defenderObj.GetComponent<Defender>();
                def.SetWaypoints(wps);
                def.myWeaponStat = weaponStat;
                
                float baseSpeed = 6f, baseHp = 50f, baseDmg = 10f, baseAspd = 1f, baseRange = 1.5f, baseKb = 1f;

                if (unitType == PreviewDropZone.UnitType.Tank)
                {
                    baseSpeed = 4f; baseHp = 100f; baseDmg = 12f; baseAspd = 1f; baseRange = 1.5f; baseKb = 1.5f;
                }
                else if (unitType == PreviewDropZone.UnitType.Faster)
                {
                    baseSpeed = 8f; baseHp = 30f; baseDmg = 9f; baseAspd = 1.1f; baseRange = 1.5f; baseKb = 1f;
                }

                float finalDmg = weaponStat.damage * (baseDmg / 10f);
                float finalCd = 1f / (weaponStat.attackSpeed * baseAspd);
                float finalKb = weaponStat.knockback * (baseKb / 1f);
                float finalRange = weaponStat.attackRange * (baseRange / 1.5f);

                def.SetupBaseStats(baseSpeed, baseHp, finalDmg, finalCd, finalRange, finalKb);

                def.SetUnitVisuals(unitType);

                if (SoundManager.Instance != null) SoundManager.Instance.PlaySummonSound();
                

                return true;
            }
        }
        else
        {
            Debug.Log("용병을 고용할 코인이 부족합니다.");
            if (SoundManager.Instance != null) SoundManager.Instance.PlayFailSound();
        }
        return false;
    }
}
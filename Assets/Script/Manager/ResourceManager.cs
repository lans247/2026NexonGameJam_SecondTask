using UnityEngine;
using TMPro; // 텍스트 UI를 다루기 위해 추가

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [Header("시작 재화")]
    public int coins = 100;
    public int scrap = 0;

    [Header("UI 텍스트 연결")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI scrapText;

    void Awake()
    {
        // 싱글톤 셋업
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
    }

    // 적이 죽을 때 재화를 추가하는 함수
    public void AddResources(int coinAmount, int scrapAmount)
    {
        coins += coinAmount;
        scrap += scrapAmount;
        UpdateUI();
    }

    // 유닛/포탑 구매 시 재화를 소모하는 함수
    public bool SpendResources(int coinCost, int scrapCost)
    {
        if (coins >= coinCost && scrap >= scrapCost)
        {
            coins -= coinCost;
            scrap -= scrapCost;
            UpdateUI();
            return true;
        }
        return false; // 재화가 부족하면 false 반환
    }

    private void UpdateUI()
    {
        if (coinText != null) coinText.text = "Coin: " + coins;
        if (scrapText != null) scrapText.text = "Scrap: " + scrap;
    }
}
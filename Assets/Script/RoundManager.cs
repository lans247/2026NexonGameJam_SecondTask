using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;

    [Header("라운드 설정")]
    public List<RoundData> roundList;
    public int currentRoundIndex = 0;
    private RoundData currentRoundData;

    [Header("프리팹 연결")]
    public GameObject enemyPrefab;

    [Header("맵 환경 설정 (타워 건설 판정용)")]
    public float tileSize = 2f;
    [HideInInspector] public int mapWidth;
    [HideInInspector] public int mapHeight;
    public int[,] mapGrid; 
    public List<Vector3> worldWaypoints = new List<Vector3>();  

    [Header("전투 진행 상태")]
    public int activeEnemiesCount = 0; // 현재 맵에 살아있는 적의 수
    private bool isSpawningWave = false; // 현재 웨이브 소환이 진행 중인지 여부

    [Header("전투 UI 및 튜토리얼 텍스트 연결")]
    public TextMeshProUGUI waveText;          // 현재 웨이브 표시 텍스트
    public TextMeshProUGUI enemyCountText;    // 남은 적 수 표시 텍스트
    public TextMeshProUGUI nextWaveTimeText;  // 다음 웨이브 대기 시간 표시 텍스트
    public GameObject tutorialInformationText;// 0라운드에서만 켜질 튜토리얼 오브젝트
    private int currentWaveIndex = 0;
    private float nextWaveTimer = 0f;
    private bool isWaitingForNextWave = false;





    private Transform mapContainer;
    private Transform enemyContainer;
    public Transform defenderContainer; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        mapContainer = new GameObject("=== Map Environment ===").transform;
        enemyContainer = new GameObject("=== Enemies ===").transform;
        defenderContainer = new GameObject("=== Defenders ===").transform;

        //RoundScene에서 가져온 라운드 넘버로
        int selectedRound = PlayerPrefs.GetInt("SelectedRound", 0);
        StartRound(selectedRound);
    }

    public void StartRound(int roundIndex)
    {
        if (roundIndex >= roundList.Count)
        {
            Debug.Log("모든 라운드를 클리어했습니다.");
            return;
        }

        currentRoundIndex = roundIndex;
        currentRoundData = roundList[currentRoundIndex];

        ClearBoard();

        if (tutorialInformationText != null)
        {
            tutorialInformationText.SetActive(currentRoundIndex == 0);
        }

        if (currentRoundData.mapPrefab != null)
        {
            // 1. 라운드 데이터에 등록된 맵(타일맵 + 웨이포인트) 프리팹 소환
            GameObject currentMapObj = Instantiate(currentRoundData.mapPrefab, Vector3.zero, Quaternion.identity, mapContainer);
            
            // 2. 맵에 붙어있는 설계도(MapBlueprint) 정보 추출
            MapBlueprint blueprint = currentMapObj.GetComponent<MapBlueprint>();
            if (blueprint != null)
            {
                mapWidth = blueprint.mapWidth;
                mapHeight = blueprint.mapHeight;

                // 설계도에서 Transform의 좌표들만 꺼내옵니다.
                worldWaypoints = blueprint.GetWaypointPositions();

                // 3. 투명한 논리 격자(타워 건설 제한용) 생성
                BuildLogicGrid();
            }
        }

        StartCoroutine(SpawnEnemyWaveRoutine());
    }

    void Update()
    {
        if (enemyCountText != null)
        {
            enemyCountText.text = $"남은 적: {activeEnemiesCount}";
        }

        if (nextWaveTimeText != null)
        {
            if (isWaitingForNextWave && nextWaveTimer > 0)
            {
                // 소수점 1자리까지 타이머를 보여줍니다.
                nextWaveTimeText.text = $"다음 웨이브까지: {nextWaveTimer:F1}초";
            }
            else if (isSpawningWave)
            {
                nextWaveTimeText.text = "적 스폰 중!";
            }
            else
            {
                nextWaveTimeText.text = "전투 중!";
            }
        }
    }

    // 웨이브 텍스트를 갱신하는 함수
    private void UpdateWaveUI()
    {
        if (waveText != null && currentRoundData != null)
        {
            waveText.text = $"웨이브: {currentWaveIndex} / {currentRoundData.waves.Count}";
        }
    }

    private void ClearBoard()
    {
        foreach (Transform child in mapContainer) Destroy(child.gameObject);
        foreach (Transform child in enemyContainer) Destroy(child.gameObject);
        foreach (Transform child in defenderContainer) Destroy(child.gameObject);
        
        Tower[] existingTowers = FindObjectsOfType<Tower>();
        foreach (Tower t in existingTowers) Destroy(t.gameObject);
    }

    // 길(Path)을 스크립트 내부 행렬(mapGrid)에 1로 채워 타워를 못 짓게 하는 함수
    void BuildLogicGrid()
    {
        mapGrid = new int[mapWidth, mapHeight];

        for (int i = 0; i < worldWaypoints.Count - 1; i++)
        {
            int currentX = Mathf.RoundToInt(worldWaypoints[i].x / tileSize);
            int currentY = Mathf.RoundToInt(worldWaypoints[i].y / tileSize);
            int targetX = Mathf.RoundToInt(worldWaypoints[i + 1].x / tileSize);
            int targetY = Mathf.RoundToInt(worldWaypoints[i + 1].y / tileSize);

            while (currentX != targetX || currentY != targetY)
            {
                if (currentX >= 0 && currentX < mapWidth && currentY >= 0 && currentY < mapHeight)
                {
                    mapGrid[currentX, currentY] = 1; 
                }

                if (currentX < targetX) currentX++;
                else if (currentX > targetX) currentX--;
                else if (currentY < targetY) currentY++;
                else if (currentY > targetY) currentY--;
            }
        }

        if (worldWaypoints.Count > 0)
        {
            int lastX = Mathf.RoundToInt(worldWaypoints[worldWaypoints.Count - 1].x / tileSize);
            int lastY = Mathf.RoundToInt(worldWaypoints[worldWaypoints.Count - 1].y / tileSize);
            if (lastX >= 0 && lastX < mapWidth && lastY >= 0 && lastY < mapHeight)
            {
                mapGrid[lastX, lastY] = 1;
            }
        }
    }

    public bool IsOnPath(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / tileSize);
        int y = Mathf.RoundToInt(worldPos.y / tileSize);

        if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
        {
            return mapGrid[x, y] == 1; 
        }
        return false; 
    }

    private System.Collections.IEnumerator SpawnEnemyWaveRoutine()
    {
        isSpawningWave = true;
        
        nextWaveTimer = 2f;
        isWaitingForNextWave = true;
        while (nextWaveTimer > 0)
        {
            nextWaveTimer -= Time.deltaTime;
            yield return null; 
        }
        isWaitingForNextWave = false;

        for (int w = 0; w < currentRoundData.waves.Count; w++)
        {
            currentWaveIndex = w + 1; 
            UpdateWaveUI();

            WaveData currentWave = currentRoundData.waves[w];
            
            nextWaveTimer = currentWave.delayBeforeWave;
            isWaitingForNextWave = true;
            while (nextWaveTimer > 0)
            {
                nextWaveTimer -= Time.deltaTime;
                yield return null; 
            }
            isWaitingForNextWave = false;
            
            //적 스폰
            for (int i = 0; i < currentWave.enemyCount; i++)
            {
                SpawnEnemy(currentWave);
                yield return new WaitForSeconds(currentWave.spawnInterval);
            }
        }
        isSpawningWave = false;
        CheckRoundClear(); 
    }

    void SpawnEnemy(WaveData waveData)
    {
        if (worldWaypoints.Count > 0)
        {
            GameObject enemyObj = Instantiate(enemyPrefab, worldWaypoints[0], Quaternion.identity, enemyContainer);
            activeEnemiesCount++;
            Enemy enemyScript = enemyObj.GetComponent<Enemy>();

            enemyScript.SetWaypoints(worldWaypoints);
            enemyScript.InitStats(waveData.enemyHp, waveData.enemySpeed, waveData.enemyDamage);

            if (waveData.isBossWave)
            {
                enemyObj.transform.localScale = new Vector3(8f, 8f, 1f);
                SpriteRenderer sr = enemyObj.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sr.color = Color.red;
            }
        }
    }

    // Enemy.cs에서 적이 파괴될 때마다 이 함수를 호출합니다.
    public void OnEnemyDestroyed()
    {
        activeEnemiesCount--;
        CheckRoundClear();
    }


    // 승리 판정 함수
    private void CheckRoundClear()
    {
        // 소환이 모두 끝났고, 살아있는 적이 없으며, 플레이어 기지 체력이 남아있다면 승리
        if (!isSpawningWave && activeEnemiesCount <= 0 && GameManager.Instance.currentBaseHp > 0)
        {
            Debug.Log("라운드 클리어!");

            int unlockedRound = PlayerPrefs.GetInt("UnlockedRound", 0);
            if (currentRoundIndex >= unlockedRound)
            {
                PlayerPrefs.SetInt("UnlockedRound", currentRoundIndex + 1);
            }

            string clearMsg = currentRoundData.roundClearMessage;
            GameManager.Instance.ShowClearPanel(clearMsg);
        }
    }
}
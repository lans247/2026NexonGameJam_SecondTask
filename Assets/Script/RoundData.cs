using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WaveData
{
    [Header("웨이브 기본 설정")]
    public int enemyCount = 10;
    public float spawnInterval = 1f;
    public float delayBeforeWave = 5f; 

    [Header("적 스탯 설정")]
    public float enemyHp = 100f;
    public float enemySpeed = 2f;
    public float enemyDamage = 10f;

    [Header("보스 설정")]
    public bool isBossWave = false; 
}

[CreateAssetMenu(fileName = "NewRoundData", menuName = "DefenseGame/RoundData")]
public class RoundData : ScriptableObject
{
    [Header("맵 프리팹 연결")]
    // 시각적 타일맵과 MapBlueprint가 모두 포함된 프리팹을 연결합니다.
    public GameObject mapPrefab; 

    [Header("라운드 클리어 시 출력할 텍스트")]
    [TextArea(3, 5)] 
    public string roundClearMessage = "라운드를 클리어했습니다!";

    [Header("웨이브 목록")]
    public List<WaveData> waves = new List<WaveData>();
}
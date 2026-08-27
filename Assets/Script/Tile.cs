using UnityEngine;

// 맵의 각 칸(그리드)의 속성을 저장하는 클래스입니다.
public class Tile : MonoBehaviour
{
    public bool isPath; // 길인지 여부 (적 이동 & 아군 파견 가능)
    public bool isOccupied; // 이미 포탑이나 아군이 배치되었는지 여부

    // 에디터에서 쉽게 구분할 수 있도록 색상을 변경합니다.
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (isPath)
                sr.color = new Color(0.7f, 0.7f, 0.7f); // 길은 회색
            else
                sr.color = new Color(0.9f, 0.9f, 0.9f); // 건설 구역은 밝은 흰색
        }
    }
}
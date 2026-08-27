using UnityEngine;
using System.Collections.Generic;

public class MapBlueprint : MonoBehaviour
{
    [Header("맵 크기 설정 (논리적 크기)")]
    public int mapWidth = 30;
    public int mapHeight = 30;
    public float tileSize = 2f;

    [Header("웨이포인트 묶음 (부모 오브젝트)")]
    // 이 변수에 자식으로 만든 웨이포인트들을 묶어둔 부모 오브젝트를 연결합니다.
    public Transform waypointContainer;

    // 게임 시작 시 라운드 매니저가 이 함수를 호출하여 좌표를 한 번에 빼갑니다.
    public List<Vector3> GetWaypointPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        if (waypointContainer != null)
        {
            foreach (Transform child in waypointContainer)
            {
                positions.Add(child.position);
            }
        }
        return positions;
    }

    // 에디터 씬 뷰에서 맵 디자인을 할 때 실시간으로 보여주는 기즈모입니다.
    private void OnDrawGizmos()
    {
        // 1. 하얀색 격자 그리기
        Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Gizmos.DrawWireCube(transform.position + new Vector3(x * tileSize, y * tileSize, 0), new Vector3(tileSize, tileSize, 1));
            }
        }

        // 2. 웨이포인트 붉은 선 그리기
        if (waypointContainer != null && waypointContainer.childCount > 0)
        {
            Gizmos.color = Color.red;
            Transform previousNode = null;

            foreach (Transform child in waypointContainer)
            {
                if (previousNode != null)
                {
                    Gizmos.DrawLine(previousNode.position, child.position);
                }
                Gizmos.DrawSphere(child.position, 0.2f);
                previousNode = child;
            }
        }
    }
}
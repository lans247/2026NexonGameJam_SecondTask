using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("줌 설정")]
    public float zoomSpeed = 5f; // 마우스 휠 민감도
    public float minZoom = 3f;   // 최대 줌 인 (가장 가까움, 숫자가 작을수록 확대됨)
    public float maxZoom = 15f;  // 최대 줌 아웃 (가장 멂, 숫자가 클수록 축소됨)

    private Camera cam;

    void Start()
    {
        // 이 스크립트가 붙어있는 카메라 컴포넌트를 가져옵니다.
        cam = GetComponent<Camera>(); 
    }

    void Update()
    {
        HandleZoom();
    }

    void HandleZoom()
    {
        // 마우스 휠 스크롤 값을 가져옵니다. (위로 굴리면 양수, 아래로 굴리면 음수)
        float scrollData = Input.GetAxis("Mouse ScrollWheel");

        if (scrollData != 0f)
        {
            // 1. 줌을 하기 전, 현재 마우스가 가리키고 있는 게임 월드의 좌표를 기억합니다.
            Vector3 mouseWorldPosBeforeZoom = cam.ScreenToWorldPoint(Input.mousePosition);

            // 2. 카메라 사이즈(줌) 변경 (2D 직교 카메라 기준)
            cam.orthographicSize -= scrollData * zoomSpeed;
            
            // 3. 줌 한계를 설정하여 너무 가깝거나 너무 멀어지는 것을 방지합니다.
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);

            // 4. 줌을 하고 난 후, 방금 전과 동일한 화면 픽셀 좌표가 월드에서 어디를 가리키는지 다시 계산합니다.
            Vector3 mouseWorldPosAfterZoom = cam.ScreenToWorldPoint(Input.mousePosition);

            // 5. 줌 전후의 월드 좌표 차이만큼 카메라 위치를 이동시켜서 커서 위치를 중앙으로 유지합니다.
            Vector3 difference = mouseWorldPosBeforeZoom - mouseWorldPosAfterZoom;
            cam.transform.position += difference;
        }
    }
}
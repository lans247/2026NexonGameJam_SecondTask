using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RoundSelectManager : MonoBehaviour
{
    [Header("라운드 선택 버튼들")]
    // 에디터에서 1라운드, 2라운드, 3라운드 버튼을 순서대로 넣을 배열입니다.
    public Button[] roundButtons; 

    void Start()
    {
        RefreshRoundButtons();
    }

    private void RefreshRoundButtons()
    {
        // GameManager에서 클리어 시 저장했던 "UnlockedRound" 값을 불러옵니다. (기본값 0)
        int unlockedRound = PlayerPrefs.GetInt("UnlockedRound", 0);

        for (int i = 0; i < roundButtons.Length; i++)
        {
            if (i <= unlockedRound)
            {
                // 해금된 라운드: 클릭 가능하도록 활성화
                roundButtons[i].interactable = true;
            }
            else
            {
                // 아직 도달하지 못한 라운드: 클릭 불가 (잠금 상태)
                roundButtons[i].interactable = false;
            }
        }
    }

    // 각 라운드 버튼이 눌렸을 때 호출될 함수 (인스펙터에서 번호를 넘겨줍니다)
    public void UIBtn_SelectRound(int roundIndex)
    {
        // 플레이어가 선택한 라운드 번호를 "SelectedRound"라는 이름으로 저장합니다.
        PlayerPrefs.SetInt("SelectedRound", roundIndex);
        
        // GameScene으로 넘어가면, RoundManager가 방금 저장한 번호를 읽고 해당 맵을 띄웁니다.
        SceneManager.LoadScene("GameScene");
    }

    // (선택 사항) 시작 화면으로 돌아가는 뒤로가기 버튼
    public void UIBtn_GoBackToStart()
    {
        SceneManager.LoadScene("StartScene");
    }
}
using UnityEngine;
using UnityEngine.UI;

public class UISoundController : MonoBehaviour
{
    // ==========================================
    // 버튼의 On Click () 에 연결할 함수들
    // ==========================================
    
    public void OnClick_SoundUP()
    {
        // 살아있는 진짜 사운드 매니저를 찾아서 명령을 전달합니다.
        if (SoundManager.Instance != null) SoundManager.Instance.SoundUP();
    }

    public void OnClick_SoundDown()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SoundDown();
    }

    public void OnClick_SoundMute()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SoundMute();
    }

    // ==========================================
    // 슬라이더의 On Value Changed 에 연결할 함수
    // ==========================================
    
    public void OnScroll_Volume(float value)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SoundScroll(value);
    }
}
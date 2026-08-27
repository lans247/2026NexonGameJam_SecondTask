using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    void Start()
    {
        Button btn = GetComponent<Button>();
        
        if (btn != null)
        {
            btn.onClick.AddListener(PlaySound);
        }
    }

    void PlaySound()
    {
        // 살아남은 진짜 사운드 매니저를 찾아서 소리를 재생합니다.
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonSound();
        }
    }
}
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("오디오 소스")]
    public AudioSource bgmSource;
    public AudioSource sfxSource; 

    [Header("배경음악 파일")]
    public AudioClip mainBGM;

    [Header("효과음 파일")]
    public AudioClip hitSound;   
    public AudioClip clearSound;  
    public AudioClip failSound;   
    public AudioClip buttonSound;
    public AudioClip LevelSound;
    public AudioClip SummonSound;
    public AudioClip BulletSound;
    public AudioClip HPSound;

    [Header("사운드 설정창 UI")]
    public GameObject soundWindowPrefab; 
    private GameObject currentSoundWindow;

    private float currentVolume = 1f;
    private bool isMuted = false;

    void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSoundSettings();
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        if (mainBGM != null)
        {
            PlayBGM(mainBGM);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSoundWindow();
        }

        if (Input.GetKeyDown(KeyCode.F12))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("세이브 삭제됨!");
        }
    }

    public void ToggleSoundWindow()
    {
        if (currentSoundWindow != null)
        {
            PlayButtonSound(); 
            Destroy(currentSoundWindow);
            
            Time.timeScale = 1f; 
        }
        else
        {
            if (soundWindowPrefab != null)
            {
                PlayButtonSound(); // 열 때도 효과음 재생
                currentSoundWindow = Instantiate(soundWindowPrefab);
                
                DontDestroyOnLoad(currentSoundWindow); 

                Time.timeScale = 0f; 
            }
        }
    }

    public void PlayBGM(AudioClip bgmClip)
    {
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true; // BGM은 무조건 무한 반복
            bgmSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip); 
        }
    }

    // 자주 쓰는 소리를 쉽게 부르기 위한 편의용 함수들
    public void PlayHitSound()
    {
        PlaySFX(hitSound);
    }

    public void PlayClearSound()
    {
        PlaySFX(clearSound);
    }

    public void PlayFailSound() 
    { 
        PlaySFX(failSound); 
    } 
    public void PlayButtonSound() 
    { 
        PlaySFX(buttonSound); 
    }
    public void PlayLevelSound() 
    { 
        PlaySFX(LevelSound); 
    }
    public void PlaySummonSound() 
    { 
        PlaySFX(SummonSound); 
    }

    public void PlayBulletSound() 
    { 
        PlaySFX(BulletSound); 
    }
    public void PlayHPSound() 
    { 
        PlaySFX(HPSound); 
    }

    



    // 1. 슬라이더(UI Slider)를 드래그할 때 값(0~1)을 실시간으로 받아옵니다.
    public void SoundScroll(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);
        ApplyVolume();
        SaveSoundSettings();
    }

    // 2. 소리 올리기 버튼 (+10%)
    public void SoundUP()
    {
        currentVolume = Mathf.Clamp01(currentVolume + 0.1f);
        ApplyVolume();
        SaveSoundSettings();
    }

    // 3. 소리 내리기 버튼 (-10%)
    public void SoundDown()
    {
        currentVolume = Mathf.Clamp01(currentVolume - 0.1f);
        ApplyVolume();
        SaveSoundSettings();
    }

    // 4. 음소거 켜고 끄기 버튼 (토글)
    public void SoundMute()
    {
        isMuted = !isMuted;
        ApplyVolume();
        SaveSoundSettings();
    }

    // 변경된 볼륨을 실제 두 개의 AudioSource에 동시에 적용하는 내부 함수
    private void ApplyVolume()
    {
        float finalVolume = isMuted ? 0f : currentVolume;

        if (bgmSource != null) bgmSource.volume = finalVolume;
        if (sfxSource != null) sfxSource.volume = finalVolume;
    }

    // 유저의 볼륨 설정을 저장 (기기 메모리)
    private void SaveSoundSettings()
    {
        PlayerPrefs.SetFloat("Volume", currentVolume);
        PlayerPrefs.SetInt("Muted", isMuted ? 1 : 0);
    }

    private void LoadSoundSettings()
    {
        currentVolume = PlayerPrefs.GetFloat("Volume", 1f);
        isMuted = PlayerPrefs.GetInt("Muted", 0) == 1;
        ApplyVolume();
    }
}
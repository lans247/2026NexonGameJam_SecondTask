using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class NextAnim : MonoBehaviour
{
    [Header("씬 장면")]
    public List<GameObject> scenes = new List<GameObject>();

    public int nowScene = 0;

    public void Start()
    {
        SceneIndi();
    }


    public void SceneIndi()
    {
        for(int i = 0; i < scenes.Count; i++)
        {
            if(i == nowScene)
                scenes[i].SetActive(true);   
            else
                scenes[i].SetActive(false);
        }
    }


    public void UIBTN_Next()
    {
        if(nowScene != scenes.Count - 1)
        {
            nowScene++;
            SceneIndi();
        }   
    }

    public void UIBTN_PRE()
    {
        if(nowScene != 0)
        {
            nowScene--;
            SceneIndi();
        }   
    
    }

    public void GOTOROUND()
    {
        SceneManager.LoadScene("RoundScene");
    }

    public void GoToIntro()
    {
        SceneManager.LoadScene("IntroScene");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}

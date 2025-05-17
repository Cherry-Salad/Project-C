using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;


public class MainUIManager : MonoBehaviour
{
    public GameObject OptionPanel;

    bool _load = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OptionPanel.gameObject.SetActive(!OptionPanel.gameObject.activeSelf);
        }
    }

    public void StartGame()
    {
        if (_load)
            return;

        _load = true;

        //1) 먼저 화면 페이드 인
        Fade.Instance.FadeIn(() =>
        {
            // 2) 페이드 완료 콜백 안에서 로딩 시작
            Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, loadCount, totalCount) =>
            {
                if (loadCount == totalCount)
                {
                    Debug.Log("PreLoad 에셋 로드 모두 완료!");
                    Managers.Data.Init();
                    Managers.Game.Init();
                    Managers.Game.Save();
                    SceneManager.sceneLoaded += OnSceneLoaded;
                    Managers.Scene.LoadScene(EScene.TutorialScene);
                }
            });
        });
    }

    // For 최혁도, TODO: 게임 이어하기, 활성화된 세이브 포인트 씬부터 시작 

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 중복 호출 방지
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Fade.Instance.FadeOut();
    }

    public void OpenOption()
    {
        OptionPanel.SetActive(true);
        Debug.Log("눌림");
    }    
    
    public void CloseOption()
    {
        OptionPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

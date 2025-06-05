using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;


public class MainUIManager : MonoBehaviour
{
    [SerializeField]private DUIManager _duiManager;

    bool _load = false;

    private void Awake()
    {
        _duiManager = FindObjectOfType<DUIManager>();
        if (_duiManager == null)
        {
            Debug.LogError("씬에 DUIManager가 없습니다! MainUIManager에서 OptionPanel을 찾을 수 없어요.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_duiManager != null && _duiManager.OptionPanel != null)
            {
                bool currently = _duiManager.OptionPanel.activeSelf;
                _duiManager.OptionPanel.SetActive(!currently);
                if (!currently) // 이제 패널이 켜진 상태라면
                    Time.timeScale = 0f;
                else           // 이제 패널이 꺼진 상태라면
                    Time.timeScale = 1f;
            }
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
        _duiManager.OptionPanel.SetActive(true);
        Time.timeScale = 0f;
    }    
    
    public void CloseOption()
    {
        _duiManager.OptionPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

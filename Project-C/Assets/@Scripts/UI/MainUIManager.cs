using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, loadCount, totalCount) =>
        {
            // For 최혁도, TODO: 로딩 화면

            // 모두 로드 완료
            if (loadCount == totalCount)
            {
                Debug.Log("PreLoad 에셋 로드 모두 완료!");
                Managers.Data.Init();

                Managers.Game.Init();
                Managers.Game.Save();

                //SceneManager.LoadScene(1); //GameScene불러오기
                Managers.Scene.LoadScene(Define.EScene.TutorialScene);  // 게임 시작
            }
        });
    }

    // For 최혁도, TODO: 게임 이어하기, 활성화된 세이브 포인트 씬부터 시작

    public void OpenOption()
    {
        Debug.Log("열림");
        OptionPanel.SetActive(true);
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

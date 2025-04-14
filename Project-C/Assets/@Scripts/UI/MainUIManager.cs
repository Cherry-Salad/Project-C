using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainUIManager : MonoBehaviour
{
    public GameObject OptionPanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OptionPanel.gameObject.SetActive(!OptionPanel.gameObject.activeSelf);
        }
    }

    public void StartGame()
    {
        //SceneManager.LoadScene(1); //GameScene불러오기
        Managers.Scene.LoadScene(Define.EScene.TutorialScene);  // TODO: 활성화된 세이브 포인트 씬부터 시작
    }

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

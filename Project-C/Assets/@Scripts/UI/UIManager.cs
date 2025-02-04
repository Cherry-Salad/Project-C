using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class UIManager : MonoBehaviour
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
        SceneManager.LoadScene(1);
    }

    public void OpenOption()
    {
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

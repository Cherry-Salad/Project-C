using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;

public class DUIManager : MonoBehaviour
{
    [Header("Startup/UI")]
    [SerializeField] private GameObject _optionPanel;
    public GameObject OptionPanel => _optionPanel;
    [SerializeField] private GameObject InventoryPanel;
    [SerializeField] private GameObject SkillPanel;
    [SerializeField] private GameObject MapPanel;

    GameObject activePanel = null;
    Dictionary<KeyCode, GameObject> DUIPanel;

    private void Awake()
    {
        AudioManager.Instance.StartBGM(AudioManager.Instance.background);
        DUIPanel = new Dictionary<KeyCode, GameObject>
        {
            {KeyCode.I, InventoryPanel},
            {KeyCode.K, SkillPanel},
            {KeyCode.M, MapPanel}
        };
    }

    private void Update()
    {
        //타이틀 씬에서는 키 입력처리 X
        if (SceneManager.GetActiveScene().name == EScene.MainScene.ToString())
            return;

        // ESC 누르면 옵션 패널 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePanel(OptionPanel);
            Time.timeScale = activePanel == null ? 1f : 0f;
            return;
        }

        //I, K, M 키로 해당 패널 토글 (인벤, 스킬, 맵)
        foreach (var panel in DUIPanel)
        {
            if (Input.GetKeyDown(panel.Key))
            {
                TogglePanel(panel.Value);
                AudioManager.Instance.PlaySFX(AudioManager.Instance.button);
                return;
            }
        }
    }

    void TogglePanel(GameObject panel)
    {
        if (activePanel == panel) // 같은 패널을 누르면 닫기
        {
            CloseActivePanel();
        }
        else
        {
            OpenPanel(panel);
        }
    }

    void OpenPanel(GameObject panel)
    {
        // 이미 열려져있는 패널 닫기
        if (activePanel != null && activePanel != panel)
        {
            activePanel.SetActive(false);
        }

        // 현재 패널 열기
        panel.SetActive(true);
        activePanel = panel;
    }

    public void CloseActivePanel()
    {
        if (activePanel != null)
        {
            activePanel.SetActive(false);
            activePanel = null;
        }
            Time.timeScale = 1f;
    }

    public void ReturnToTitle() //수정 필요 - 씬 전환 후 MainPanel의 OptionPanel의 오브젝트가 사라짐
    {
        if (SceneManager.GetActiveScene().name == EScene.MainScene.ToString())
            return;

        Time.timeScale = 1f;

        if (OptionPanel != null && OptionPanel.activeSelf)
            OptionPanel.SetActive(false);

        Managers.Game.Save();
        Managers.Scene.LoadScene(EScene.MainScene);

        OptionPanel.SetActive(false);
    }


    public void QuitGame()
    {
        Debug.Log("QuitGame 호출됨");
        Application.Quit();
    }

    public void RespawnPlayer()
    {
        Player playerComp = FindObjectOfType<Player>();
        if (playerComp == null)
            return;

        Managers.Game.Save();   // 데이터 저장

        if (Managers.Game.GameData.CurrentSavePoint.SceneType == EScene.None)
        {
            Debug.LogError("와 파피루스");
            return;
        }

        Managers.Scene.LoadScene(Managers.Game.GameData.CurrentSavePoint.SceneType);
        OptionPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
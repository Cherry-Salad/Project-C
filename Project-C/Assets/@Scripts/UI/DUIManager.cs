using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DUIManager : MonoBehaviour
{
    public GameObject OptionPanel;
    public GameObject InventoryPanel;
    public GameObject SkillPanel;
    public GameObject MapPanel;

    private GameObject activePanel = null;

    private Dictionary<KeyCode, GameObject> DUIPanel;

    private void Start()
    {
        DUIPanel = new Dictionary<KeyCode, GameObject>
        {
            {KeyCode.I, InventoryPanel},
            {KeyCode.K, SkillPanel},
            {KeyCode.M, MapPanel}
        };
    }

    private void Update()
    {
        //키를 눌렀을 때 토글
        foreach (var entry in DUIPanel)
        {
            if (Input.GetKeyDown(entry.Key))
            {
                TogglePanel(entry.Value);
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (activePanel == OptionPanel)
            {
                ClosePanel();
            }
            else if (activePanel != null) ClosePanel();
            else
            {
                OpenPanel(OptionPanel);
                activePanel = OptionPanel;
            }
        }
    }
    void TogglePanel(GameObject panel)
    {
        if (activePanel == panel) // 같은 패널을 누르면 닫기
        {
            ClosePanel();
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

    void ClosePanel()
    {
        if (activePanel != null)
        {
            activePanel.SetActive(false);
            activePanel = null;
        }
    }
}
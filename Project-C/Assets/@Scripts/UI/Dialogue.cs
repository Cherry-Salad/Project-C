using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

[System.Serializable]
public class DialogueData
{
    public string npcName;
    public string[] dialogues;
}

public class Dialogue : MonoBehaviour
{
    [SerializeField] private GameObject NPCCanvas;       // NPC UI Canvas
    [SerializeField] private GameObject StorePanel;      // 상점 UI
    [SerializeField] private GameObject DialoguePanel;   // 대화문 UI
    [SerializeField] private TMP_Text npcNameText;       // NPC 이름 UI
    [SerializeField] private TMP_Text dialogueText;      // 대화 내용 UI
    [SerializeField] private string dialogueFileName;    // JSON 파일명
    [SerializeField] private NPCMovement npcMovement;

    private DialogueData dialogueData;
    private int currentDialogueIndex = 0;
    private bool dialougeActivated;
    private bool canStoreOpen = false;

    void Start()
    {
        LoadDialogueData();
        if (dialogueData.npcName == "빨간모자") canStoreOpen = true; //RedCap NPC는 상점 기능 사용 가능
    }

    void LoadDialogueData()
    {
        //string filePath = Path.Combine(Application.dataPath, "@Resources/Data/", dialogueFileName + ".json");

        //if (File.Exists(filePath))
        //{
        //    string json = File.ReadAllText(filePath);
        //    dialogueData = JsonUtility.FromJson<DialogueData>(json);
        //    //Debug.Log($"JSON 로드 성공: {dialogueData.npcName}");
        //}
        //else
        //{
        //    Debug.LogError($"JSON 로드 실패: {filePath}");
        //}
        var json = Managers.Resource.Load<TextAsset>("Dialogue_Redcap");
        if (json == null)
            Debug.LogWarning("와 샌즈");

        dialogueData = JsonUtility.FromJson<DialogueData>(json.text);
    }

    void Update()
    {
        if (dialougeActivated == true && KeySetting.GetKeyDown(KeyInput.NEXT))
        {
            NPCCanvas.SetActive(true);
            ShowDialogue();
        }

        if (canStoreOpen && Input.GetKeyDown(KeyCode.B)) ShowStore();
    }

    void ShowDialogue()
    {
        if (dialogueData == null) return;

        if (npcMovement != null) npcMovement.SetState(NPCState.Idle);
        if (currentDialogueIndex < dialogueData.dialogues.Length)
        {
            dialogueText.text = dialogueData.dialogues[currentDialogueIndex];
            npcNameText.text = dialogueData.npcName;
            currentDialogueIndex++;
        }
        else
        {
            EndDialogue();
        }
    }
    void EndDialogue()
    {
        NPCCanvas.SetActive(false);
        StorePanel.SetActive(false);
        DialoguePanel.SetActive(true);
        currentDialogueIndex = 0;
        if (npcMovement != null)
        {
            npcMovement.ExitDialogue(); //원래 이동 방향을 바라보기
        }
    }

    void ShowStore()
    {
        if(!StorePanel.activeSelf)
        {
            StorePanel.SetActive(true);
            DialoguePanel.SetActive(false);
        }
        else
        {
            EndDialogue();
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            dialougeActivated = true;
            currentDialogueIndex = 0;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            dialougeActivated = false;
            EndDialogue();
        }
    }
}

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
    [SerializeField] private GameObject dialogueCanvas;  // 대화 UI Canvas
    [SerializeField] private TMP_Text npcNameText;       // NPC 이름 UI
    [SerializeField] private TMP_Text dialogueText;      // 대화 내용 UI
    [SerializeField] private string dialogueFileName;    // JSON 파일명

    private DialogueData dialogueData;
    private int currentDialogueIndex = 0;
    private bool dialougeActivated;
    private NPCMovement npcMovement;

    void Start()
    {
        npcMovement = GetComponent<NPCMovement>();
        LoadDialogueData();
    }

    void LoadDialogueData()
    {
        string filePath = Path.Combine(Application.dataPath, "@Resources/Data/", dialogueFileName + ".json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            dialogueData = JsonUtility.FromJson<DialogueData>(json);
            Debug.Log($"JSON 로드 성공: {dialogueData.npcName}");
        }
        else
        {
            Debug.LogError($"JSON 로드 실패: {filePath}");
        }
    }

    void Update()
    {
        if (dialougeActivated == true && Input.GetButtonDown("Interact"))
        {
            dialogueCanvas.SetActive(true);
            ShowDialogue();
        }
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
        dialogueCanvas.SetActive(false);
        currentDialogueIndex = 0;
        if (npcMovement != null)
        {
            npcMovement.ExitDialogue(); //원래 이동 방향을 바라보기
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

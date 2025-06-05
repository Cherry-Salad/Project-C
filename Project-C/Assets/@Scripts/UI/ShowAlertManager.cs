using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShowAlertManager : MonoBehaviour
{
    public static ShowAlertManager Instance { get; private set; }

    [SerializeField] private GameObject saveAlertPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowSaveAlert()
    {
        StartCoroutine(ShowAlertCoroutine(saveAlertPanel));
    }


    private IEnumerator ShowAlertCoroutine(GameObject obj)
    {
        // 오브젝트가 비활성화 상태이면 활성화
        obj.SetActive(true);

        // 1초 동안 대기
        yield return new WaitForSeconds(1f);

        // 오브젝트 제거
        obj.SetActive(false);
    }
}

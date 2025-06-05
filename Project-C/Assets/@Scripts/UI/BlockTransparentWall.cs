using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockTransparentWall : MonoBehaviour
{
    public GameObject AlertPanel;
    

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ShowSaveAlert();
        }
    }
    public void ShowSaveAlert()
    {
        StartCoroutine(ShowAlertCoroutine(AlertPanel));
    }


    private IEnumerator ShowAlertCoroutine(GameObject obj)
    {
        // 오브젝트가 비활성화 상태이면 활성화
        obj.SetActive(true);

        // 1초 동안 대기
        yield return new WaitForSeconds(2f);

        // 오브젝트 제거
        obj.SetActive(false);
    }

}

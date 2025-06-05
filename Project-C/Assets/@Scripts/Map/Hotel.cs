using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hotel : MonoBehaviour
{
    private bool _isActiveTrggier = false ;
    public GameObject SpaceMark;

    
    void Update()
    {

        if (_isActiveTrggier == true && KeySetting.GetKeyDown(KeyInput.NEXT))
        {
            Fade.Instance.FadeIn(() =>
            {
                SceneManager.LoadScene("EndingScene");
            }
            );
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isActiveTrggier = true;
            SpaceMark.SetActive(true);
            StartCoroutine(PressKeyDisplay());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isActiveTrggier = false;
            if (SpaceMark != null) SpaceMark.SetActive(false);
        }
    }

    private IEnumerator PressKeyDisplay()
    {
        float currentY = SpaceMark.transform.position.y;
        float initY = currentY;
        float dir = 1f;               // 1 == 위로, -1 == 아래로
        float distance = 0.1f;        // 위아래로 움직일 거리
        float speed = 1f;             // 이동 속도

        while (true)
        {
            currentY += dir * speed * Time.deltaTime;

            if (currentY >= initY + distance)
            {
                currentY = initY + distance;
                dir = -1f;
            }
            else if (currentY <= initY - distance)
            {
                currentY = initY - distance;
                dir = 1f;
            }

            Vector3 pos = SpaceMark.transform.position;
            SpaceMark.transform.position = new Vector3(pos.x, currentY, pos.z);

            yield return null; // 한 프레임 대기
        }
    }

}

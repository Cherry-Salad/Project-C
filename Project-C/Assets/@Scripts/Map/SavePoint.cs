using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SavePoint : InitBase
{
    public GameObject SpaceMark;
    public BoxCollider2D Collider { get; private set; }
    public EScene SceneType { get; private set; }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Collider = GetComponent<BoxCollider2D>();

        // 필터링
        LayerMask includeLayers = 0;
        includeLayers.AddLayer(ELayer.Player);
        Collider.includeLayers = includeLayers;

        return true;
    }

    public void SetInfo(Vector3 pos, EScene type)
    {
        transform.position = pos;
        SceneType = type;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SpaceMark.SetActive(true);
            StartCoroutine(PressKeyDisplay());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SpaceMark.SetActive(false);
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

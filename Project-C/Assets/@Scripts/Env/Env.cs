using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env : BaseObject
{
    public Data.EnvData Data;
    public float Hp { get; set; }
    public GameObject SpaceMark;


    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = Define.EObjectType.Env;

        return true;
    }

    public void SetInfo(int dataId, Vector3 pos, bool flipX = false, bool flipY = false)
    {
        Data = Managers.Data.EnvDataDic[dataId];
        Hp = Data.Hp;
        transform.position = pos;

        if (flipX)
        {
            //Debug.Log("flipX");
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
        
        if (flipY)
        {
            //Debug.Log("flipY");
            Vector3 localScale = transform.localScale;
            localScale.y *= -1;
            transform.localScale = localScale;
        }
    }

    public override void OnDamaged(float damage, bool ignoreInvincibility = false, Collider2D attacker = null)
    {
        base.OnDamaged(damage, ignoreInvincibility, attacker);

        if (attacker != null)
        {
            BaseObject obj = attacker.GetComponent<BaseObject>();
            if (obj == null)
                return;

            if (obj.ObjectType == Define.EObjectType.Player)
            {
                Hp--;
                StartCoroutine(CoShake());
            }
        }
    }

    public override void OnDied()
    {
        base.OnDied();
        Managers.Map.DespawnObject(this);
        Managers.Resource.Destroy(gameObject);  
    }

    public virtual void OnPickedUp() 
    {
        // For 최혁도 선배, TODO: 아이템 획득하는 효과음 재생
        AudioManager.Instance.PlaySFX(AudioManager.Instance.AcquireItem); // 획득 효과음
    }

    protected IEnumerator CoShake(float duration = 0.2f)
    {
        Vector3 pos = transform.position;
        float magnitude = 0.05f;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float x = Random.Range(-magnitude, magnitude);
            transform.position = pos + new Vector3(x, 0, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = pos; // 원래 위치로 복구

        if (Hp <= 0)
            OnDied();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (SpaceMark != null)
        {
            SpaceMark.SetActive(true);
            StartCoroutine(PressKeyDisplay());
        }

        OnPlayerEnter(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (SpaceMark != null) SpaceMark.SetActive(false);

        OnPlayerExit(collision);
    }

    protected virtual void OnPlayerEnter(Collider2D other){ }
    protected virtual void OnPlayerExit(Collider2D other) { }


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

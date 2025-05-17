using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env : BaseObject
{
    public Data.EnvData Data;
    public float Hp { get; set; }
    private GameObject _loopSfxGO;


    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = Define.EObjectType.Env;

        // 등장하자마자 3D 루프 사운드 시작
        var src = AudioManager.Instance.Play3DSFXAt(
            AudioManager.Instance.AroundItem,
            transform.position
        );
        src.loop = true;
        _loopSfxGO = src.gameObject;

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
        // 루프 중인 3D 사운드가 있으면 중지
        if (_loopSfxGO != null)
        {
            Destroy(_loopSfxGO);
            _loopSfxGO = null;
        }

        AudioManager.Instance.PlaySFX(AudioManager.Instance.AcquireItem); // 획득 효과음
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && _loopSfxGO == null)
        {
            // AroundItem 을 루프 모드로 3D 재생
            var src = AudioManager.Instance.Play3DSFXAt(
                AudioManager.Instance.AroundItem,
                transform.position
            );
            src.loop = true;
            _loopSfxGO = src.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && _loopSfxGO != null)
        {
            Destroy(_loopSfxGO);
            _loopSfxGO = null;
        }
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
}

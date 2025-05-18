using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxHpUpItem : Env
{
    AudioSource _3DSFXSource;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = Define.EObjectType.Env;

        _3DSFXSource = GetComponent<AudioSource>();
        _3DSFXSource.loop = true;

        return true;
    }

    public override void OnDamaged(float damage, bool ignoreInvincibility = false, Collider2D attacker = null) {}

    public override void OnPickedUp()
    {
        if (_3DSFXSource.isPlaying)
            _3DSFXSource.Stop();

        base.OnPickedUp();
        Managers.Game.Player.MaxHp++;
        Managers.Game.Player.Hp++;
        Managers.Game.Player.TriggerOnHpChanged();
        OnDied();
    }

    public override void OnDied()
    {
        base.OnDied();
        Managers.Map.DespawnObject(this);
        Managers.Resource.Destroy(gameObject);  // For 최혁도, TODO: 부숴지는 연출
    }

    protected override void OnPlayerEnter(Collider2D other)
    {
        if (!_3DSFXSource.isPlaying)
            _3DSFXSource.Play();
    }

    protected override void OnPlayerExit(Collider2D other)
    {
        // 플레이어가 범위 밖으로 나갔을 때 사운드 정지
        if (_3DSFXSource.isPlaying)
            _3DSFXSource.Stop();
    }
}

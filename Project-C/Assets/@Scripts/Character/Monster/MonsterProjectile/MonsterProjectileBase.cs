using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class MonsterProjectileBase : BaseObject
{
    protected Data.ProjectileData Data;

    protected float Speed;
    protected Vector2 Dir;
    public bool _isActive;
    public bool _isLoad;
    
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        DefaultGravityScale = 0f;
        _isActive = false;

        return true;
    }

    public bool SetData(int dataID)
    {
        if (Managers.Data.ProjectileDataDic.TryGetValue(dataID, out Data) == false) return false;
        
        _isLoad = true;
        Speed = 0;

        return true;
    }

    public virtual void ShootingProjectile(Vector2 pos, Vector2 dir)
    {
        _isActive = true;
        
        Dir = dir.normalized;
        
        Speed = Data.BaseSpeed;
        DefaultGravityScale = Data.DefaultGravity;
        
        this.transform.position = pos;
        this.Rigidbody.velocity = new Vector2(Dir.x *  Speed, Dir.y * Speed);

        StartCoroutine(ChangingVector());
        StartCoroutine(StartLifeTime());
    }

    public virtual void ShootingProjectile(GameObject target)
    {

    }

    protected virtual IEnumerator ChangingVector()
    {
        yield return null;
    }

    protected IEnumerator StartLifeTime()
    {
        yield return new WaitForSeconds(Data.LifeTime);
        EndOfProjectile();
    }

    public void EndOfProjectile()
    {
        _isActive = false;
        Speed = 0;
        Dir = Vector2.zero;
        this.gameObject.SetActive(false);
    }

    protected void OnTriggerEnter2D(Collider2D collider)
    {
        int layerMask = ~LayerMask.GetMask("Monster", "Player", "HitBox", "Default");

        if(((1 << collider.gameObject.layer) & layerMask) != 0)
        {
            EndOfProjectile();
        }
    }

}

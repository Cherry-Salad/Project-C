using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObject : InitBase
{
    public Define.EObjectType ObjectType { get; protected set; } = Define.EObjectType.None;
    
    public SpriteRenderer SpriteRenderer { get; protected set; }
    public Rigidbody2D Rigidbody { get; protected set; }
    public CapsuleCollider2D Collider { get; protected set; }
    public Animator Animator { get; protected set; }

    float _gravityScale;
    public float DefaultGravityScale
    {
        get { return _gravityScale; }
        protected set 
        {
            _gravityScale = value;
            Rigidbody.gravityScale = DefaultGravityScale;
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        SpriteRenderer = GetComponent<SpriteRenderer>();
        Rigidbody = GetComponent<Rigidbody2D>();
        Collider = GetComponent<CapsuleCollider2D>();
        Animator = GetComponent<Animator>();

        Rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;  // Z축 고정

        return true;
    }

    /// <summary>
    /// 피격 판정한다.
    /// </summary>
    /// <param name="damage">대미지 값</param>
    /// <param name="ignoreInvincibility">무적 상태를 무시하고 피격 받을지 확인한다. 주로 함정에 당했을 때 사용한다.</param>
    /// <param name="attacker">가해자(이런 번역 맞나..?)</param>
    public virtual void OnDamaged(float damage, bool ignoreInvincibility = false, Collider2D attacker = null)
    {
        //Debug.Log("OnDamaged");
    }
}

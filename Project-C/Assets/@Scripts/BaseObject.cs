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

        DefaultGravityScale = 2f;   // 중력을 1로 설정하니까 낙하할 때 시원찮더라~
        Rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;  // Z축 고정

        return true;
    }
}

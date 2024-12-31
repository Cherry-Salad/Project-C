using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BaseObject : InitBase
{
    public Define.EObjectType ObjectType { get; protected set; } = Define.EObjectType.None;
    
    public SpriteRenderer SpriteRenderer { get; protected set; }
    public Rigidbody2D Rigidbody { get; protected set; }
    public CapsuleCollider2D Collider { get; protected set; }
    public Animator Animator { get; protected set; }

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
}

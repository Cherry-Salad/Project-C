using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Creature : BaseObject
{
    [SerializeField]
    ECreatureState _state = ECreatureState.None;    // 테스트하기 편하게 인스펙터 창에서도 확인 가능
    public ECreatureState State
    {
        get { return _state; }
        protected set 
        {
            if (_state != value)
            {
                _state = value;
                UpdateAnimation();
            }
        }
    }

    public Vector2 MoveDir { get; protected set; } = Vector2.right;

    bool _lookLeft = false;
    public bool LookLeft
    {
        get { return _lookLeft; }
        set 
        {
            if (_lookLeft != value)
            {
                _lookLeft = value;
                SpriteRenderer.flipX = _lookLeft;
            }
        }
    }

    public float MoveSpeed { get; protected set; }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        State = ECreatureState.Idle;

        return true;
    }

    void Update()
    {
        UpdateController();
    }

    protected virtual void UpdateAnimation()
    {
        switch (State)  // TODO: 애니메이션 연결
        {
            case ECreatureState.Idle:
                Animator.Play("Idle");
                break;
            case ECreatureState.Run:
                Animator.Play("Run");
                break;
            case ECreatureState.Jump:
                break;
            case ECreatureState.DoubleJump:
                break;
            case ECreatureState.Skill:
                break;
            case ECreatureState.Dash:
                Animator.Play("Dash");
                break;
            case ECreatureState.WallClimbing:
                break;
            case ECreatureState.WallCling:
                break;
            case ECreatureState.Hurt:
                break;
            case ECreatureState.Dead:
                break;
        }
    }

    protected virtual void UpdateController()
    {
        switch (State)
        {
            case ECreatureState.Idle:
                UpdateIdle();
                break;
            case ECreatureState.Run:
                UpdateRun();
                break;
        }
    }

    protected virtual void UpdateIdle() 
    {

    }
    
    protected virtual void UpdateRun() 
    {
        MoveDir = MoveDir.normalized;   // 방향 정규화
        Rigidbody.MovePosition(Rigidbody.position + MoveDir * MoveSpeed * Time.deltaTime);
    }

    protected virtual void OnDash()
    {
        State = ECreatureState.Dash;
        StartCoroutine(CoDash());
    }

    IEnumerator CoDash()
    {
        MoveDir = MoveDir.normalized;   // 방향 정규화
        float distance = 3f;    // 대시 거리
        float dashSpeed = MoveSpeed * 3f;

        RaycastHit2D hit = Physics2D.Raycast(Rigidbody.position, MoveDir, distance, LayerMask.GetMask("Impassable"));

        // 목적지 설정
        Vector2 destPos = hit.collider != null
        ? hit.point - MoveDir * 0.1f    // 충돌 지점에서 약간 떨어진 위치
        : Rigidbody.position + MoveDir * distance;  // 원래 목적지

        Vector2 dir = destPos - Rigidbody.position; // 목적지로 가기 위한 방향과 거리

        // 대시
        while (dir.magnitude > 0.01f)
        {
            Rigidbody.position = Vector2.MoveTowards(Rigidbody.position, destPos, dashSpeed * Time.deltaTime);
            dir = destPos - Rigidbody.position; 
            yield return null;
        }

        State = ECreatureState.Idle;
    }
}
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

    void FixedUpdate()
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
            case ECreatureState.WallCling:
                Animator.Play("WallSlide");
                break;
            case ECreatureState.WallClimbing:
                Animator.Play("WallSlide");
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
            case ECreatureState.WallCling:
                UpdateWallCling();
                break;
            case ECreatureState.WallClimbing:
                UpdateWallClimbing();
                break;
        }
    }

    protected virtual void UpdateIdle()
    {
        Rigidbody.velocity = new Vector2(Vector2.zero.x, Rigidbody.velocity.y);
    }
    
    protected virtual void UpdateRun()
    {
        // 벽 감지
        if (CheckWall())
            State = ECreatureState.WallCling;
        
        Rigidbody.velocity = new Vector2(MoveDir.x * MoveSpeed, Rigidbody.velocity.y);
    }

    protected virtual void UpdateWallCling()
    {
    }

    protected virtual void UpdateWallClimbing()
    {
        if (CheckWall() == false)
        {
            State = ECreatureState.Idle;
            return;
        }

        float wallClimbingSpeed = MoveSpeed / 3f;
        Rigidbody.velocity = Vector2.up * wallClimbingSpeed;
        Debug.Log(Rigidbody.velocity);
    }

    protected virtual void OnDash()
    {
        // 벽에 매달린 상태에서 방향키를 누르지 않고 대시하면 반대 방향으로 대시한다
        if (State == ECreatureState.WallCling)
        {
            LookLeft = !LookLeft;
            MoveDir = MoveDir.x < 0 ? Vector2.right : Vector2.left;
        }

        State = ECreatureState.Dash;
        StartCoroutine(CoDash());
    }

    IEnumerator CoDash()
    {
        float distance = 3f;    // 대시 거리
        float dashSpeed = MoveSpeed * 3f;

        // 해당 방향으로 distance만큼 대시로 지나갈 수 있는 지
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

            // 벽 감지
            if (CheckWall())
            {
                State = ECreatureState.WallCling;
                yield break;
            }

            yield return null;
        }

        State = ECreatureState.Idle;
    }

    protected bool CheckWall()
    {
        // 벽 감지
        RaycastHit2D wall = Physics2D.Raycast(Rigidbody.position, MoveDir, 0.3f, LayerMask.GetMask("Wall"));
        return wall.collider != null;
    }
}
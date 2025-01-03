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
        //Debug.Log(Rigidbody.velocity);
        UpdateController();
    }

    protected virtual void UpdateAnimation()
    {
        // State pattern
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
        // State pattern
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
        if (CheckAllGround())
        {
            // 평평한 바닥과 경사진 바닥 모두 감지
            Rigidbody.gravityScale = 0f;    // 캐릭터가 경사진 바닥에서 미끄러지는 효과가 있어 중력을 임시로 없앴다
        }
        else
        {
            // 공중에 있다면 원래대로 중력 적용
            Rigidbody.gravityScale = DefaultGravityScale;
        }

        Rigidbody.velocity = new Vector2(Vector2.zero.x, Rigidbody.velocity.y);
    }

    protected virtual void UpdateRun()
    {
        // 캐릭터가 공중에 있으면서 벽을 감지
        // TODO: 바닥에 있다면 점프를 사용해서 벽 타기
        if (CheckGround() == false && CheckWall())
            State = ECreatureState.WallCling;

        if (CheckAllGround())   
        {
            // 평평한 바닥과 경사진 바닥 모두 감지
            Rigidbody.gravityScale = 0f;    // 캐릭터가 경사진 바닥에서 미끄러지는 효과가 있어 중력을 임시로 없앴다
            Rigidbody.velocity = MoveDir * MoveSpeed;   // Rigidbody.velocity.y를 0으로 하지 않는다면, 캐릭터가 경사진 바닥에서 뛸 때 위로 튀어오른다
        }
        else
        {
            // 공중에 있다면 원래대로 중력 적용
            Rigidbody.gravityScale = DefaultGravityScale;
            Rigidbody.velocity = new Vector2(MoveDir.x * MoveSpeed, Rigidbody.velocity.y);
        }
    }

    protected virtual void UpdateWallCling()
    {
        // 캐릭터가 바닥에 닿거나 벽을 감지하지 못한 경우
        if (CheckGround() || CheckWall() == false)
        {
            State = ECreatureState.Idle;
            return;
        }

        // 천천히 아래로 떨어지는 속도 유지
        float speed = MoveSpeed / 2f;
        Rigidbody.velocity = Vector2.down * speed;
    }

    protected virtual void UpdateWallClimbing()
    {
        // 벽을 감지하지 못한 경우
        if (CheckWall() == false)
        {
            State = ECreatureState.Idle;
            return;
        }

        float wallClimbingSpeed = MoveSpeed / 3f;
        Rigidbody.velocity = Vector2.up * wallClimbingSpeed;
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

        // 현재 위치와 목적지 간의 이동 방향과 남은 거리
        Vector2 dir = destPos - Rigidbody.position; 

        // 대시
        while (dir.magnitude > 0.01f)
        {
            Rigidbody.position = Vector2.MoveTowards(Rigidbody.position, destPos, dashSpeed * Time.deltaTime);
            dir = destPos - Rigidbody.position;

            // 캐릭터가 공중에 있으면서 벽을 감지
            if (CheckGround() == false && CheckWall())
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
        float wallCheckDistance = Collider.bounds.extents.x + 0.1f; // 벽 감지 거리, Collider 크기 절반에 여유값(0.1f)을 추가
        RaycastHit2D wall = Physics2D.Raycast(Rigidbody.position, MoveDir, wallCheckDistance, LayerMask.GetMask("Wall"));   // 캐릭터가 바라보고 있는 방향에 벽을 감지하는가
        return wall.collider != null;
    }

    protected bool CheckGround()
    {
        // 벽에 매달린 상태에서 평평한 바닥 감지, 캐릭터 밑의 바닥만 감지해야 한다.
        float groundCheckDistance = Collider.bounds.extents.y + 0.1f;   // 바닥 감지 거리
        RaycastHit2D ground = Physics2D.Raycast(Rigidbody.position, Vector2.down, groundCheckDistance, LayerMask.GetMask("Impassable"));
        return ground.collider != null;
        //return Mathf.Abs(Rigidbody.velocity.x) < 0.1f && Mathf.Abs(Rigidbody.velocity.y) < 0.1f;  // 이건 바닥 감지가 제대로 되지 않아 지웠다
    }

    protected bool CheckAllGround()
    {
        // 벽에 매달리지 않은 상태에서 평평한 바닥과 경사진 바닥을 감지한다.
        // CheckGround로는 경사진 바닥 감지가 어려워서, Rigidbody에 닿은 레이어를 바탕으로 바닥을 감지한다.
        // 바닥 레이어를 만들면 Wall 레이어와 완벽히 구분하기 어려워서, 따로 만들지 않았다.
        // Wall 레이어의 타일은 무조건 평평한 타일이다.
        if (Rigidbody.IsTouchingLayers(LayerMask.GetMask("Impassable")) && Rigidbody.IsTouchingLayers(LayerMask.GetMask("Wall")) == false)
            return true;

        return false;
        //float groundCheckDistance = Collider.bounds.extents.y + 0.1f;   // 바닥 감지 거리
        //RaycastHit2D ground = Physics2D.Raycast(Rigidbody.position, Vector2.down, groundCheckDistance, LayerMask.GetMask("Impassable"));  // 이 방식으론 경사진 바닥은 감지가 안된다. 서럽다.

        //var p = Vector2.Perpendicular(ground.normal);
        //var angle = Vector2.Angle(ground.normal, Vector2.up);

        //return angle != 0;
    }
}
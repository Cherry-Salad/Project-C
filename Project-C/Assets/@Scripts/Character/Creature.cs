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
        Managers.Resource.LoadAsync<Object>("Dust");    // Test

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
                Animator.Play("Jump");
                break;
            case ECreatureState.DoubleJump:
                break;
            case ECreatureState.Skill:
                break;
            case ECreatureState.Dash:
                Animator.Play("Dash");
                ShowDustEffect();
                break;
            case ECreatureState.WallCling:
                Animator.Play("WallSlide");
                ShowDustEffect();
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
            case ECreatureState.Jump:
                UpdateJump();
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
        if (CheckGround())
        {
            // 바닥 감지
            Rigidbody.gravityScale = 0f;    // 캐릭터가 경사진 바닥에서 미끄러지는 효과가 있어 중력을 임시로 없앴다
            Rigidbody.velocity = Vector2.zero;
        }
        else
        {
            // 공중에 있다면 점프로 전환
            State = ECreatureState.Jump;
        }
    }

    protected virtual void UpdateRun()
    {
        if (CheckGround())
        {
            // 바닥 감지
            Rigidbody.gravityScale = 0f;    // 캐릭터가 경사진 바닥에서 미끄러지는 효과가 있어 중력을 임시로 없앴다
            Rigidbody.velocity = MoveDir * MoveSpeed;   // Rigidbody.velocity.y를 0으로 하지 않는다면, 캐릭터가 경사진 바닥에서 뛸 때 위로 튀어오른다
        }
        else
        {
            // 공중에 있다면 점프로 전환
            State = ECreatureState.Jump;
        }
    }

    protected virtual void UpdateJump()
    {
        bool OnGround = CheckGround();

        // TODO: 바닥에 있다면 점프를 사용해서 벽 타기
        // 캐릭터가 공중에 있으면서 벽을 감지
        if (OnGround == false && CheckWall())
        {
            State = ECreatureState.WallCling;
            return;
        }
        else if (OnGround)
        {
            State = ECreatureState.Idle;
            return;
        }
        // TODO: 이단 점프 가능

        // 공중에서 낙하 중 이동 방향에 장애물이 있으면 제자리에서 걷는 버그 방지
        // 수평 속도를 0으로 설정하고 즉시 낙하
        float distance = Collider.bounds.extents.x + 0.1f;
        float velocityX = (CheckObstacle(MoveDir, distance) == false) ? MoveDir.x * MoveSpeed : 0f;

        // 공중에 있다면 원래대로 중력 적용
        Rigidbody.gravityScale = DefaultGravityScale;
        Rigidbody.velocity = new Vector2(velocityX, Rigidbody.velocity.y);
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

        // 대시하는 동안 물리적인 현상은 무시한다
        Rigidbody.gravityScale = 0f;    
        Rigidbody.velocity = Vector2.zero;

        // 움직이는 방향이 없다면 LookLeft을 기준으로 설정
        if (MoveDir.normalized.x == 0)
            MoveDir = LookLeft ? Vector2.left : Vector2.right;

        State = ECreatureState.Dash;
        StartCoroutine(CoDash());
    }

    IEnumerator CoDash()
    {
        float distance = 3f;    // 대시 거리
        float dashSpeed = MoveSpeed * 3f;

        // 벽이나 다른 물체에 의해 막힐 수 있으므로 해당 방향으로 최대한 갈 수 있는 거리를 구한다
        RaycastHit2D hit = CheckObstacle(MoveDir, distance);

        // 목적지 설정
        Vector2 destPos = hit.collider != null
        ? hit.point - MoveDir * 0.1f    // 충돌 지점에서 약간 떨어진 위치
        : Rigidbody.position + MoveDir * distance;  // 원래 목적지

        // 현재 위치와 목적지 간의 이동 방향과 남은 거리
        Vector2 dir = destPos - Rigidbody.position;

        // 바닥 감지
        bool OnGround = CheckGround();

        // 대시
        while (dir.magnitude > 0.01f)
        {
            Rigidbody.position = Vector2.MoveTowards(Rigidbody.position, destPos, dashSpeed * Time.deltaTime);
            dir = destPos - Rigidbody.position;

            OnGround = CheckGround();
            if (OnGround == false && CheckWall())
            {
                // 캐릭터가 공중에 있으면서 벽을 감지
                State = ECreatureState.WallCling;
                yield break;
            }

            yield return null;
        }

        // 캐릭터가 공중에 있으면 점프로 전환
        State = OnGround ? ECreatureState.Idle : ECreatureState.Jump;
    }

    protected void ShowDustEffect()
    {
        Dust dust = Managers.Resource.Instantiate("Dust").GetComponent<Dust>();
        if (dust != null)
            dust.PlayEffect(this);
    }

    protected RaycastHit2D CheckObstacle(Vector2 dir, float distance)
    {
        // 벽이나 다른 물체같은 통행에 방해되는 장애물
        return Physics2D.Raycast(Rigidbody.position, dir, distance, LayerMask.GetMask("Wall", "Ground"));
    }

    protected bool CheckWall()
    {
        // 벽 감지
        float wallCheckDistance = Collider.bounds.extents.x + 0.1f; // 벽 감지 거리, Collider 크기 절반에 여유값(0.1f)을 추가
        return Physics2D.Raycast(Rigidbody.position, MoveDir, wallCheckDistance, LayerMask.GetMask("Wall"));    // 캐릭터가 바라보고 있는 방향에 벽을 감지하는가
    }

    protected bool CheckGround()
    {
        float groundCheckDistance = Collider.bounds.extents.y + 0.1f;   // 바닥 감지 거리
        LayerMask groundLayer = LayerMask.GetMask("Ground");
        Debug.DrawRay(Rigidbody.position, Vector2.down * groundCheckDistance, Color.red);

        // 캐릭터 밑의 평평한 바닥 감지
        if (Physics2D.Raycast(Rigidbody.position, Vector2.down, groundCheckDistance, groundLayer))
            return true;

        // 캐릭터 밑의 경사진 바닥 감지
        int rayCount = 5;   // Raycast 발사 횟수
        Vector2 leftDown = new Vector2(-0.5f, -1f); // 왼쪽 대각선
        Vector2 rightDown = new Vector2(0.5f, -1f); // 오른쪽 대각선

        // Vector2.down부터 leftDown
        for (int i = 1; i <= rayCount; i++)
        {
            float interpolationRatio = i / (float)rayCount; // 보간 비율 (0 ~ 1)
            Vector2 dir = Vector2.Lerp(Vector2.down, leftDown, interpolationRatio).normalized;
            Debug.DrawRay(Rigidbody.position, dir * groundCheckDistance, Color.blue);

            if (Physics2D.Raycast(Rigidbody.position, dir, groundCheckDistance, groundLayer))
                return true;
        }

        // Vector2.down부터 rightDown
        for (int i = 1; i <= rayCount; i++)
        {
            float interpolationRatio = i / (float)rayCount; // 보간 비율 (0 ~ 1)
            Vector2 dir = Vector2.Lerp(Vector2.down, rightDown, interpolationRatio).normalized;
            Debug.DrawRay(Rigidbody.position, dir * groundCheckDistance, Color.red);

            if (Physics2D.Raycast(Rigidbody.position, dir, groundCheckDistance, groundLayer))
                return true;
        }

        return false;
    }
}
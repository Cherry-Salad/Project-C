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
    public float JumpForce { get; protected set; }
    public float DoubleJumpForce { get; protected set; }    // 이단 점프할 때 추가적인 점프 힘

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        State = ECreatureState.Idle;
        Managers.Resource.LoadAsync<Object>("Dust");    // Test

        return true;
    }

    void Update()
    {
        Debug.Log(Rigidbody.velocity);
        UpdateController();
    }

    protected virtual void UpdateAnimation()
    {
        switch (State)  // State pattern
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
                Animator.Play("DoubleJump");
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
        switch (State)  // State pattern
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
            case ECreatureState.DoubleJump:
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
            // 캐릭터가 경사진 바닥에서 미끄러지는 효과가 있어 중력을 임시로 없앴다
            Rigidbody.gravityScale = 0f;
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
            // Rigidbody.velocity.y를 0으로 하지 않는다면, 캐릭터가 경사진 바닥에서 뛸 때 위로 튀어오른다
            Rigidbody.velocity = MoveDir * MoveSpeed;
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
        if (OnGround == false && CheckWall())
        {
            // 캐릭터가 공중에 있으면서 벽을 감지
            State = ECreatureState.WallCling;
            return;
        }
        else if (OnGround)
        {
            // 캐릭터가 바닥에 닿을 때
            State = ECreatureState.Idle;
            return;
        }
        // TODO: 이단 점프 가능

        // 낙하 중일 때 이동 방향에 장애물이 있으면 제자리에서 걷는 버그 방지: 수평 속도를 0으로 설정하고 즉시 낙하
        float distance = Collider.bounds.extents.x + 0.1f;
        bool noObstacles = CheckObstacle(MoveDir, distance, true).collider == null;
        float velocityX = (noObstacles) ? MoveDir.x * MoveSpeed : 0f;
        
        // 낙하 중이라면 기본 중력 적용
        Rigidbody.gravityScale = DefaultGravityScale;
        Rigidbody.velocity = new Vector2(velocityX, Rigidbody.velocity.y);
    }

    protected virtual void UpdateWallCling()
    {
        // 캐릭터가 정지 상태라면 LookLeft 기준으로 이동 방향 설정
        if (MoveDir.normalized.x == 0)
            MoveDir = LookLeft ? Vector2.left : Vector2.right;

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
        if (CheckWall() == false)
        {
            State = ECreatureState.Idle;
            return;
        }

        float wallClimbingSpeed = MoveSpeed / 3f;
        Rigidbody.velocity = Vector2.up * wallClimbingSpeed;
    }

    protected virtual void OnJump()
    {
        // 이단 점프 중에는 점프 불가능
        if (State != ECreatureState.DoubleJump && CheckGround())
        {
            // 공중이므로 기본 중력 적용
            Rigidbody.gravityScale = DefaultGravityScale;

            // 경사진 바닥에서도 점프를 할 수 있도록 velocity.x를 0으로 설정
            Rigidbody.velocity = new Vector2(0f, Rigidbody.velocity.y);
            Rigidbody.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
            
            State = ECreatureState.Jump;
        }
        else if (State == ECreatureState.Jump)
        {
            // 이단 점프
            // 공중이므로 기본 중력 적용
            Rigidbody.gravityScale = DefaultGravityScale;
            Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, JumpForce + DoubleJumpForce);
            State = ECreatureState.DoubleJump;
        }
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

        // 캐릭터가 정지 상태라면 LookLeft 기준으로 이동 방향 설정
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

    /// <summary>
    /// Dash, WallCling 등의 상태에 따라 다양한 먼지 효과를 연출한다
    /// </summary>
    protected void ShowDustEffect()
    {
        Dust dust = Managers.Resource.Instantiate("Dust").GetComponent<Dust>();
        if (dust != null)
            dust.PlayEffect(this);
    }

    #region 벽, 바닥, 장애물 감지
    /// <summary>
    /// 벽이나 벽처럼 통행에 방해되는 장애물을 감지한다
    /// </summary>
    /// <param name="dir">방향</param>
    /// <param name="distance">감지 거리</param>
    /// <param name="isDetailedCheck">더 섬세하게 감지할 것인지</param>
    /// <returns></returns>
    protected RaycastHit2D CheckObstacle(Vector2 dir, float distance, bool isDetailedCheck = false)
    {
        // 캐릭터가 정지 상태인지 확인
        if (dir == Vector2.zero)
            return default;

        LayerMask obstacleLayer = LayerMask.GetMask("Ground", "Wall");
        RaycastHit2D obstacle = Physics2D.Raycast(Rigidbody.position, dir, distance, obstacleLayer);
        Debug.DrawRay(Rigidbody.position, dir * distance, Color.green);

        if (obstacle.collider != null || isDetailedCheck == false)
            return obstacle;

        int rayCount = 5;   // Raycast 발사 횟수
        Vector2 dirUp = new Vector2(dir.x, 1f);
        Vector2 dirDown = new Vector2(dir.x, -1f);

        // Vector2.down부터 leftDown
        for (int i = 1; i <= rayCount; i++)
        {
            float interpolationRatio = i / (float)rayCount; // 보간 비율 (0 ~ 1)
            Vector2 rayDir = Vector2.Lerp(dir, dirUp, interpolationRatio).normalized;
            Debug.DrawRay(Rigidbody.position, rayDir * distance, Color.green);

            obstacle = Physics2D.Raycast(Rigidbody.position, rayDir, distance, obstacleLayer);
            if (obstacle.collider != null)
                return obstacle;
        }

        // Vector2.down부터 rightDown
        for (int i = 1; i <= rayCount; i++)
        {
            float interpolationRatio = i / (float)rayCount; // 보간 비율 (0 ~ 1)
            Vector2 rayDir = Vector2.Lerp(dir, dirDown, interpolationRatio).normalized;
            Debug.DrawRay(Rigidbody.position, rayDir * distance, Color.magenta);

            obstacle = Physics2D.Raycast(Rigidbody.position, rayDir, distance, obstacleLayer);
            if (obstacle.collider != null)
                return obstacle;
        }

        return default;
    }

    /// <summary>
    /// 캐릭터 앞에 벽이 있는가
    /// </summary>
    /// <returns></returns>
    protected bool CheckWall()
    {
        // 벽 감지
        float wallCheckDistance = Collider.bounds.extents.x + 0.1f; // 벽 감지 거리, Collider 크기 절반에 여유값 추가
        Debug.DrawRay(Rigidbody.position, MoveDir * wallCheckDistance, Color.red);
        return Physics2D.Raycast(Rigidbody.position, MoveDir, wallCheckDistance, LayerMask.GetMask("Wall"));
    }

    /// <summary>
    /// 캐릭터가 바닥에 있는가
    /// </summary>
    /// <returns></returns>
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
            Vector2 rayDir = Vector2.Lerp(Vector2.down, leftDown, interpolationRatio).normalized;
            Debug.DrawRay(Rigidbody.position, rayDir * groundCheckDistance, Color.red);

            if (Physics2D.Raycast(Rigidbody.position, rayDir, groundCheckDistance, groundLayer))
                return true;
        }

        // Vector2.down부터 rightDown
        for (int i = 1; i <= rayCount; i++)
        {
            float interpolationRatio = i / (float)rayCount; // 보간 비율 (0 ~ 1)
            Vector2 rayDir = Vector2.Lerp(Vector2.down, rightDown, interpolationRatio).normalized;
            Debug.DrawRay(Rigidbody.position, rayDir * groundCheckDistance, Color.blue);

            if (Physics2D.Raycast(Rigidbody.position, rayDir, groundCheckDistance, groundLayer))
                return true;
        }

        return false;
    }
    #endregion
}
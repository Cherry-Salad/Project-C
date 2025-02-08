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
        set 
        {
            if (_state != value)
            {
                _state = value;
                UpdateAnimation();
            }
        }
    }

    public Vector2 MoveDir { get; protected set; } = Vector2.zero;

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

    /// <summary>
    /// 피격 판정 히트 박스
    /// </summary>
    public CapsuleCollider2D BodyHitBox { get; protected set; }

    #region Stat
    public float Hp { get; set; }
    public float MaxHp { get; set; }
    public float Mp { get; set; }
    public float MaxMp { get; set; }
    public float Atk { get; set; }
    public float MoveSpeed { get; set; }
    public float JumpForce { get; set; }
    public float DoubleJumpForce { get; set; }  // 이단 점프할 때 추가적인 점프 힘
    #endregion

    /// <summary>
    /// 이단 점프 재사용을 방지하기 위해 공중에서 이미 이단 점프를 했는지 확인한다. 
    /// </summary>
    protected bool _hasDoubleJumped = false;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        State = ECreatureState.Idle;
        BodyHitBox = Util.FindChild<CapsuleCollider2D>(gameObject, "BodyHitBox", true);
        StartCoroutine(CoUpdate());
        return true;
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
                Animator.Play("Hurt");
                break;
            case ECreatureState.Dead:
                Animator.Play("Dead");
                break;
        }
    }

    protected IEnumerator CoUpdate()
    {
        while (true)
        {
            // 200FPS 이상처럼 프레임이 높다면, 물리 연산을 시작하기도 전에 벽과 바닥, 장애물을 감지하는 버그가 나타난다.
            // 프레임이 높을수록 Update가 더 빠르게 호출된 것이 원인이었다.
            // 예를 들어 OnJump를 통해 속력과 중력을 바꾸었다. 그런데, 프레임이 높으면 캐릭터를 위로 올리기도 전에 바닥을 감지하여 Idle로 전환된다.
            // FixedUpdate도 사용해봤다. 그런데 간혹 타이밍이 어긋나거나 애니메이션 전환을 실패하여 때려치웠다.
            // 이를 방지하기 위해 이벤트 함수의 실행 순서를 참고하였다. 물리 엔진 업데이트 주기에 맞춰 UpdateController가 호출되기 때문에 버그를 방지한다.
            yield return new WaitForFixedUpdate();
            UpdateController();
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
            case ECreatureState.Skill:
                UpdateSkill();
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
            Rigidbody.velocity = Vector2.zero;
        }
        else
            // 공중에 있다면 점프로 전환
            State = ECreatureState.Jump;
    }

    protected virtual void UpdateRun()
    {
        if (CheckGround())
            // Rigidbody.velocity.y를 0으로 하지 않는다면, 캐릭터가 경사진 바닥에서 뛸 때 위로 튀어오른다
            Rigidbody.velocity = MoveDir * MoveSpeed;
        else
            // 공중에 있다면 점프로 전환
            State = ECreatureState.Jump;
    }

    protected virtual void UpdateJump()
    {
        // 캐릭터가 바닥에 닿은지 확인
        bool onGround = CheckGround();

        if (onGround == false && CheckWall())
        {
            // 캐릭터가 공중에 있으면서 벽을 감지
            State = ECreatureState.WallCling;
            _hasDoubleJumped = false;
            return;
        }
        else if (onGround)
        {
            // 캐릭터가 바닥에 닿을 때
            State = ECreatureState.Idle;
            _hasDoubleJumped = false;
            return;
        }

        // 공중(점프, 낙하)이라면 이동 방향에 장애물이 있을 때 제자리에서 걷는 버그 방지
        float distance = Collider.bounds.extents.x + 0.1f;
        bool noObstacles = CheckObstacle(MoveDir, distance, true).collider == null; // 장애물이 없는 지 확인
        float velocityX = (noObstacles) ? MoveDir.x * MoveSpeed : 0f;   // 장애물이 있다면 수평 속도를 0으로 설정

        // 점프, 낙하
        Rigidbody.velocity = new Vector2(velocityX, Rigidbody.velocity.y);
    }

    protected virtual void UpdateSkill()
    {
        if (CheckGround() == false)
        {
            // 공중(점프, 낙하)이라면 이동 방향에 장애물이 있을 때 제자리에서 걷는 버그 방지
            float distance = Collider.bounds.extents.x + 0.1f;
            bool noObstacles = CheckObstacle(MoveDir, distance, true).collider == null; // 장애물이 없는 지 확인
            float velocityX = (noObstacles) ? MoveDir.x * MoveSpeed : 0f;   // 장애물이 있다면 수평 속도를 0으로 설정

            // 점프, 낙하
            Rigidbody.velocity = new Vector2(velocityX, Rigidbody.velocity.y);
        }
        else
        {
            // 스킬 사용 중일 때 바닥에 있다면 움직이지 않는다
            Rigidbody.velocity = Vector2.zero;
        }
    }

    protected virtual void UpdateWallCling()
    {
        // 캐릭터가 정지 상태라면 LookLeft 기준으로 이동 방향 설정
        Vector2 moveDir = MoveDir;
        if (MoveDir == Vector2.zero)
            MoveDir = LookLeft ? Vector2.left : Vector2.right;

        if (CheckGround())
        {
            State = ECreatureState.Idle;
            return;
        }
        else if (CheckWall() == false)
        {
            // 공중이며 벽을 감지하지 못했다면
            if (moveDir == Vector2.zero) 
                State = ECreatureState.Jump;    // 정지 상태였다면 바로 낙하
            else
                OnWallJump();   // 벽 점프
            
            return;
        }

        // 천천히 아래로 내려간다
        float speed = MoveSpeed / 2f;
        Rigidbody.velocity = Vector2.down * speed;
    }

    protected virtual void UpdateWallClimbing()
    {
        // 벽 타기 중일 때, 벽을 감지하지 못한다면 벽 점프로 전환
        if (CheckWall() == false)
        {
            OnWallJump();
            return;
        }

        float wallClimbingSpeed = MoveSpeed / 3f;
        Rigidbody.velocity = Vector2.up * wallClimbingSpeed;
    }

    protected virtual void OnJump()
    {
        bool isWall = CheckWall();

        // 이단 점프 중에는 점프 불가능, 벽을 감지하면 벽 점프로 전환
        if (State != ECreatureState.DoubleJump && CheckGround() && isWall == false)
        {
            State = ECreatureState.Jump;

            // 기본(1단) 점프
            // 경사진 바닥에서도 점프를 할 수 있도록 velocity.x를 0으로 설정
            Rigidbody.velocity = new Vector2(0f, JumpForce);
        }
        // 기본(1단) 점프이거나 벽 점프 상태에서 다시 점프하면 이단 점프로 전환
        else if (_hasDoubleJumped == false && State == ECreatureState.Jump)
        {
            State = ECreatureState.DoubleJump;

            // 이단 점프
            _hasDoubleJumped = true;
            Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, JumpForce + DoubleJumpForce);
        }
        // 벽에 매달린 상태이거나 벽 타기 또는 벽을 감지했다면 벽 점프로 전환
        else if (State == ECreatureState.WallCling || State == ECreatureState.WallClimbing || isWall)
        {
            OnWallJump();
        }
    }

    protected virtual void OnWallJump()
    {
        State = ECreatureState.WallJump;

        // 현재 캐릭터는 벽을 마주하고 있으므로, 반대 방향을 바라봐야 한다
        MoveDir = TurnObject();

        // 벽 점프
        StartCoroutine(CoWallJump(MoveDir));
    }

    protected virtual bool OnDash()
    {
        // 벽에 매달린 상태에서 방향키를 누르지 않고 대시하면 반대 방향으로 대시한다
        if (State == ECreatureState.WallCling)
            MoveDir = TurnObject();

        // 캐릭터가 정지 상태라면 LookLeft 기준으로 이동 방향 설정
        if (MoveDir == Vector2.zero)
            MoveDir = LookLeft ? Vector2.left : Vector2.right;

        float distance = 3f;    // 대시 거리
        float dashSpeed = MoveSpeed * 3f;

        // 목적지 설정
        Vector2 destPos = FindDashDestPos(MoveDir, distance);

        // 현재 위치와 목적지 간의 이동 방향과 남은 거리
        Vector2 dir = destPos - Rigidbody.position;

        // 대시가 가능한지 확인
        if (dir.magnitude > 0.1f)
        {
            State = ECreatureState.Dash;

            // 대시하는 동안 물리적인 제약에서 벗어난다
            Rigidbody.gravityScale = 0f;
            Rigidbody.velocity = Vector2.zero;
            Collider.isTrigger = true;

            // 캐릭터가 정지 상태라면 LookLeft 기준으로 이동 방향 설정
            if (MoveDir == Vector2.zero)
                MoveDir = LookLeft ? Vector2.left : Vector2.right;

            // 대시
            StartCoroutine(CoDash(dir, destPos, dashSpeed));
            return true;
        }

        return false;
    }

    public override void OnDamaged(float damage, Creature attacker = null) 
    {
        // 이미 피격 당하여 무적 상태라면 대미지를 입지 않는다
        if (State == ECreatureState.Dead || State == ECreatureState.Hurt)
            return;

        base.OnDamaged(damage, attacker);

        // HP가 0 이하라면 사망 처리
        State = (Hp <= 0) ? ECreatureState.Dead : ECreatureState.Hurt;
    }

    public virtual void OnDied()
    {
        // 소멸
        Managers.Resource.Destroy(gameObject);
    }

    protected IEnumerator CoWallJump(Vector2 moveDir)
    {
        float elapsedTime = 0f;
        while (elapsedTime < 0.1f)
        {
            // 방향 고정
            MoveDir = moveDir;

            // 공중(점프, 낙하)이라면 이동 방향에 장애물이 있을 때 제자리에서 걷는 버그 방지
            float distance = Collider.bounds.extents.x + 0.1f;
            bool noObstacles = CheckObstacle(moveDir, distance, true).collider == null; // 장애물이 없는 지 확인
            float velocityX = (noObstacles) ? moveDir.x * MoveSpeed : 0f;   // 장애물이 있다면 수평 속도를 0으로 설정

            // 벽 점프
            Rigidbody.velocity = new Vector2(velocityX, JumpForce);
            elapsedTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 벽 점프가 끝나면 전환
        // 캐릭터가 공중에 있으면 점프로 전환
        State = CheckGround() ? ECreatureState.Idle : ECreatureState.Jump;
    }

    IEnumerator CoDash(Vector2 dir, Vector2 destPos, float dashSpeed)
    {
        // 바닥 감지
        bool OnGround = CheckGround();

        // 대시
        while (true)
        {
            Rigidbody.position = Vector2.MoveTowards(Rigidbody.position, destPos, dashSpeed * Time.deltaTime);
            OnGround = CheckGround();

            // 캐릭터가 공중에 있으면서 벽을 감지
            if (OnGround == false && CheckWall())
            {
                // 기본 중력 적용
                Rigidbody.gravityScale = DefaultGravityScale;

                // 대시가 끝나면 충돌 처리 활성화
                if (Collider.isTrigger)
                    Collider.isTrigger = false;

                State = ECreatureState.WallCling;
                yield break;
            }

            dir = destPos - Rigidbody.position;
            if (dir.magnitude <= 0.1f)
                break;

            yield return null;
        }

        Rigidbody.position = destPos;

        // 기본 중력 적용
        Rigidbody.gravityScale = DefaultGravityScale;

        // 대시가 끝나면 충돌 처리 활성화
        if (Collider.isTrigger)
            Collider.isTrigger = false;
        
        // 캐릭터가 공중에 있으면 점프로 전환
        State = OnGround ? ECreatureState.Idle : ECreatureState.Jump;
    }

    /// <summary>
    /// Dash, WallCling 등의 상태에 따라 다양한 먼지 효과를 연출한다
    /// </summary>
    protected void OnSpawnDust()
    {
        Dust dust = Managers.Resource.Instantiate("Dust").GetComponent<Dust>();
        if (dust != null)
            dust.PlayEffect(this);
    }

    #region 벽, 바닥, 장애물 감지
    protected virtual Vector2 TurnObject()
    {
        LookLeft = !LookLeft;
        return LookLeft ? Vector2.left : Vector2.right;
    }

    /// <summary>
    /// 벽이나 벽처럼 통행에 방해되는 장애물을 감지한다
    /// </summary>
    /// <param name="dir">방향</param>
    /// <param name="distance">감지 거리</param>
    /// <param name="isDetailedCheck">더 섬세하게 감지할 것인지</param>
    /// <returns></returns>
    public RaycastHit2D CheckObstacle(Vector2 dir, float distance, bool isDetailedCheck = false)
    {
        // 캐릭터가 정지 상태인지 확인
        if (dir == Vector2.zero)
            return default;

        // 충돌 필터링
        LayerMask includeLayers = 0;
        includeLayers.AddLayer(ELayer.Wall);
        includeLayers.AddLayer(ELayer.Ground);

        RaycastHit2D obstacle = Physics2D.Raycast(Rigidbody.position, dir, distance, includeLayers);
        Debug.DrawRay(Rigidbody.position, dir * distance, Color.green);

        if (obstacle.collider != null || isDetailedCheck == false)
            return obstacle;

        #region 섬세하게 감지
        int rayCount = 5;   // Raycast 발사 횟수
        Vector2 dirUp = new Vector2(dir.x, 1f);
        Vector2 dirDown = new Vector2(dir.x, -1f);

        // Vector2.down부터 leftDown
        for (int i = 1; i <= rayCount; i++)
        {
            float interpolationRatio = i / (float)rayCount; // 보간 비율 (0 ~ 1)
            Vector2 rayDir = Vector2.Lerp(dir, dirUp, interpolationRatio).normalized;
            Debug.DrawRay(Rigidbody.position, rayDir * distance, Color.green);

            obstacle = Physics2D.Raycast(Rigidbody.position, rayDir, distance, includeLayers);
            if (obstacle.collider != null)
                return obstacle;
        }

        // Vector2.down부터 rightDown
        for (int i = 1; i <= rayCount; i++)
        {
            float interpolationRatio = i / (float)rayCount; // 보간 비율 (0 ~ 1)
            Vector2 rayDir = Vector2.Lerp(dir, dirDown, interpolationRatio).normalized;
            Debug.DrawRay(Rigidbody.position, rayDir * distance, Color.magenta);

            obstacle = Physics2D.Raycast(Rigidbody.position, rayDir, distance, includeLayers);
            if (obstacle.collider != null)
                return obstacle;
        }
        #endregion

        return default;
    }

    /// <summary>
    /// 캐릭터 앞에 벽이 있는가
    /// </summary>
    /// <returns></returns>
    public bool CheckWall()
    {
        // 벽 감지
        float wallCheckDistance = Collider.bounds.extents.x + 0.1f; // 벽 감지 거리, Collider 크기 절반에 여유값 추가

        // 충돌 필터링
        LayerMask includeLayers = 0;
        includeLayers.AddLayer(ELayer.Wall);

        Debug.DrawRay(Rigidbody.position, MoveDir * wallCheckDistance, Color.red);
        return Physics2D.Raycast(Rigidbody.position, MoveDir, wallCheckDistance, includeLayers);
    }

    /// <summary>
    /// 캐릭터가 바닥에 있는가
    /// </summary>
    /// <returns></returns>
    public bool CheckGround()
    {
        float groundCheckDistance = Collider.bounds.extents.y + 0.05f;   // 바닥 감지 거리
        
        // 충돌 필터링
        LayerMask includeLayers = 0;
        includeLayers.AddLayer(ELayer.Ground);
        
        Debug.DrawRay(Rigidbody.position, Vector2.down * groundCheckDistance, Color.red);
        var p = Physics2D.Raycast(Rigidbody.position, Vector2.down, groundCheckDistance, includeLayers);

        // 캐릭터 밑의 평평한 바닥 감지
        if (Physics2D.Raycast(Rigidbody.position, Vector2.down, groundCheckDistance, includeLayers))
            return true;

        #region 섬세하게 감지
        int rayCount = 5;   // Raycast 발사 횟수
        Vector2 leftDown = new Vector2(-0.5f, -1f); // 왼쪽 대각선
        Vector2 rightDown = new Vector2(0.5f, -1f); // 오른쪽 대각선

        float minSlopeAngle = 0f;
        float maxSlopeAngle = 60f;

        // Vector2.down부터 leftDown
        for (int i = 1; i <= rayCount; i++)
        {
            float interpolationRatio = i / (float)rayCount; // 보간 비율 (0 ~ 1)
            Vector2 rayDir = Vector2.Lerp(Vector2.down, leftDown, interpolationRatio).normalized;
            Debug.DrawRay(Rigidbody.position, rayDir * groundCheckDistance, Color.red);

            //if (Physics2D.Raycast(Rigidbody.position, rayDir, groundCheckDistance, includeLayers))
            //{
            //    return true;
            //}

            var groundInfo = Physics2D.Raycast(Rigidbody.position, rayDir, groundCheckDistance, includeLayers);
            if (groundInfo.collider != null)
            {
                float slopeAngle = Vector2.Angle(groundInfo.normal, Vector2.up);
                if (slopeAngle > minSlopeAngle && slopeAngle < maxSlopeAngle)
                    return true;

                //Debug.Log(slopeAngle);
                return false;
            }
        }

        // Vector2.down부터 rightDown
        for (int i = 1; i <= rayCount; i++)
        {
            float interpolationRatio = i / (float)rayCount; // 보간 비율 (0 ~ 1)
            Vector2 rayDir = Vector2.Lerp(Vector2.down, rightDown, interpolationRatio).normalized;
            Debug.DrawRay(Rigidbody.position, rayDir * groundCheckDistance, Color.blue);

            //if (Physics2D.Raycast(Rigidbody.position, rayDir, groundCheckDistance, includeLayers))
            //{
            //    return true;
            //}

            var groundInfo = Physics2D.Raycast(Rigidbody.position, rayDir, groundCheckDistance, includeLayers);
            if (groundInfo.collider != null)
            {
                float slopeAngle = Vector2.Angle(groundInfo.normal, Vector2.up);
                if (slopeAngle > minSlopeAngle && slopeAngle < maxSlopeAngle)
                    return true;

                //Debug.Log(slopeAngle);
                return false;
            }

        }
        #endregion

        return false;
    }

    /// <summary>
    /// 벽이나 다른 물체에 의해 막힐 수 있으므로, 대시할 수 있는 최대 목적지를 구한다
    /// </summary>
    /// <param name="dir">방향</param>
    /// <param name="distance">거리</param>
    /// <returns></returns>
    Vector2 FindDashDestPos(Vector2 dir, float distance)
    {
        List<RaycastHit2D> obstacles = new List<RaycastHit2D>();
        ContactFilter2D filter = new ContactFilter2D();

        // 충돌 필터링
        LayerMask includeLayers = 0;
        includeLayers.AddLayer(ELayer.Wall);
        includeLayers.AddLayer(ELayer.Ground);
        
        filter.SetLayerMask(includeLayers);
        filter.useTriggers = false;

        Collider.Cast(dir, filter, obstacles, distance);

        if (obstacles.Count > 0)
        {
            RaycastHit2D closestHit = obstacles[0];
            float closestDistance = closestHit.distance;

            // 가장 가까운 충돌 거리 계산
            foreach (var obstacle in obstacles)
            {
                if (obstacle.distance < closestDistance)
                {
                    closestHit = obstacle;
                    closestDistance = obstacle.distance;
                }
            }

            // 충돌 지점에서 약간 떨어진 위치
            Vector2 destPos = Rigidbody.position + dir * closestDistance;
            Debug.DrawLine(Rigidbody.position, (destPos), Color.green, 0.5f);
            return destPos;
        }

        // 충돌이 없다면 원래 목적지
        Debug.DrawLine(Rigidbody.position, Rigidbody.position + dir * distance, Color.red);
        return Rigidbody.position + dir * distance;
    }
    #endregion
}
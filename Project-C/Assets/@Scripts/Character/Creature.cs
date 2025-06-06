using System;
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

    public string Name { get; set; }
    
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
    /// 공중에서 무한으로 이단 점프하는 것을 방지하기 위해서 공중에서 이미 이단 점프를 했는지 확인한다. 
    /// </summary>
    protected bool _hasDoubleJumped = false;
    /// <summary>
    /// 공중에서 무한으로 대시하는 것을 방지하기 위해서 공중에서 이미 대시 했는지 확인한다
    /// </summary>
    protected bool _hasDashed = false;
    protected bool _isInvincibility = false;  // 무적 상태

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        
        DefaultGravityScale = 2f;   // 중력을 1로 설정하니까 낙하할 때 시원찮더라~

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
            case ECreatureState.Dash:
                Animator.Play("Dash");
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
            Rigidbody.velocity = Vector2.zero;  // 움직이지 않는다
        else
            State = ECreatureState.Jump;    // 공중에 있다면 낙하 처리
    }

    protected virtual void UpdateRun()
    {
        if (CheckGround())
            Rigidbody.velocity = MoveDir * MoveSpeed;   // 수직 속도(velocity.y)를 0으로 하지 않는다면, 캐릭터가 경사진 바닥에서 뛸 때 위로 튀어오른다
        else
            State = ECreatureState.Jump;    // 공중에 있다면 낙하 처리
    }

    protected virtual void UpdateJump()
    {
        // 캐릭터가 바닥에 닿은지 확인
        bool onGround = CheckGround();

        if (onGround == false && CheckWall())   // 캐릭터가 공중에 있으면서 벽을 감지
        {
            State = ECreatureState.WallCling;
            _hasDoubleJumped = false;
            _hasDashed = false;
            return;
        }
        else if (onGround)  // 캐릭터가 바닥에 닿을 때
        {
            State = ECreatureState.Idle;
            _hasDoubleJumped = false;
            _hasDashed = false;
            return;
        }

        // 공중이라면 이동 방향에 장애물이 있을 때 제자리에서 걷는 버그 방지
        float distance = Collider.bounds.extents.x + 0.1f;
        bool noObstacles = FindObstacle(MoveDir, distance, true).collider == null; // 장애물이 없는 지 확인
        float velocityX = (noObstacles) ? MoveDir.x * MoveSpeed : 0f;   // 장애물이 있다면 수평 속도(velocity.x)를 0으로 설정

        // 공중에서 낙하 처리
        Rigidbody.velocity = new Vector2(velocityX, Rigidbody.velocity.y);
    }

    protected virtual void UpdateSkill() {}

    protected virtual void UpdateWallCling() {}

    protected virtual void UpdateWallClimbing() {}

    protected virtual void OnJump()
    {
        // 기본(1단) 점프이다. 이단 점프 중에는 불가능
        if (State != ECreatureState.DoubleJump && CheckGround())
        {
            State = ECreatureState.Jump;

            // 경사진 바닥에서도 점프를 할 수 있도록 수평 속도(velocity.x)를 0으로 설정
            //Rigidbody.velocity = new Vector2(0f, JumpForce);
            Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, 0f);
            Rigidbody.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
        }
    }

    protected virtual void OnDoubleJump()
    {
        State = ECreatureState.DoubleJump;

        // 이단 점프
        _hasDoubleJumped = true;
        // Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, JumpForce + DoubleJumpForce);
        Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, 0f);
        Rigidbody.AddForce(new Vector2(0f, JumpForce + DoubleJumpForce), ForceMode2D.Impulse);
    }

    protected virtual void OnWallJump(float duration)
    {
        State = ECreatureState.WallJump;

        // 현재 캐릭터는 벽을 마주하고 있으므로, 반대 방향을 바라봐야 한다
        MoveDir = TurnObject();

        // 벽 점프
        StartCoroutine(CoWallJump(MoveDir, duration));
    }

    /// <summary>
    /// 캐릭터가 바라보는 방향으로 대시한다.
    /// </summary>
    /// <param name="distance">거리</param>
    /// <param name="speedMultiplier">속도 배율</param>
    /// <param name="ignorePhysics">물리적인 제약을 무시(중력과 충돌 제외)한다.</param>
    /// <param name="ignoreObstacle">장애물을 무시한다.</param>
    /// <param name="callback">대시에 성공했을 때 이벤트</param>
    protected virtual bool OnDash(float distance, float speedMultiplier, bool ignorePhysics = true, bool ignoreObstacle = false)
    {
        // 벽에 매달린 상태에서 방향키를 누르지 않고 대시하면 반대 방향으로 대시한다
        if (State == ECreatureState.WallCling)
            MoveDir = TurnObject();

        // 캐릭터가 정지 상태라면 LookLeft 기준으로 이동 방향 설정
        if (MoveDir == Vector2.zero)
            MoveDir = LookLeft ? Vector2.left : Vector2.right;

        // 목적지 설정
        Vector2 destPos = FindDashDestPos(MoveDir, distance, ignorePhysics, ignoreObstacle);

        // 현재 위치와 목적지 간의 이동 방향과 남은 거리
        Vector2 dir = destPos - Rigidbody.position;

        // 대시가 가능한지 확인
        if (dir.magnitude > 0.1f)
        {
            State = ECreatureState.Dash;

            // 대시하는 동안 물리적인 제약에서 벗어난다
            if (ignorePhysics)
            {
                Rigidbody.gravityScale = 0f;
                Rigidbody.velocity = Vector2.zero;
                Collider.isTrigger = true;
            }
            
            // 대시 시작
            StartCoroutine(CoDash(dir, destPos, MoveSpeed * speedMultiplier, ignorePhysics));
            return true;
        }

        return false;
    }

    public override void OnDamaged(float damage, bool ignoreInvincibility = false, Collider2D attacker = null) 
    {
        if (State == ECreatureState.Dead)
            return;

        // 무적 상태라면 대미지를 입지 않는다
        if (ignoreInvincibility == false && _isInvincibility)
            return;

        base.OnDamaged(damage, attacker);

        // HP가 0 이하라면 사망 처리
        Hp -= damage;
        State = (Hp <= 0) ? ECreatureState.Dead : ECreatureState.Hurt;
    }

    public override void OnDied()
    {
        base.OnDied();
        
        // 소멸
        Managers.Resource.Destroy(gameObject);
    }

    protected IEnumerator CoWallJump(Vector2 moveDir, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            // 방향 고정
            MoveDir = moveDir;

            // 공중(점프, 낙하)이라면 이동 방향에 장애물이 있을 때 제자리에서 걷는 버그 방지
            float distance = Collider.bounds.extents.x + 0.1f;
            bool noObstacles = FindObstacle(moveDir, distance, true).collider == null; // 장애물이 없는 지 확인
            float velocityX = (noObstacles) ? moveDir.x * MoveSpeed : 0f;   // 장애물이 있다면 수평 속도(velocityX)를 0으로 설정

            // 벽 점프
            //Rigidbody.velocity = new Vector2(velocityX, JumpForce);
            Rigidbody.velocity = new Vector2(velocityX, 0f);
            Rigidbody.AddForce(new Vector2(0f, JumpForce), ForceMode2D.Impulse);

            elapsedTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();  // Collider 갱신
        }

        // 벽 점프가 끝나면 전환
        // 캐릭터가 공중에 있으면 점프로 전환
        State = CheckGround() ? ECreatureState.Idle : ECreatureState.Jump;
    }

    protected IEnumerator CoDash(Vector2 moveDir, Vector2 destPos, float dashSpeed, bool ignorePhysics = true)
    {
        // 바닥 감지
        bool OnGround = CheckGround();

        // 대시
        while (true)
        {
            // UpdateDash를 만들어도 되지만, 목적지를 미리 설정하였기 때문에 코루틴을 사용한다
            Rigidbody.position = Vector2.MoveTowards(Rigidbody.position, destPos, dashSpeed * Time.deltaTime);
            yield return new WaitForFixedUpdate();  // Collider 갱신

            // 벽 타기로 넘어가야 하는 상황인지 확인한다
            OnGround = CheckGround();
            bool shouldWallCling = (ObjectType == EObjectType.Player && State == ECreatureState.Dash) && (OnGround == false && CheckWall());

            // 대시가 아닌 다른 상태로 전환
            if (State != ECreatureState.Dash || shouldWallCling)
            {
                if (ignorePhysics)
                {
                    // 기본 중력 적용
                    Rigidbody.gravityScale = DefaultGravityScale;

                    // 대시가 끝나면 충돌 처리 활성화
                    if (Collider.isTrigger)
                        Collider.isTrigger = false;
                }

                // 벽 타기로 전환
                if (shouldWallCling)
                    State = ECreatureState.WallCling;

                yield break;
            }

            // 목적지에 근접했는지 확인
            moveDir = destPos - Rigidbody.position;
            if (moveDir.magnitude <= 0.1f)
                break;
        }

        // 위치 보정
        Rigidbody.position = destPos;

        if (ignorePhysics)
        {
            // 기본 중력 적용
            Rigidbody.gravityScale = DefaultGravityScale;

            // 대시가 끝나면 충돌 처리 활성화
            if (Collider.isTrigger)
                Collider.isTrigger = false;
        }

        _hasDashed = !OnGround;

        // 캐릭터가 공중에 있으면 점프로 전환
        State = OnGround ? ECreatureState.Idle : ECreatureState.Jump;
    }

    /// <summary>
    /// Dash, WallCling 등 특정 상태에 따라 다양한 먼지 효과를 연출한다.
    /// </summary>
    protected void OnSpawnDust()
    {
        Dust dust = Managers.Resource.Instantiate("Dust").GetComponent<Dust>();
        if (dust != null)
            dust.PlayEffect(this);
    }

    #region Util
    /// <summary>
    /// 방향을 전환한다.
    /// </summary>
    /// <returns></returns>
    protected virtual Vector2 TurnObject()  // 오버라이드가 필요 없다면, virtual을 지워주세요.
    {
        LookLeft = !LookLeft;
        return LookLeft ? Vector2.left : Vector2.right;
    }

    #region 벽, 바닥, 장애물 등을 감지한다. 반드시, Collider를 갱신(FixedUpdate)한 뒤에 사용한다.
    /// <summary>
    /// 통행에 방해되는 장애물을 감지한다.
    /// </summary>
    /// <param name="dir">방향</param>
    /// <param name="distance">감지 거리</param>
    /// <param name="isDetailedCheck">더 섬세하게 감지할 것인지</param>
    /// <returns></returns>
    public RaycastHit2D FindObstacle(Vector2 dir, float distance, bool isDetailedCheck = false)
    {
        // 캐릭터가 정지 상태인지 확인
        if (dir == Vector2.zero)
            return default;

        // 충돌 필터링
        LayerMask includeLayers = 0;
        includeLayers.AddLayer(ELayer.Wall);
        includeLayers.AddLayer(ELayer.Ground);
        includeLayers.AddLayer(ELayer.Env);

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
    /// <param name="includeLayers">벽으로 간주할 레이어</param>
    /// <returns></returns>
    public virtual bool CheckWall(LayerMask includeLayers = default)
    {
        // 벽 감지
        float wallCheckDistance = Collider.bounds.extents.x + 0.1f; // 벽 감지 거리, Collider 크기 절반에 여유값 추가

        // 충돌 필터링
        if (includeLayers == default)
        {
            includeLayers = 0;
            includeLayers.AddLayer(ELayer.Wall);
            includeLayers.AddLayer(ELayer.Ground);
            includeLayers.AddLayer(ELayer.Env);
        }

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
        includeLayers.AddLayer(ELayer.Env);

        Debug.DrawRay(Rigidbody.position, Vector2.down * groundCheckDistance, Color.red);

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
    #endregion

    /// <summary>
    /// 벽이나 다른 물체에 의해 막힐 수 있으므로, 대시할 수 있는 최대 목적지를 구한다
    /// </summary>
    /// <param name="moveDir">방향</param>
    /// <param name="distance">거리</param>
    /// <param name="ignorePhysics">물리적 제약 무시 여부</param>
    /// <param name="ignoreObstacle">장애물 무시 여부</param>
    /// <returns></returns>
    protected Vector2 FindDashDestPos(Vector2 moveDir, float distance, bool ignorePhysics, bool ignoreObstacle = false)
    {
        // 장애물 무시
        if (ignoreObstacle)
        {
            Debug.DrawLine(Rigidbody.position, Rigidbody.position + moveDir * distance, Color.red);
            return Rigidbody.position + moveDir * distance;
        }

        List<RaycastHit2D> obstacles = new List<RaycastHit2D>();
        ContactFilter2D filter = new ContactFilter2D();

        // 충돌 필터링
        LayerMask includeLayers = 0;
        includeLayers.AddLayer(ELayer.Wall);
        includeLayers.AddLayer(ELayer.Ground);
        includeLayers.AddLayer(ELayer.Env);
        
        filter.SetLayerMask(includeLayers);
        filter.useTriggers = false;

        Collider.Cast(moveDir, filter, obstacles, distance);

        // 장애물을 찾았다
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
            Vector2 destPos = Rigidbody.position + moveDir * closestDistance;
            Debug.DrawLine(Rigidbody.position, (destPos), Color.green, 0.5f);
            return destPos;
        }

        // 충돌이 없다면 원래 목적지
        Debug.DrawLine(Rigidbody.position, Rigidbody.position + moveDir * distance, Color.red);
        return Rigidbody.position + moveDir * distance;
    }
    #endregion
}
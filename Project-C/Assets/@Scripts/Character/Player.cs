using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Player : Creature
{
    public PlayerData Data;

    #region Stat
    public int HpLevel { get; set; }
    public int MpLevel { get; set; }
    public int AtkLevel { get; set; }
    public int AccessorySlot { get; set; }
    #endregion

    public Dictionary<KeyCode, PlayerSkillBase> Skills = new Dictionary<KeyCode, PlayerSkillBase>();

    // 이동
    bool _moveDirKeyPressed = false;

    // 점프
    bool _isWallJump = false;
    bool _jumpKeyPressed = false;
    float _jumpKeyPressedTime = 0f; // 점프 키를 누르고 있는 시간
    float _jumpDuration = 0.3f; // 점프 유지 시간, 원래는 0.5초로 했는데 이게 체감상 손가락에 좀 무리가 가더라구..
    float _jumpHoldForce = 0.2f;    // 점프 키를 유지했을 때 적용되는 힘

    // 대시
    float _dashCoolTime = 1.0f; // 대시 쿨타임
    bool _completeDashCooldown = true;  // 대쉬 쿨다운 완료 여부

    // 스킬
    KeyCode _pressedSkillKey = KeyCode.None;
    float _skillKeyPressedTime = 0f;    // 스킬 키를 누르고 있는 시간

    bool _isTouchingTrap = false;   // 함정 충돌 여부

    /// <summary>
    /// 세이브 포인트와 접촉 중인지 확인
    /// </summary>
    SavePoint _savePoint = null;

    Coroutine _CoDamaged = null;    // 피격 판정 중복 방지

    //UI를 위한 이벤트 추가
    public event Action OnHpChanged;
    public event Action OnMpChanged;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Player;

        // 트리거 필터링
        if (BodyHitBox != null)
        {
            BodyHitBox.isTrigger = true;
            // 플레이어 BodyHitBox에 태그를 Player로 하지 않으면, 몬스터가 플레이어 제대로 못 찾는다

            LayerMask excludeLayers = 0;
            excludeLayers.AddLayer(ELayer.Default);
            excludeLayers.AddLayer(ELayer.Wall);
            excludeLayers.AddLayer(ELayer.Ground);
            excludeLayers.AddLayer(ELayer.Player);
            BodyHitBox.excludeLayers = excludeLayers;
        }

        JumpForce = 6f;
        DoubleJumpForce = 1f;

        #region 데이터 로드
        // 플레이어 스탯
        //Data = Managers.Data.PlayerDataDic[PLAYER_ID];
        //Hp = Data.Hp;
        //MaxHp = Data.MaxHp;
        //HpLevel = Data.HpLevel;
        //Mp = Data.Mp;
        //MaxMp = Data.MaxMp;
        //MpLevel = Data.MpLevel;
        //Atk = Data.Atk;
        //AtkLevel = Data.AtkLevel;
        //MoveSpeed = Data.Speed;
        //AccessorySlot = Data.AccessorySlot;

        var data = Managers.Game.GameData.Player;

        // 플레이어 스탯
        Hp = data.Hp;
        MaxHp = data.MaxHp;
        HpLevel = data.HpLevel;
        Mp = data.Mp;
        MaxMp = data.MaxMp;
        MpLevel = data.MpLevel;
        Atk = data.Atk;
        AtkLevel = data.AtkLevel;
        MoveSpeed = data.Speed;
        AccessorySlot = data.AccessorySlot;

        LoadSkills();
        #endregion

        return true;
    }

    //플레이어 스킬 로드
    private void LoadSkills()
    {
        foreach (int skillId in Managers.Game.GameData.Player.SkillIdList)
        {
            if (Managers.Data.PlayerSkillDataDic.TryGetValue(skillId, out var data) == false)
                continue;

            var type = Type.GetType(data.CodeName);
            if (type == null)
                continue;

            // GetOrAddComponent가 안돼서 null 검사
            PlayerSkillBase skill = gameObject.GetComponent(type) as PlayerSkillBase;
            if (skill == null)
                skill = gameObject.AddComponent(type) as PlayerSkillBase;

            skill.SetInfo(this, data);
            Skills.Add(skill.Key, skill);
        }
    }

    public void TriggerOnHpChanged() { OnHpChanged?.Invoke(); } // HP 업데이트 이벤트 트리거
    public void TriggerOnMpChanged() { OnMpChanged?.Invoke(); } // MP 업데이트 이벤트 트리거

    #region 입력 감지
    void Update()
    {
        GetInput();
    }

    /// <summary>
    /// 입력 키를 감지한다.
    /// 사망했거나 대시, 피격, 스킬을 사용할 때 캐릭터는 추가적인 조작 불가능(ex: 대시하는 동안 공격과 점프는 불가능)
    /// </summary>
    void GetInput()
    {
        if (State == ECreatureState.Dead || State == ECreatureState.Dash || State == ECreatureState.Hurt || State == ECreatureState.Skill)
            return;

        // TODO: 입력 키 설정이 구현되면 불러오는 것으로 바꾼다

        // 테스트용 코드, 마나 회복
        if (Input.GetKeyDown(KeyCode.Q))
            Mp = Mathf.Clamp(Mp + 1f, 1f, MaxMp);

        if (IsInteractionInput())
            return;

        if ((State != ECreatureState.WallJump && IsDashInput()) || IsSkillInput())
            return;

        IsJumpInput();

        _moveDirKeyPressed = IsMoveDirInput();
        if (_moveDirKeyPressed)
            LookLeft = MoveDir.x < 0;
    }

    bool IsDashInput()
    {
        // 대시키 입력
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            OnDash();
            return true;
        }

        return false;
    }

    bool IsJumpInput()
    {
        // TODO: 나중에 기본 키를 바꾸는게 좋겠다.. 점프 키를 컨트롤로 하니까 왼손 새끼 손가락에 무리가 간다.. 아니면 내 손이 문제인가..?
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            _jumpKeyPressed = false;
            _jumpKeyPressedTime = 0f;
            return false;
        }

        // 키를 꾹 누르고 있을 때
        if (_jumpKeyPressed)
        {
            float pressedTime = Time.time - _jumpKeyPressedTime;
            if (pressedTime >= 0.1f && pressedTime < _jumpDuration)
            {
                OnJumpHold();
                return true;
            }

            return false;
        }

        // 점프키 입력
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            _jumpKeyPressed = true;
            _jumpKeyPressedTime = Time.time;
            OnJump();
            return true;
        }

        return false;
    }

    bool IsMoveDirInput()
    {
        // 방향키 입력
        bool leftPressed = Input.GetKey(KeyCode.LeftArrow);
        bool rightPressed = Input.GetKey(KeyCode.RightArrow);

        int pressedCount = (leftPressed ? 1 : 0) + (rightPressed ? 1 : 0);

        // 방향키 입력을 두 개 이상 눌렸다면 입력 취소
        if (pressedCount > 1)
        {
            // 벽을 감지하면 벽 점프로 전환
            if (State == ECreatureState.WallCling || State == ECreatureState.WallClimbing || CheckWall())
                OnWallJump();

            return false;
        }

        if (leftPressed)
        {
            MoveDir = Vector2.left;
            return true;
        }
        else if (rightPressed)
        {
            MoveDir = Vector2.right;
            return true;
        }

        // 방향키 입력이 없다면 캐릭터는 정지 상태 
        MoveDir = Vector2.zero;
        return false;
    }

    bool IsSkillInput()
    {
        // 꾹 눌러야 하는 키
        if (_pressedSkillKey != KeyCode.None)
        {
            if (Input.GetKeyUp(_pressedSkillKey))
            {
                _pressedSkillKey = KeyCode.None;
                _skillKeyPressedTime = 0f;
            }
            else
            {
                // 키를 누르고 있다
                float pressedTime = Time.time - _skillKeyPressedTime;
                if (pressedTime >= Skills[_pressedSkillKey].KeyPressedTime)
                {
                    Skills[_pressedSkillKey].DoSkill();

                    _pressedSkillKey = KeyCode.None;
                    _skillKeyPressedTime = 0f;
                }

                return true;
            }
        }

        // 스킬키 입력
        foreach (KeyCode keyCode in Skills.Keys)
        {
            if (Input.GetKeyDown(keyCode))
            {
                //Debug.Log($"입력된 키: {keyCode}");

                if (Skills[keyCode].KeyPressedTime > 0)
                {
                    // 꾹 눌러야 하는 키
                    _pressedSkillKey = keyCode;
                    _skillKeyPressedTime = Time.time;
                }
                else
                {
                    Skills[keyCode].DoSkill();
                    _pressedSkillKey = KeyCode.None;
                    _skillKeyPressedTime = 0f;
                }

                return true;
            }

            if (Input.GetKey(keyCode))
            {
                //Debug.Log($"입력된 키: {keyCode}");
                if (Skills[keyCode].KeyPressedTime > 0)
                {
                    // 꾹 눌러야 하는 키
                    _pressedSkillKey = keyCode;
                    _skillKeyPressedTime = Time.time;
                }

                return true;
            }
        }

        return false;
    }

    bool IsInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_savePoint != null)
            {
                Debug.Log("세이브 포인트 활성화");
                Managers.Map.CurrentSavePoint = _savePoint;
                Managers.Game.Save();
            }

            return true;
        }

        return false;
    }

    #endregion

    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();

        switch (State)
        {
            case ECreatureState.Jump:
                Animator.Play("Jump");
                break;
            case ECreatureState.DoubleJump:
                Animator.Play("DoubleJump");
                break;
            case ECreatureState.Dash:
                OnSpawnDust();
                break;
            case ECreatureState.WallCling:
                Animator.Play("WallSlide");
                OnSpawnDust();
                break;
            case ECreatureState.WallClimbing:
                Animator.Play("WallSlide");
                break;
            case ECreatureState.Dead:
                Animator.Play("Dead");
                //OnSpawnDust();  // 애니메이션 이벤트로 호출한다
                break;
        }
    }

    protected override void UpdateController()
    {
        if (Managers.Map.CurrentRoom == null || Managers.Map.CurrentRoom.IsInRoom(transform.position) == false)
        {
            //Debug.Log("ChangeCurrentRoom");
            bool disableCameraBlend = (State == ECreatureState.Dead) ? true : false;
            Managers.Map.ChangeCurrentRoom(transform.position, disableCameraBlend);
        }

        base.UpdateController();
    }

    protected override void UpdateIdle()
    {
        if (_moveDirKeyPressed)
            State = ECreatureState.Run;

        base.UpdateIdle();
    }

    protected override void UpdateRun()
    {
        if (_moveDirKeyPressed == false)
        {
            State = ECreatureState.Idle;
            return;
        }

        base.UpdateRun();
    }

    protected override void UpdateJump()
    {
        base.UpdateJump();

        if (State == ECreatureState.Idle || State == ECreatureState.WallCling)
            _isWallJump = false;
    }

    protected override void UpdateSkill()
    {
        base.UpdateSkill();
        //if (CheckGround() == false)
        //{
        //    // 공중(점프, 낙하)이라면 이동 방향에 장애물이 있을 때 제자리에서 걷는 버그 방지
        //    float distance = Collider.bounds.extents.x + 0.1f;
        //    bool noObstacles = FindObstacle(MoveDir, distance, true).collider == null; // 장애물이 없는 지 확인
        //    float velocityX = (noObstacles) ? MoveDir.x * MoveSpeed : 0f;   // 장애물이 있다면 수평 속도(velocity.x)를 0으로 설정

        //    // 점프, 낙하
        //    Rigidbody.velocity = new Vector2(velocityX, Rigidbody.velocity.y);
        //}
        //else
        //{
        //    // 스킬 사용 중일 때 바닥에 있다면 움직이지 않는다
        //    Rigidbody.velocity = Vector2.zero;
        //}
    }

    protected override void UpdateWallCling()
    {
        if (_moveDirKeyPressed)
            State = ECreatureState.WallClimbing;

        base.UpdateWallCling();

        // 캐릭터가 정지 상태라면 LookLeft 기준으로 이동 방향 설정
        Vector2 moveDir = MoveDir;
        if (MoveDir == Vector2.zero)
            MoveDir = LookLeft ? Vector2.left : Vector2.right;

        if (CheckGround())
        {
            TurnObject();
            State = ECreatureState.Idle;
            return;
        }
        else if (CheckWall() == false)
        {
            TurnObject();

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

    protected override void UpdateWallClimbing()
    {
        // 방향키를 눌러야 벽 타기
        // 만약 누르지 않는다면, 벽에 매달린 상태로 전환
        if (_moveDirKeyPressed == false)
        {
            State = ECreatureState.WallCling;
            return;
        }

        base.UpdateWallClimbing();

        // 벽 타기 중일 때, 벽을 감지하지 못한다면 벽 점프로 전환
        if (CheckWall() == false)
        {
            OnWallJump();
            return;
        }

        // 벽 타기 속도
        float wallClimbingSpeed = MoveSpeed / 3f;
        Rigidbody.velocity = Vector2.up * wallClimbingSpeed;
    }

    protected override void OnJump()
    {
        // 벽을 감지하면 벽 점프로 전환
        if (State == ECreatureState.WallCling || State == ECreatureState.WallClimbing || CheckWall())
        {
            OnWallJump();
            return;
        }

        // 기본(1단) 점프이거나 벽 점프 상태에서 다시 점프하면 이단 점프로 전환
        else if (_hasDoubleJumped == false && State == ECreatureState.Jump)
        {
            OnDoubleJump();
            return;
        }

        base.OnJump();
    }

    protected override void OnWallJump(float duration = 0.1f)
    {
        // 벽 점프
        base.OnWallJump(duration);
        _isWallJump = true;
    }

    /// <summary>
    /// 점프 키를 누르고 있을 때 더 높이 점프한다
    /// </summary>
    void OnJumpHold()
    {
        // 벽 점프나 이단 점프를 했다면 추가 점프 힘을 적용하지 않는다
        if (_isWallJump || _hasDoubleJumped)
            return;

        // 1단 점프일 때 추가 점프 힘 적용
        if (State == ECreatureState.Jump)
        {
            //Rigidbody.AddForce(Vector2.up * _jumpHoldForce, ForceMode2D.Impulse); // 이건 영 조작감이 별로라 velocity를 사용
            Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, JumpForce + _jumpHoldForce);
            State = ECreatureState.Jump;
        }
    }

    protected override bool OnDash(float distance = 3f, float speedMultiplier = 3f, bool ignorePhysics = true, bool ignoreObstacle = false)
    {
        // 대시 쿨타임 완료 여부
        if (_completeDashCooldown == false)
            return false;

        if (base.OnDash(distance, speedMultiplier, ignorePhysics, ignoreObstacle))
        {
            StartCoroutine(CoDashCooldown());
            StartCoroutine(CoHandleDashInvincibility());
            return true;
        }

        return false;
    }

    public override void OnDamaged(float damage = 1f, bool ignoreInvincibility = false, Collider2D attacker = null)
    {
        if (State == ECreatureState.Dead)
            return;

        // 무적 상태라면 대미지를 입지 않는다
        if (ignoreInvincibility == false && _isInvincibility)
            return;

        // HP 감소
        Hp -= damage;
        OnHpChanged?.Invoke(); //체력 변경 이벤트 호출

        State = ECreatureState.Hurt;
        if (_CoDamaged != null)
        {
            StopCoroutine(_CoDamaged);
            _CoDamaged = null;
            SpriteRenderer.enabled = true;
        }
        
        _CoDamaged = StartCoroutine(CoDamaged());   // 무적 상태

        Rigidbody.velocity = Vector2.zero;

        if (attacker == null)
            return;

        // 살짝 위로 튀어오르듯이
        float dirX = Mathf.Sign(Rigidbody.position.x - attacker.transform.position.x);  // x값은 -1 또는 1로 고정
        Vector2 knockbackDir = (Vector2.up * 1.5f) + new Vector2(dirX, 0);

        // 넉백
        float knockbackForce = 4.5f;
        Rigidbody.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
    }

    /// <summary>
    /// 애니메이션 이벤트로 호출한다.
    /// </summary>
    public override void OnDied()
    {
        // 모두 회복
        Hp = MaxHp;
        Mp = MaxMp;

        Managers.Game.Save();
        Managers.Map.RespawnAtSavePoint(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<SavePoint>(out var sp))
        {
            //Debug.Log($"{collision.name} 충돌");
            _savePoint = sp;
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (State == ECreatureState.Dead)
            return;

        // 무적 상태일 때 몬스터와 충돌하면 안된다
        if (_isInvincibility == false && collision.gameObject.CompareTag("EnemyHitBox"))
        {
            //Debug.Log($"{collision.name} 충돌");
            OnDamaged(attacker: collision);
        }

        // 장애물 충돌
        if (_isTouchingTrap == false && collision.gameObject.CompareTag("Trap"))
        {
            //Debug.Log($"{collision.name} 충돌");
            _isTouchingTrap = true;
            OnDamaged(ignoreInvincibility: true);
            Managers.Map.RespawnAtCheckpoint(gameObject);
            return;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (State == ECreatureState.Dead)
            return;

        // 체크포인트와 상호작용
        if (collision.gameObject.CompareTag("Checkpoint"))
        {
            Debug.Log("체크포인트 활성화");
            Vector3 worldPos = collision.bounds.center;
            Managers.Map.CurrentCheckpoint = worldPos;
        }

        if (collision.TryGetComponent<SavePoint>(out var sp))
        {
            //Debug.Log($"{collision.name} 충돌 안 함");
            _savePoint = null;
        }
    }

    IEnumerator CoDashCooldown()
    {
        _completeDashCooldown = false;
        yield return new WaitForSeconds(_dashCoolTime);
        _completeDashCooldown = true;
    }

    IEnumerator CoHandleDashInvincibility()
    {
        _isInvincibility = true;
        
        while (State == ECreatureState.Dash)
            yield return new WaitForFixedUpdate();

        _isInvincibility = false;
    }

    IEnumerator CoDamaged(float duration = 0.5f, float bonusDuration = 0.7f)
    {
        // 지속 시간만큼 무적 상태이다
        _isInvincibility = true;
        yield return new WaitForSeconds(duration);

        if (Hp > 0)
            State = CheckGround() ? ECreatureState.Idle : ECreatureState.Jump;  // 캐릭터가 공중에 있으면 점프로 전환
        else
        {
            StartCoroutine(CoUpdateDead()); // 사망 판정
            _CoDamaged = null;
            yield break;
        }

        // 추가 무적 시간
        float elapsedTime = 0f;
        while (elapsedTime < bonusDuration)
        {
            elapsedTime += Time.deltaTime;
            SpriteRenderer.enabled = !SpriteRenderer.enabled;   // 스프라이트 깜빡 효과
            yield return null;
        }

        SpriteRenderer.enabled = true;
        _isInvincibility = false;
        _isTouchingTrap = false;
        _CoDamaged = null;
    }

    IEnumerator CoUpdateDead()
    {
        float elapsedTime = 0f;

        // 캐릭터가 바닥에 떨어질 때까지 피격 애니메이션을 재생
        // 낙사나 너무 높은 곳에서 떨어질 것을 대비하여, 최대 1.5초를 넘기지 않도록 하였다.
        while (CheckGround() == false)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= 1.5f)
                yield break;

            yield return new WaitForFixedUpdate();
        }

        // 사망
        Rigidbody.velocity = Vector2.zero;
        State = ECreatureState.Dead;
    }

    public override bool CheckWall(LayerMask includeLayers = default)
    {
        includeLayers.AddLayer(ELayer.Wall);
        return base.CheckWall(includeLayers);
    }
}
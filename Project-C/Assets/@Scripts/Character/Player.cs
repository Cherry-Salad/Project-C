using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;
using Object = UnityEngine.Object;

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

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Player;
        
        JumpForce = 6f;
        DoubleJumpForce = 1f;

        // 기본 공격
        BasicAttack basicAttack = gameObject.GetOrAddComponent<BasicAttack>();
        basicAttack.SetInfo(this, null);
        Skills.Add(basicAttack.Key, basicAttack);

        // Test, TODO: 메인 화면에서 PreLoad 어드레서블을 모두 불러온다
        #region PreLoad 어드레서블 모두 로드
        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, loadCount, totalCount) =>
        {
            // 모두 로드
            if (loadCount == totalCount)
            {
                Managers.Data.Init();

                // 플레이어 스탯
                Data = Managers.Data.PlayerDataDic[PLAYER_ID];
                Hp = Data.Hp;
                MaxHp = Data.MaxHp;
                HpLevel = Data.HpLevel;
                Mp = Data.Mp;
                MaxMp = Data.MaxMp;
                MpLevel = Data.MpLevel;
                Atk = Data.Atk;
                AtkLevel = Data.AtkLevel;
                MoveSpeed = Data.Speed;
                AccessorySlot = Data.AccessorySlot;

                // 확인용
                Debug.Log($"Hp: {Hp}, MaxHp: {MaxHp}, HpLevel: {HpLevel}");
                Debug.Log($"Mp: {Mp}, MaxMp: {MaxMp}, MpLevel: {MpLevel}");
                Debug.Log($"Atk: {Atk}, AtkLevel: {AtkLevel}");
                Debug.Log($"MoveSpeed: {MoveSpeed}, AccessorySlot: {AccessorySlot}, Data parsing successful!");

                // 플레이어 스킬
                foreach (int skillId in Data.SkillIdList)
                {
                    if (Managers.Data.PlayerSkillDataDic.TryGetValue(skillId, out var data) == false)
                        return;

                    Debug.Log($"{data.CodeName}: {skillId}");

                    var type = Type.GetType(data.CodeName);
                    if (type == null)
                        return;

                    // GetOrAddComponent가 안돼서 null 검사
                    PlayerSkillBase skill = gameObject.GetComponent(type) as PlayerSkillBase;
                    if (skill == null)
                        skill = gameObject.AddComponent(type) as PlayerSkillBase;

                    skill.SetInfo(this, data);
                    Skills.Add(skill.Key, skill);
                }
            }
        });
        #endregion

        return true;
    }

    #region 입력 감지
    void Update()
    {
        GetInput();
    }

    /// <summary>
    /// 입력 키를 감지한다.
    /// 대시, 피격, 스킬을 사용할 때 캐릭터는 추가적인 조작 불가능(ex: 대시하는 동안 공격과 점프는 불가능)
    /// </summary>
    void GetInput()
    {
        if (State == ECreatureState.Dash || State == ECreatureState.Hurt || State == ECreatureState.Skill)
            return;

        // TODO: 입력 키 설정이 구현되면 불러오는 것으로 바꾼다

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
            // 벽에 매달린 상태이거나 벽 타기 중이라면 벽 점프로 전환
            if (State == ECreatureState.WallCling || State == ECreatureState.WallClimbing)
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
                Debug.Log($"입력된 키: {keyCode}");

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
                Debug.Log($"입력된 키: {keyCode}");
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
    #endregion

    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();
        
        // 임시로 플레이어만 먼지 효과를 연출한다
        switch (State)
        {
            case ECreatureState.Dash:
                SpawnDust();
                break;
            case ECreatureState.WallCling:
                SpawnDust();
                break;
        }
    }

    protected override void UpdateController()
    {
        // 물리 상태를 업데이트 한 뒤에 입력 처리
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

    protected override void UpdateWallCling()
    {
        if (_moveDirKeyPressed)
            State = ECreatureState.WallClimbing;

        base.UpdateWallCling();
    }

    protected override void UpdateWallClimbing()
    {
        if (_moveDirKeyPressed == false)
        {
            State = ECreatureState.WallCling;
            return;
        }

        base.UpdateWallClimbing();
    }

    protected override void OnWallJump()
    {
        base.OnWallJump();
        _isWallJump = true;
    }

    /// <summary>
    /// 점프 키를 누르고 있을 때 더 높이 점프한다
    /// </summary>
    void OnJumpHold()
    {
        // 벽 점프하고 기본(1단) 점프로 전환될 때까지 추가 점프 힘을 적용하지 않는다
        if (State == ECreatureState.Jump && _isWallJump == false && _hasDoubleJumped == false)
        {
            //Rigidbody.AddForce(Vector2.up * _jumpHoldForce, ForceMode2D.Impulse); // 이건 영 조작감이 별로라 velocity를 사용
            Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, JumpForce + _jumpHoldForce);
            State = ECreatureState.Jump;
            return;
        }
    }

    protected override bool OnDash()
    {
        if (_completeDashCooldown == false)
            return false;

        if (base.OnDash() == false) 
            return false;
        
        StartCoroutine(CoDashCooldown());
        return true;
    }

    public override void OnDamaged(float damage = 1f, Creature attacker = null)
    {
        // 이미 피격 당하여 무적 상태라면 대미지를 입지 않는다
        if (State == ECreatureState.Hurt)
            return;

        // HP 감소
        Hp -= damage;
        // TODO: HP가 모두 감소 시 사망 처리

        base.OnDamaged(damage, attacker);

        // 무적 상태, TODO: 특정 장애물과 충돌하면 무적이 아니라 체크 포인트로 바로 이동
        State = ECreatureState.Hurt;
        StartCoroutine(CoHandleInvincibility());

        Rigidbody.velocity = Vector2.zero;

        // 살짝 위로 튀어오르듯이
        float dirX = Mathf.Sign(Rigidbody.position.x - attacker.Rigidbody.position.x);  // x값은 -1 또는 1로 고정
        Vector2 knockbackDir = (Vector2.up * 1.5f) + new Vector2(dirX, 0).normalized;

        // 넉백
        float knockbackForce = 3f;
        Rigidbody.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 몬스터와의 충돌 확인
        MonsterBase monster = collision.gameObject.GetComponent<MonsterBase>();

        // 몬스터 충돌할 때 대시 중이라면 피격 무시
        if (monster != null && State != ECreatureState.Dash)
            OnDamaged(attacker: monster);

        // TODO: 장애물와 충돌 시 피격
    }

    IEnumerator CoDashCooldown()
    {
        _completeDashCooldown = false;
        yield return new WaitForSeconds(_dashCoolTime);
        _completeDashCooldown = true;
    }

    IEnumerator CoHandleInvincibility(float duration = 0.5f)
    {
        // 지속 시간만큼 무적 상태이다
        yield return new WaitForSeconds(duration);

        // 캐릭터가 공중에 있으면 점프로 전환
        State = CheckGround() ? ECreatureState.Idle : ECreatureState.Jump;
    }
}
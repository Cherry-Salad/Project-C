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

    //UI를 위한 이벤트 추가
    public event Action OnHpChanged;
    public event Action OnMpChanged;
    public event Action OnDataLoaded; //데이터 로드 완료 이벤트 추가

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Player;

        // 충돌 필터링
        if (BodyHitBox != null)
        {
            BodyHitBox.isTrigger = true;
            // 플레이어 BodyHitBox에 태그를 Player로 하지 않으면, 몬스터가 플레이어 제대로 못 찾는다

            LayerMask includeLayers = 0;
            includeLayers.AddLayer(ELayer.Monster);
            BodyHitBox.includeLayers = includeLayers;
        }

        JumpForce = 6f;
        DoubleJumpForce = 1f;

        // 기본 공격
        BasicAttack basicAttack = gameObject.GetOrAddComponent<BasicAttack>();
        basicAttack.SetInfo(this, null);
        Skills.Add(basicAttack.Key, basicAttack);

        StartCoroutine(LoadPlayerData());

        return true;
    }

    // Addressables에서 데이터 로드 (비동기)
    private IEnumerator LoadPlayerData()
    {
        yield return new WaitUntil(() => Managers.Resource != null && Managers.Data != null);

        Debug.Log("Addressables 데이터 로드 시작...");

        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, loadCount, totalCount) =>
        {
            if (loadCount == totalCount)
            {
                Managers.Data.Init();

                //플레이어 데이터 적용
                ApplyPlayerData();

                //플레이어 스킬 로드
                LoadSkills();

                //맵 및 카메라 설정
                SetMapAndCamera();

                //UI 업데이트 이벤트 호출 (확인용 + 확장용 나중에 안쓰면 걍 지우기)
                TriggerOnDataLoaded();
            }
        });
    }

     //Addressables 데이터 로드 후 플레이어 데이터 적용
    private void ApplyPlayerData()
    {
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

        Debug.Log($"데이터 로드 완료! Hp: {Hp}, MaxHp: {MaxHp}, HpLevel: {HpLevel}");
    }

     //플레이어 스킬 로드
    private void LoadSkills()
    {
        Debug.Log("플레이어 스킬 로드 시작...");

        foreach (int skillId in Data.SkillIdList)
        {
            if (!Managers.Data.PlayerSkillDataDic.TryGetValue(skillId, out var data))
                continue;

            Debug.Log($"스킬 로드: {data.CodeName} (ID: {skillId})");

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

     //맵 및 카메라 설정
    private void SetMapAndCamera()
    {
        // 맵 불러오기
        Managers.Map.LoadMap("TestMap");

        // 카메라 설정
        CameraController camera = Camera.main.GetComponent<CameraController>();
        if (camera != null)
            camera.Target = this;
    }

    public void TriggerOnHpChanged() { OnHpChanged?.Invoke(); } // HP 업데이트 이벤트 트리거
    public void TriggerOnMpChanged() { OnMpChanged?.Invoke(); } // MP 업데이트 이벤트 트리거
    public void TriggerOnDataLoaded() { OnDataLoaded?.Invoke(); } // 데이터 로드 완료 이벤트 트리거

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
    #endregion

    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();
        
        // 임시로 플레이어만 먼지 효과를 연출한다
        switch (State)
        {
            case ECreatureState.Dash:
                OnSpawnDust();
                break;
            case ECreatureState.WallCling:
                OnSpawnDust();
                break;
            //case ECreatureState.Dead: // 애니메이션 이벤트로 호출한다
            //    OnSpawnDust();
            //    break;
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

    protected override void UpdateSkill()
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

        float wallClimbingSpeed = MoveSpeed / 3f;
        Rigidbody.velocity = Vector2.up * wallClimbingSpeed;
    }

    protected override void OnJump()
    {
        // 벽을 감지하면 벽 점프로 전환
        bool isWall = CheckWall();
        if (State == ECreatureState.WallCling || State == ECreatureState.WallClimbing || isWall)
        {
            OnWallJump();
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
        // 벽 점프하고 기본(1단) 점프로 전환될 때까지 추가 점프 힘을 적용하지 않는다
        if (State == ECreatureState.Jump && _isWallJump == false && _hasDoubleJumped == false)
        {
            //Rigidbody.AddForce(Vector2.up * _jumpHoldForce, ForceMode2D.Impulse); // 이건 영 조작감이 별로라 velocity를 사용
            Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, JumpForce + _jumpHoldForce);
            State = ECreatureState.Jump;
            return;
        }
    }

    protected override void OnDash(float distance = 3f, float speedMultiplier = 3f, bool ignorePhysics = true, bool ignoreObstacle = false)
    {
        // 대시 쿨타임 완료 여부
        if (_completeDashCooldown == false)
            return;

        base.OnDash(distance, speedMultiplier, ignorePhysics, ignoreObstacle);
        StartCoroutine(CoDashCooldown());
    }

    public override void OnDamaged(float damage = 1f, Creature attacker = null)
    {
        // 이미 피격 당하여 무적 상태라면 대미지를 입지 않는다
        if (State == ECreatureState.Dead || State == ECreatureState.Hurt)
            return;

        // HP 감소
        Hp -= damage;
        OnHpChanged?.Invoke(); //체력 변경 이벤트 호출

        // 무적 상태, TODO: 특정 장애물과 충돌하면 무적이 아니라 체크 포인트로 바로 이동
        State = ECreatureState.Hurt;
        StartCoroutine(CoHandleInvincibility());

        // 살짝 위로 튀어오르듯이
        Rigidbody.velocity = Vector2.zero;
        float dirX = Mathf.Sign(Rigidbody.position.x - attacker.Rigidbody.position.x);  // x값은 -1 또는 1로 고정
        Vector2 knockbackDir = (Vector2.up * 1.5f) + new Vector2(dirX, 0);

        // 넉백
        float knockbackForce = 4.5f;
        Rigidbody.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
    }

    public override void OnDied()
    {
        base.OnDied();
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        // 트리거가 활성화된 오브젝트들이 활성화(SetActive(true))가 되어야 플레이어와 충돌을 감지한다.
        // 그러므로, 기본적으로 바디 히트 박스가 활성화되어야 한다.

        // 사망했다면 충돌 감지를 할 필요없다
        if (State == ECreatureState.Hurt || State == ECreatureState.Dead)
            return;

        // 대시 중일 때 몬스터와 충돌하면 안된다
        // MonsterBase monster = collision.gameObject.GetComponent<MonsterBase>();
        if (State != ECreatureState.Dash && collision.gameObject.CompareTag("EnemyHitBox"))
        {
            Debug.Log($"{collision.name} 충돌");
            OnDamaged(attacker: this);
        }

        // TODO: 장애물과 충돌 시 피격
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

        if (Hp > 0)
            State = CheckGround() ? ECreatureState.Idle : ECreatureState.Jump;  // 캐릭터가 공중에 있으면 점프로 전환
        else
            // 사망 판정
            StartCoroutine(CoUpdateDead());
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

            yield return null;
        }

        // 사망
        Rigidbody.velocity = Vector2.zero;
        State = ECreatureState.Dead;
    }
}
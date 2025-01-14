using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Player : Creature
{
    public Data.PlayerData Data;

    public int Mp { get; protected set; }
    public int MaxMp { get; protected set; }
    public int HpLevel { get; protected set; }
    public int MpLevel { get; protected set; }

    bool _moveDirKeyPressed = false;

    bool _isWallJump = false;
    bool _jumpKeyPressed = false;
    float _jumpKeyPressedTime = 0f; // 점프 키를 누르고 있는 시간
    float _jumpDuration = 0.3f; // 점프 유지 시간, 원래는 0.5초로 했는데 이게 체감상 손가락에 좀 무리가 가더라구..
    float _jumpHoldForce = 0.2f;    // 점프 키를 유지했을 때 적용되는 힘

    float _dashCoolTime = 1.0f; // 대시 쿨타임
    bool _isDashCooldownComplete = true;    // 대쉬 쿨다운 완료 여부

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Player;
        
        JumpForce = 6f;
        DoubleJumpForce = 1f;

        // Test
        Managers.Resource.LoadAsync<Object>("Dust");

        // Test, 플레이어 데이터 파싱
        Managers.Resource.LoadAsync<Object>("PlayerData", (obj) => {
            Managers.Data.Init();   // TODO: 메인 화면에서 모든 데이터 초기화

            // 플레이어 데이터
            Data = Managers.Data.PlayerDataDic[PLAYER_ID];
            Hp = Data.Hp;
            MaxHp = Data.MaxHp;
            HpLevel = Data.HpLevel;
            Mp = Data.Mp;
            MaxMp = Data.MaxMp;
            MpLevel = Data.MpLevel;
            MoveSpeed = Data.Speed;

            // 확인용
            Debug.Log($"Hp: {Hp}, MaxHp: {MaxHp}, HpLevel: {HpLevel}");
            Debug.Log($"Mp: {Mp}, MaxMp: {MaxMp}, MpLevel: {MpLevel}");
            Debug.Log($"MoveSpeed: {MoveSpeed}, Data parsing successful!");
        });
        
        return true;
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

        if (IsDashInput())
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
            return false;

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

    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();
        
        // 임시로 플레이어만 먼지 효과를 연출한다
        switch (State)
        {
            case ECreatureState.Dash:
                ShowDustEffect();
                break;
            case ECreatureState.WallCling:
                ShowDustEffect();
                break;
        }
    }

    protected override void UpdateController()
    {
        // 물리 상태를 업데이트 한 뒤에 입력 처리
        base.UpdateController();
        GetInput();
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
        // 추가 점프 힘 적용
        // 벽 점프하고 기본(1단) 점프로 전환될 때까지 추가 점프 힘을 적용하지 않는다
        if (State == ECreatureState.Jump && _isWallJump == false)
        {
            // 공중이므로 기본 중력 적용
            Rigidbody.gravityScale = DefaultGravityScale;

            //Rigidbody.AddForce(Vector2.up * _jumpHoldForce, ForceMode2D.Impulse); // 이건 영 조작감이 별로라 velocity를 사용
            Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, JumpForce + _jumpHoldForce);
            State = ECreatureState.Jump;
            return;
        }
    }

    protected override void OnDash()
    {
        if (_isDashCooldownComplete == false)
            return;

        base.OnDash();
        StartCoroutine(CoDashCooldown());
    }

    IEnumerator CoDashCooldown()
    {
        _isDashCooldownComplete = false;
        yield return new WaitForSeconds(_dashCoolTime);
        _isDashCooldownComplete = true;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Player : Creature
{
    bool _moveDirKeyPressed = false;

    float _jumpStartTime = 0f;  // 점프 시작 시간

    float _dashCoolTime = 1.0f; // 대시 쿨타임
    bool _isDashCooldownComplete = true;    // 대쉬 쿨다운 완료 여부

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Player;
        MoveSpeed = 5f;    // TODO: 데이터를 파싱하여 MoveSpeed 불러오기

        return true;
    }

    void GetInput() // 입력 감지, TODO: 입력 키 설정이 구현되면 불러오는 것으로 바꾼다.
    {
        // 대시, 피격, 스킬 사용 중일 때 캐릭터는 추가적인 조작 불가능(ex: 대시하는 동안 공격과 점프는 불가능)
        if (State == ECreatureState.Dash || State == ECreatureState.Hurt || State == ECreatureState.Skill)
            return;

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
        // 점프키 입력
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            // TODO: 입력에 따라 점프 높이 조절
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

    protected override void UpdateController()
    {
        // 물리 상태를 업데이트 한 뒤에 입력 처리
        base.UpdateController();
        GetInput();
    }

    protected override void UpdateIdle()
    {
        if (_moveDirKeyPressed)
        {
            State = ECreatureState.Run;
        }

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
    }

    protected override void UpdateWallCling()
    {
        if (_moveDirKeyPressed)
        {
            State = ECreatureState.WallClimbing;
        }

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
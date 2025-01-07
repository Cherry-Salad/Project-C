using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Player : Creature
{
    bool _moveDirKeyPressed = false;
    
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

        if (IsDashInput() || IsJumpInput())
            return;
        
        _moveDirKeyPressed = IsMoveDirInput();
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
        if (State != ECreatureState.DoubleJump && Input.GetKey(KeyCode.LeftControl) && CheckGround())
        {
            // 점프키를 입력하고 캐릭터가 바닥에 닿아있으면 점프 가능
            // 이단 점프 중에는 점프 불가능
            State = ECreatureState.Jump;
            return true;
        }
        else if (State == ECreatureState.Jump && Input.GetKey(KeyCode.LeftControl) && CheckGround() == false)
        {
            // TODO: 이단 점프
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

        return false;
    }

    protected override void UpdateController()
    {
        GetInput();
        base.UpdateController();
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
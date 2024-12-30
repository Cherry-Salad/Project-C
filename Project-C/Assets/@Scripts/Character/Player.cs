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
        // TODO: 코드 리팩토링, 내가 봐도 이 코드 진짜 별로다. 일단 기능만 잘 구현하고 리팩토링할 계획이다.
        if (State == ECreatureState.Dash || State == ECreatureState.Hurt || State == ECreatureState.Skill)
            return;
        
        // 대시키 입력
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Debug.Log("LeftShift");
            _moveDirKeyPressed = false;
            OnDash();
            return;
        }

        // 방향키 입력
        bool upPressed = Input.GetKey(KeyCode.UpArrow);
        bool leftPressed = Input.GetKey(KeyCode.LeftArrow);
        bool downPressed = Input.GetKey(KeyCode.DownArrow);
        bool rightPressed = Input.GetKey(KeyCode.RightArrow);

        int pressedCount = (upPressed ? 1 : 0) + (leftPressed ? 1 : 0) + (downPressed ? 1 : 0) + (rightPressed ? 1 : 0);
        
        // 방향키 입력을 두 개 이상 눌렸다면 입력 취소
        if (pressedCount > 1)
        {
            _moveDirKeyPressed = false;
            return;
        }

        if (upPressed)
        {
            // TODO: 벽 감지
            Debug.Log("upPressed");
            _moveDirKeyPressed = true;
            return; // TODO: 벽 타기
        }
        else if (leftPressed)
        {
            // TODO: 벽 감지
            Debug.Log("leftPressed");
            MoveDir = Vector2.left;
            _moveDirKeyPressed = true;
        }
        else if (downPressed)
        {
            // TODO: 벽 감지
            Debug.Log("downPressed");
            _moveDirKeyPressed = true;
            return; // TODO: 벽 타기
        }
        else if (rightPressed)
        {
            // TODO: 벽 감지
            Debug.Log("rightPressed");
            MoveDir = Vector2.right;
            _moveDirKeyPressed = true;
        }
        else
        {
            // 입력이 없다면
            _moveDirKeyPressed = false;
            return;
        }

        LookLeft = MoveDir.x < 0;
    }

    protected override void UpdateController()
    {
        GetInput();
        base.UpdateController();
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        if (_moveDirKeyPressed)
        {
            State = ECreatureState.Run;
        }
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

    protected override void OnDash()
    {
        if (_isDashCooldownComplete == false)
            return;

        StartCoroutine(CoDashCooldown());
        base.OnDash();
    }
    
    IEnumerator CoDashCooldown()
    {
        _isDashCooldownComplete = false;
        yield return new WaitForSeconds(_dashCoolTime);
        _isDashCooldownComplete = true;
    }
}
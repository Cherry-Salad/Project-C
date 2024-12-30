using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Player : Creature
{
    bool _moveDirKeyPressed = false;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Player;
        MoveSpeed = 5f;    // TODO: 데이터를 파싱하여 MoveSpeed 불러오기

        return true;
    }

    void GetMoveDirInput()  // 입력을 감지하여 MoveDir를 얻는다
    {
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
            _moveDirKeyPressed = true;
            return; // TODO: 벽 타기
        }
        else if (leftPressed)
        {
            // TODO: 벽 감지
            MoveDir = Vector2.left;
            _moveDirKeyPressed = true;
        }
        else if (downPressed)
        {
            // TODO: 벽 감지
            _moveDirKeyPressed = true;
            return; // TODO: 벽 타기
        }
        else if (rightPressed)
        {
            // TODO: 벽 감지
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
        GetMoveDirInput();
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

    protected override void UpdateDash()
    {
        base.UpdateDash();
    }

    protected override void UpdateWallClimbing()
    {
        base.UpdateWallClimbing();
    }
}
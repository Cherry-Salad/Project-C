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
        if (Input.GetKey(KeyCode.UpArrow))
        {
            // TODO: 벽 감지
            _moveDirKeyPressed = true;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            // TODO: 벽 감지
            MoveDir = Vector2.left;
            _moveDirKeyPressed = true;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            // TODO: 벽 감지
            _moveDirKeyPressed = true;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            // TODO: 벽 감지
            MoveDir = Vector2.right;
            _moveDirKeyPressed = true;
        }
        else
        {
            MoveDir = Vector2.zero;
            _moveDirKeyPressed = false;
        }
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

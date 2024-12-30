using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Creature : BaseObject
{
    public ECreatureState _state = ECreatureState.None;
    public ECreatureState State
    {
        get { return _state; }
        protected set 
        {
            if (_state != value)
            {
                _state = value;
                UpdateAnimation();
            }
        }
    }

    public Vector2 MoveDir;

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

    public float MoveSpeed { get; protected set; }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        State = ECreatureState.Idle;

        return true;
    }

    void Update()
    {
        UpdateController();
    }

    protected virtual void UpdateAnimation()
    {

        switch (State)  // TODO: 애니메이션 연결
        {
            case ECreatureState.Idle:
                Animator.Play("Idle");
                break;

            case ECreatureState.Run:
                Animator.Play("Run");
                break;

            case ECreatureState.Jump:
                break;
            case ECreatureState.DoubleJump:
                break;
            case ECreatureState.Skill:
                break;
            case ECreatureState.Dash:
                break;
            case ECreatureState.WallClimbing:
                break;
            case ECreatureState.WallCling:
                break;
            case ECreatureState.Hurt:
                break;
            case ECreatureState.Dead:
                break;
        }
    }

    protected virtual void UpdateController()
    {
        switch (State)
        {
            case ECreatureState.Idle:
                UpdateIdle();
                break;
            case ECreatureState.Run:
                UpdateRun();
                break;
            case ECreatureState.Jump:
                break;
            case ECreatureState.DoubleJump:
                break;
            case ECreatureState.Skill:
                break;
            case ECreatureState.Dash:
                UpdateDash();
                break;
            case ECreatureState.WallClimbing:
                UpdateWallClimbing();
                break;
            case ECreatureState.WallCling:
                UpdateWallCling();
                break;
            case ECreatureState.Hurt:
                break;
            case ECreatureState.Dead:
                break;
        }
    }

    protected virtual void UpdateIdle() { }
    
    protected virtual void UpdateRun() 
    {
        Rigidbody.MovePosition(Rigidbody.position + MoveDir * MoveSpeed * Time.deltaTime);
    }

    protected virtual void UpdateDash() { }
    protected virtual void UpdateWallClimbing() { }
    protected virtual void UpdateWallCling() { }
}
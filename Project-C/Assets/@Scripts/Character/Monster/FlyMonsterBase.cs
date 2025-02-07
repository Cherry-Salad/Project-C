using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using static MonsterBase;

public class FlyMonsterBase : MonsterBase
{
    public enum EFlyStyle
    {
        Straight,
        Zigzag,
        Circular
    }

    public enum ETurnStyle
    {
        FixedHorizontal,
        Rotation
    }

    private EFlyStyle _flyStyle;
    private ETurnStyle _turnStyle;
    public EFlyStyle FlyStyle { get { return _flyStyle; } }
    public ETurnStyle TurnStyle { get { return _turnStyle; } }

    [SerializeField] private EFlyStyle _scanFlyStyle = EFlyStyle.Straight;
    [SerializeField] private float _steeringChangeTime = 0.5f;
    
    private Coroutine _steeringAdjustmentCoroutine;
    private float _steering;
    private Vector2 _startPoint;
   
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        DefaultGravityScale = 0;
        _steering = 0;

        _flyStyle = _scanFlyStyle;
        _turnStyle = ETurnStyle.Rotation;
        _startPoint = this.transform.position;
        
        return true;
    }

    protected override void UpdateScanMove() // 동적 감지 상태의 동작
    {
        if (!CheackTargetSearching())
            State = ECreatureState.Run;
    }

    protected override void UpdateIdle()
    {
        if (isCanAttack && BehaviorPattern == EBehaviorPattern.Battle)
            Attack();
        else if(BehaviorPattern == EBehaviorPattern.ScanStand)
        {
            switch(FlyStyle)
            {
                case EFlyStyle.Straight:
                    StopMove();
                    break;
                case EFlyStyle.Zigzag:
                    VelocityUpdate(Vector2.zero);
                    break;
                case EFlyStyle.Circular:
                    break;
            }
        }else
        {
            StopMove();
        }
    }

    protected override void UpdateRun()
    {
        if (CheckWall())
        {
            switch (BehaviorPattern)
            {
                case EBehaviorPattern.ScanMove:
                    TurnObject();
                    break;

                case EBehaviorPattern.ScanStand:
                    break;

                case EBehaviorPattern.Battle:
                    
                    break;
            }
        }

        VelocityUpdate(MoveDir);
    }

    protected void VelocityUpdate(Vector2 moveDir)
    {
        switch(BehaviorPattern)
        {
            case EBehaviorPattern.ScanMove:
            case EBehaviorPattern.ScanStand:
                Vector2 targetMoveDir = new Vector2(moveDir.x, moveDir.y + Mathf.Sin(_steering) * 2);
                Rigidbody.velocity = Vector2.Lerp(Rigidbody.velocity, targetMoveDir * MoveSpeed, Time.deltaTime * 5f);
                break;

            case EBehaviorPattern.Battle:
                Rigidbody.velocity = moveDir.normalized * MoveSpeed;
                break;
        }
    }

    protected override void PatternChangeToBattle() // 배틀 상태 전환시
    {
        base.PatternChangeToBattle();
        _steering = 0;
        _flyStyle = EFlyStyle.Straight;
        StopCoroutine(ref _steeringAdjustmentCoroutine);
    }

    protected override void PatternChangeToScan() // 스캔 상태 전환시 
    {
        base.PatternChangeToScan();

        _flyStyle = _scanFlyStyle;
        _steering = 0;
        SpriteRenderer.flipY = false;
        transform.rotation = Quaternion.identity;
        

        switch (FlyStyle)
        {
            case EFlyStyle.Straight:
                break;

            case EFlyStyle.Zigzag:
                StartCoroutine(ref _steeringAdjustmentCoroutine, SteeringAdjustmentCoroutine());
                break;

            case EFlyStyle.Circular:
                break;
        }
    }

    protected virtual IEnumerator SteeringAdjustmentCoroutine()
    {
        while(true){
            yield return new WaitForSeconds(_steeringChangeTime);
            _steering++;
            _steering = Mathf.Repeat(_steering, 360f);
        }
    }

    protected override void OnTargetExitAttackRange()
    {
        Vector2 targetPos = TargetGameObject.transform.position;
        Vector2 myPos = this.transform.position;

        MoveDir = new Vector2(targetPos.x - myPos.x, targetPos.y - myPos.y);
        MoveDir.Normalize();
        State = ECreatureState.Run;
    }

    protected override void ViewTarget() // 몬스터를 타겟을 바라보도록 지정
    {
        switch(TurnStyle)
        {
            case ETurnStyle.FixedHorizontal:
                base.ViewTarget(); 
                break;

            case ETurnStyle.Rotation:
                RotationView();
                break;
        }
    }

    protected void RotationView() // 타겟을 회전하면서 응시
    {
        Vector2 targetPos = TargetGameObject.transform.position;
        Vector2 myPos = this.transform.position;

        float angle = Mathf.Atan2(targetPos.y - myPos.y, targetPos.x - myPos.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        SpriteRenderer.flipX = false;

        if (myPos.x - targetPos.x < 0)
            SpriteRenderer.flipY = false;
        else
            SpriteRenderer.flipY = true;
    }
}

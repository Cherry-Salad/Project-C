using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using static MonsterBase;
using static UnityEditor.PlayerSettings;

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

    [Header("Fly Monster Style Setting"), Space(10)]
    [SerializeField] private EFlyStyle _scanFlyStyle = EFlyStyle.Straight;
    [SerializeField] private ETurnStyle _battleTurnStyle = ETurnStyle.FixedHorizontal;

    [Header("Fly Monster Sub Object Setting"), Space(10)]
    [SerializeField] private GameObject _navigation;

    [Header("Wave Setting"), Space(10)]
    [SerializeField] private float _waveAmplitude = 1f; // 진동 진폭 (위아래 이동 크기)
    [SerializeField] private float _waveFrequency = 1f; // 진동 변화 속도 
    [SerializeField] private float _waveLerpSpeed = 1f; // 진동 기반 속도 변환 보정값

    private Navigator _navigator;
    private bool _isReturn = false;
    private Vector2? _nextPoint;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        DefaultGravityScale = 0;

        _flyStyle = _scanFlyStyle;
        _turnStyle = ETurnStyle.Rotation;
        
        _navigator = _navigation.GetComponent<Navigator>();
        
        return true;
    }

    protected override void UpdateScanMove() // 동적 감지 상태의 동작
    {
        if (!CheackTargetSearching())
            State = ECreatureState.Run;
    }

    protected override void UpdateReturn()
    {
        if (!_isReturn) return;
        if(CheackTargetSearching())
        {
            _isReturn = false;
            return;
        }

        Vector2 targetPos = _nextPoint.Value;
        TurnObject(this.transform.position, targetPos, true);

        if (_nextPoint == null || Vector2.Distance(this.transform.position, targetPos) > 0.01f)
            this.transform.position = Vector3.MoveTowards(this.transform.position, targetPos, MoveSpeed * Time.deltaTime);

        else
        {
            _nextPoint = _navigator.LoadPath();
            if (_nextPoint == null)
            {
                _isReturn = false;
                base.UpdateReturn();
            }
        }   
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
                
                switch(FlyStyle)
                {
                    case EFlyStyle.Straight:
                        Rigidbody.velocity = moveDir.normalized * MoveSpeed;
                        break;

                    case EFlyStyle.Zigzag:
                        float waveOffset = moveDir.y + Mathf.Sin(Time.time * _waveFrequency) * _waveAmplitude;
                        Rigidbody.velocity = Vector2.Lerp(Rigidbody.velocity, new Vector2(moveDir.x, waveOffset) * MoveSpeed, Time.deltaTime * _waveLerpSpeed);
                        break;
                }
                break;

            case EBehaviorPattern.Battle:
                Rigidbody.velocity = moveDir.normalized * MoveSpeed;
                break;
        }
    }

    protected override void PatternChangeToBattle() // 배틀 상태 전환시
    {
        base.PatternChangeToBattle();

        _flyStyle = EFlyStyle.Straight;
        _turnStyle = _battleTurnStyle;
        _isReturn = false;
    }

    protected override void PatternChangeToScan() // 스캔 상태 전환시 
    {
        base.PatternChangeToScan();

        if(StartDataRecorder.StartDir == Vector2.right)
        {
            MoveDir = Vector2.right;
            SpriteRenderer.flipX = false;
            LookLeft = false;
        }
        else
        {
            MoveDir = Vector2.right;
            SpriteRenderer.flipX = true;
            LookLeft = true;
        }

        _flyStyle = _scanFlyStyle;

        switch (FlyStyle)
        {
            case EFlyStyle.Straight:
                break;

            case EFlyStyle.Zigzag:
                break;

            case EFlyStyle.Circular:
                break;
        }
    }

    protected override void PatternChangeToReturn() // 귀환 상태 전환시 
    {
        SpriteRenderer.flipY = false;
        transform.rotation = Quaternion.identity;
        StopMove();

        _isReturn = _navigator.FindPath(this.transform.position, StartDataRecorder.StartPoint);

        if(!_isReturn) 
            base.PatternChangeToReturn();
        else
            _nextPoint = _navigator.LoadPath();
    }

    protected override void OnTargetExitAttackRange() // 타겟이 범위에서 벗어났을 경우 
    {
        MoveDir = getDirection(this.transform.position, TargetGameObject.transform.position);
        State = ECreatureState.Run;
    }

    protected override void ViewTarget() // 몬스터를 타겟을 바라보도록 지정
    {
        switch(TurnStyle)
        {
            case ETurnStyle.FixedHorizontal:
                TurnObject(this.transform.position, TargetGameObject.transform.position, false);
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

    protected void TurnObject(Vector2 mover, Vector2 target, bool isChangeVector) // 오브젝트 타겟을 바라보도록 회전 
    {
        if(mover.x < target.x)
        {
            LookLeft = false;
            SpriteRenderer.flipX = false;

            if(isChangeVector) MoveDir = Vector2.right;
        }
        else if(mover.x > target.x)
        {
            LookLeft = true;
            SpriteRenderer.flipX = true;
            if(isChangeVector) MoveDir = Vector2.left;
        }
    }

    protected Vector2 getDirection(Vector2 requesterPos, Vector2 targetPos)
    {
        return (targetPos - requesterPos).normalized;
    }
}

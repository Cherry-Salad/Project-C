using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using static MonsterBase;
using static UnityEngine.UI.Image;

public class MonsterBase : Creature
{
    public enum EBehaviorPattern
    {
        ScanMove,
        ScanStand,
        Battle
    }

    private EBehaviorPattern behaviorPattern;
    public EBehaviorPattern BehaviorPattern
    { get { return behaviorPattern; } }

    /*
     *  FIXME
     *  사용되는 변수들은 추후 삭제 혹은 데이터셋과 연결 예정 
     */

    private List<float> _scanAngleListForScanState = new List<float> { 0.2f, 0, -0.2f }; // 탐색 패턴에서 이용되는 감지 각도
    private int _scanRange = 2; // 탐색 거리
    private float _attackRange = 0.5f; // 공격 범위
    private string _target = "Player"; // 타겟 
    private string _monsterRole = "MeleeAttacker"; //몬스터 역할
    private float _moveSpeed = 3f;

    private float _surveillanceTime = 5f; // 한 방향 검사 시간 

    [SerializeReference] private GameObject _qMark; // 상태전환 확인용 오브젝트 (?)
    [SerializeReference] private GameObject _eMark; // 상태전환 확인용 오브젝트 (!)

    private GameObject _targetGameObject;
    private Coroutine _battleTimerCoroutine; // 전투 종료 타이머 코루틴
    private Coroutine _surveillanceCoroutine; // 방향 전환 코루틴
    private readonly EBehaviorPattern _INIT_STAIT = EBehaviorPattern.ScanMove;

    protected override void UpdateController()
    {
        base.UpdateController();

        switch(BehaviorPattern)
        {
            case EBehaviorPattern.ScanMove:
                UpdateScanMove();
                break;

            case EBehaviorPattern.ScanStand:
                UpdateScanStand();
                break;

            case EBehaviorPattern.Battle:
                UpdateBattle();
                break;
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        
        ObjectType = EObjectType.Monster;                              // 오브젝트 타입
        behaviorPattern = _INIT_STAIT;                                 // 시작 행동 패턴 
        _targetGameObject = GameObject.FindGameObjectWithTag(_target); // 타겟 검색 
        this.Rigidbody.velocity = Vector3.zero;                        // 움직임 0으로 초기화 

        _qMark.SetActive(false);
        _eMark.SetActive(false);

        MoveSpeed = _moveSpeed;

        return true;
    }

    protected override void UpdateRun()
    {
        base.UpdateRun();

        if (!CheckFrontGround() || CheckWall())
        {
            switch (BehaviorPattern)
            {
                case EBehaviorPattern.ScanMove:
                    TurnObject();
                    break;

                case EBehaviorPattern.ScanStand:
                    break;

                case EBehaviorPattern.Battle:
                    StopMove();
                    break;
            }
        }
    }

    protected void UpdateScanMove()
    {
        if (SearchingTargetInScanState())
        {
            behaviorPattern = EBehaviorPattern.Battle;
            StartCoroutine(PopUpStateTransitionIcon(_eMark));
        }

        if (CheckGround())
            State = ECreatureState.Run;
    }

    protected void UpdateScanStand()
    {
        if(SearchingTargetInScanState())
        {
            StopSurveillance();
            behaviorPattern = EBehaviorPattern.Battle;
            StartCoroutine(PopUpStateTransitionIcon(_eMark));
        }
        else
        {
            StartSurveillance();
        }
    }

    protected void UpdateBattle() // 전투상태일 때의 움직임 
    {
        float targetPosX = _targetGameObject.transform.position.x;
        float myPosX = this.transform.position.x;
        float rangeArea = _attackRange;

        if (SearchingTargetInBattleState())
            StopBattleEndTimer();
        else    
            StartBattleEndTimer();

        if ((targetPosX - rangeArea) < myPosX && myPosX < (targetPosX + rangeArea)) // 공격범위에 진입했을 시
        {
            State = ECreatureState.Idle; // 추후 SKill 또는 Attack으로 변경 
        }
        else 
        {
            if (CheckFrontGround() && !CheckWall())
                State = ECreatureState.Run;
        }

        if (targetPosX < myPosX)
        {
            if (!LookLeft) TurnObject();
        }
        else
        {
            if (LookLeft) TurnObject();
        }
    }

    protected bool CheckFrontGround() // 전방 아래의 타일 체크 
    {
        float groundCheckDistance = Collider.bounds.extents.y + 0.3f;   // 바닥 감지 거리
        LayerMask groundLayer = LayerMask.GetMask("Ground");            
        
        Vector2 cheackDir = new Vector2(MoveDir.x, MoveDir.y - 1f); // 현재 진행 중인 방향 기준으로 45도 아래

        Debug.DrawRay(Rigidbody.position, cheackDir * groundCheckDistance, Color.red);

        if (Physics2D.Raycast(Rigidbody.position, cheackDir, groundCheckDistance, groundLayer))     
            return true;
        else
            return false;
    }

    protected bool SearchingTargetInScanState() // 탐색 상태에서 타겟 서칭 (리펙토링 예정)
    {
        int layerMask = ~LayerMask.GetMask("Monster");

        foreach(float angle in _scanAngleListForScanState)
        {
            RaycastHit2D hit = Physics2D.Raycast(Rigidbody.position, new Vector2(MoveDir.x, MoveDir.y + angle), _scanRange, layerMask);
            Debug.DrawRay(Rigidbody.position, new Vector2(MoveDir.x, MoveDir.y + angle) * _scanRange, Color.blue);

            if (hit.collider != null && hit.collider.CompareTag(_target))
                return true;
        }

        return false;
    }

    protected bool SearchingTargetInBattleState() // 전투 상태에서 타겟 서칭
    {
        int layerMask = LayerMask.GetMask("Default");

        Collider2D hit = Physics2D.OverlapCircle(transform.position, _scanRange, layerMask);

        if(hit != null && hit.CompareTag(_target))
            return true;

        return false;
    }

    protected void TurnObject() // 오브젝트 회전 
    {
        if (MoveDir == Vector2.left)
        {
            MoveDir = Vector2.right;
            LookLeft = false;
        }
        else
        {
            MoveDir = Vector2.left;
            LookLeft = true;
        }
    }

    protected void StopMove() // 캐릭터 정지 
    {
        this.Rigidbody.velocity = Vector3.zero;
        State = ECreatureState.Idle;
    }

    private IEnumerator battleTimer() // 전투 이탈후 탐지 상태로 돌아가기 위한 배틀 종료 타이머 
    {
        yield return new WaitForSeconds(5);

        StartCoroutine(PopUpStateTransitionIcon(_qMark));
        behaviorPattern = _INIT_STAIT;
    }

    protected void StartBattleEndTimer() // 배틀 종료 타이머 작동 시작
    {
        if (_battleTimerCoroutine == null)
            _battleTimerCoroutine = StartCoroutine(battleTimer());
    }

    protected void StopBattleEndTimer() // 배틀 종료 타이머 작동 중지
    {
        if (_battleTimerCoroutine != null)
        {
            StopCoroutine(_battleTimerCoroutine);
            _battleTimerCoroutine = null;
        }
    }

    private IEnumerator PopUpStateTransitionIcon(GameObject mark) // 상태전환 아이콘 (!,?) 출력
    {
        mark.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        mark.SetActive(false);
    }

    private IEnumerator ChangeDirectionOfSurveillance() // 감시 방향 전환
    {
        while(true){
            yield return new WaitForSeconds(_surveillanceTime);
            TurnObject();
        }
    }

    protected void StartSurveillance() // 감시 방향 전환 시작 
    {
        if (_surveillanceCoroutine == null)
            _surveillanceCoroutine = StartCoroutine(ChangeDirectionOfSurveillance());
    }

    protected void StopSurveillance() // 감시 방향 전환 종료 
    {
        if(_surveillanceCoroutine != null)
        {
            StopCoroutine(_surveillanceCoroutine);
            _surveillanceCoroutine = null;
        }
    }
}

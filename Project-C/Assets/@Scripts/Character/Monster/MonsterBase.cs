using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static Define;
using static MonsterBase;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using static UnityEngine.UI.Image;

public class MonsterBase : Creature
{
    public enum EBehaviorPattern
    {
        ScanMove,
        ScanStand,
        Battle
    }

    private EBehaviorPattern _behaviorPattern;
    public EBehaviorPattern BehaviorPattern { get { return _behaviorPattern; } }

    /*
     *  FIXME
     *  사용되는 변수들은 추후 삭제 혹은 데이터셋과 연결 예정 
     */

    //private List<float> _scanAngleListForScanState = new List<float> { 45f, 0, -45f }; // 탐색 패턴에서 이용되는 감지 각도
    private float _maxScanAngleforScanState = 45f;
    private float _minScanAngleforScanState = -45f;
    [SerializeReference] private float _viewAngle = 15;

    private int _scanRange = 2; // 탐색 거리
    private float _attackRange = 1f; // 공격 범위
    private string _target = "Player"; // 타겟 
    private float _moveSpeed = 3f;

    private float _surveillanceTime = 5f; // 한 방향 검사 시간 

    /*
     * 객체 관리 필드 
     */
    protected Data.MonsterData DataRecorder;
    protected Data.MonsterTypeData TypeRecorder;

    [SerializeReference] private GameObject _qMark; // 상태전환 확인용 오브젝트 (?)
    [SerializeReference] private GameObject _eMark; // 상태전환 확인용 오브젝트 (!)
    [SerializeReference] protected List<GameObject> _hitBoxList; // 히트 박스 보관 리스트 

    private GameObject _targetGameObject;       // 타겟 오브젝트
    private Coroutine _battleTimerCoroutine;    // 전투 종료 타이머 코루틴
    private Coroutine _surveillanceCoroutine;   // 방향 전환 코루틴
    private Coroutine skillRoutine;             // 스킬 사용 관리 코루틴
    protected bool _isCompleteLoad = false;
    private readonly EBehaviorPattern _INIT_STAIT = EBehaviorPattern.ScanMove;

    protected bool isCanAttack = true; // 공격 가능 여부 확인
    protected int selectSkill = 0;
    protected List<IEnumerator> skillCoroutineList;

    
    protected override void UpdateController()
    {
        if (!_isCompleteLoad) return;
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
        skillCoroutineList = new List<IEnumerator>();
        _behaviorPattern = _INIT_STAIT;                                 // 시작 행동 패턴 
        _targetGameObject = GameObject.FindGameObjectWithTag(_target); // 타겟 검색 
        this.Rigidbody.velocity = Vector3.zero;                        // 움직임 0으로 초기화 
        MoveDir = Vector2.right;                                        // 방향 초기화

        _isCompleteLoad = false;

        if (_qMark != null && _eMark != null)
        {
            _qMark.SetActive(false);
            _eMark.SetActive(false);
        }
        
        MoveSpeed = _moveSpeed;

        

        return true;
    }

    protected override void UpdateIdle()
    {
        if (!_isCompleteLoad) return;
        base.UpdateIdle();
        

        if (isCanAttack && BehaviorPattern == EBehaviorPattern.Battle)
            Attack();
    }

    protected override void UpdateRun()
    {
        if (!_isCompleteLoad) return;
        base.UpdateRun();

        if (Input.GetKey(KeyCode.R)){
            StartCoroutine(HurtCoroutine());
        }

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

    protected void UpdateScanMove() // 동적 감지 상태의 동작
    {
        if (SearchingTargetInScanState())
        {
            _behaviorPattern = EBehaviorPattern.Battle;
            StartCoroutine(PopUpStateTransitionIconCoroutine(_eMark));
        }

        if (CheckGround() && (State == ECreatureState.Idle || State == ECreatureState.Jump))
            State = ECreatureState.Run;
    }

    protected void UpdateScanStand() // 정적 감지 상태의 동작
    {
        if(SearchingTargetInScanState())
        {
            StopSurveillance();
            _behaviorPattern = EBehaviorPattern.Battle;
            StartCoroutine(PopUpStateTransitionIconCoroutine(_eMark));
        }
        else
            StartSurveillance();
    }

    protected void UpdateBattle() // 전투상태일 때의 동작
    {
        if (State == ECreatureState.Skill) return;

        float targetDistance = Vector2.Distance(this.transform.position, _targetGameObject.transform.position);

        if (SearchingTargetInBattleState())
            StopBattleEndTimer();
        else    
            StartBattleEndTimer();

        if (targetDistance <= _attackRange) // 공격범위에 진입했을 시
        {
            isCanAttack = true;
            StopMove();
        }
        else 
        {
            float targetPosX = _targetGameObject.transform.position.x;
            float myPosX = this.transform.position.x;
            float rangeArea = _attackRange;

            isCanAttack = false;

            if ((targetPosX - rangeArea) < myPosX && myPosX < (targetPosX + rangeArea))
                StopMove();
            else if (CheckFrontGround() && !CheckWall())
                State = ECreatureState.Run;
        }

        if (_targetGameObject.transform.position.x < this.transform.position.x)
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
        
        Vector2 cheackDir = new Vector2(MoveDir.x, MoveDir.y - 1f);     // 현재 진행 중인 방향 기준으로 45도 아래

        Debug.DrawRay(Rigidbody.position, cheackDir * groundCheckDistance, Color.red);

        if (Physics2D.Raycast(Rigidbody.position, cheackDir, groundCheckDistance, groundLayer))     
            return true;
        else
            return false;
    }

    protected bool SearchingTargetInScanState() // 탐색 상태에서 타겟 서칭 
    {
        if(_viewAngle <= 0) return false;

        int layerMask = ~LayerMask.GetMask("Monster");
        float angle = _minScanAngleforScanState;

        while(angle <= _maxScanAngleforScanState)
        {
            Vector2 unitVector = GetUnitVectorFromAngle(angle);
            Vector2 scanVector = new Vector2(unitVector.x * MoveDir.x, unitVector.y);

            RaycastHit2D hit = Physics2D.Raycast(Rigidbody.position, scanVector, _scanRange, layerMask);
            Debug.DrawRay(Rigidbody.position, scanVector * _scanRange, Color.blue);

            angle += _viewAngle;

            if (hit.collider != null && hit.collider.CompareTag(_target))
                return true;
        }

        return false;
    }

    protected bool SearchingTargetInBattleState() // 전투 상태에서 타겟 서칭
    {
        int layerMask = LayerMask.GetMask("Player");

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

    protected void StopMove() // 오브젝트 정지 
    {
        this.Rigidbody.velocity = Vector2.zero;
        State = ECreatureState.Idle;
    }

    /// <summary>
    /// 오브젝트를 진동 시킨다.
    /// </summary>
    /// <param name="horizontal">수평 진동 크기</param>
    /// <param name="vertical">수직 진동 크기</param>
    /// <param name="amplitude">진폭</param>
    /// <param name="duration">진동 주기 </param>
    /// <returns></returns>
    protected IEnumerator ObjectVibrationCoroutine(float horizontal, float vertical, float amplitude = 0.1f, float duration = 0.01f) // 오브젝트 진동 
    {
        Vector2 originVector = Rigidbody.velocity;
        Vector2 originPosition = Rigidbody.position;

        float h = 0;
        float v = 0;

        Rigidbody.velocity = Vector2.zero;

        try
        {
            while (true)
            {
                float newPosX = this.transform.position.x + Mathf.Sin(h * Mathf.Deg2Rad) * amplitude;
                float newPosY = this.transform.position.y + Mathf.Sin(v * Mathf.Deg2Rad) * amplitude;
                this.transform.position = new Vector2(newPosX, newPosY);

                h += horizontal;
                v += vertical;

                yield return new WaitForSeconds(duration);
            }
        }
        finally
        {
            Rigidbody.velocity = originVector;
            Rigidbody.position = originPosition;
        }    
    }

    protected Vector2 GetUnitVectorFromAngle(float angle) // 각도를 단위 벡터로 변환
    {
        float radianAngle = Mathf.Deg2Rad * angle;
        float x = Mathf.Cos(radianAngle);
        float y = Mathf.Sin(radianAngle);

        return new Vector2(x, y);
    }

    protected void StartCoroutine(ref Coroutine coroutine, IEnumerator routine) // 특정 코루틴 시작
    {
        if (coroutine == null)
            coroutine = StartCoroutine(routine);
    }

    protected void StopCoroutine(ref Coroutine coroutine) // 특정 코루틴 종료 
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    protected void StartSurveillance() // 감시 방향 전환 시작 
    {
        StartCoroutine(ref _surveillanceCoroutine, ChangeDirectionOfSurveillanceCoroutine());
    }

    protected void StopSurveillance() // 감시 방향 전환 종료 
    {
        StopCoroutine(ref _surveillanceCoroutine);
    }

    private IEnumerator ChangeDirectionOfSurveillanceCoroutine() // 감시 방향 전환
    {
        while (true)
        {
            yield return new WaitForSeconds(_surveillanceTime);
            TurnObject();
        }
    }

    protected void StartBattleEndTimer() // 배틀 종료 타이머 작동 시작
    {
        StartCoroutine(ref _battleTimerCoroutine, battleTimerCoroutine());
    }

    protected void StopBattleEndTimer() // 배틀 종료 타이머 작동 중지
    {
        StopCoroutine(ref _battleTimerCoroutine);
    }
    private void Attack() // 공격
    {
        StartCoroutine(ref skillRoutine, UsingSkill());
    }

    private IEnumerator UsingSkill() // 스킬 사용 
    {
        ECreatureState originState = State;
        State = ECreatureState.Skill;
        isCanAttack = false;

        try
        {
            yield return StartCoroutine(skillCoroutineList[selectSkill]);
        }
        finally
        {
            SelectNextSkill();
        }

        State = originState;
        StopCoroutine(ref skillRoutine);
    }

    private IEnumerator battleTimerCoroutine() // 전투 이탈후 탐지 상태로 돌아가기 위한 배틀 종료 타이머 
    {
        yield return new WaitForSeconds(5);

        StartCoroutine(PopUpStateTransitionIconCoroutine(_qMark));
        isCanAttack = false;
        _behaviorPattern = _INIT_STAIT;
    }

    private IEnumerator PopUpStateTransitionIconCoroutine(GameObject mark) // 상태전환 아이콘 (!,?) 출력
    {
        mark.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        mark.SetActive(false);
    }

    protected IEnumerator HurtCoroutine()  // 데미지를 받았을 때 
    {
        Coroutine vibrationCoroutine = null;
        ECreatureState originState = (State == ECreatureState.Skill ? ECreatureState.Idle: State);
        float originGravity = Rigidbody.gravityScale;
        Color originColor = this.SpriteRenderer.color;

        try
        {
            State = ECreatureState.Hurt;
            Rigidbody.gravityScale = 0;
            this.SpriteRenderer.color = Color.red;

            StartCoroutine(ref vibrationCoroutine, ObjectVibrationCoroutine(50f, 0));
            yield return new WaitForSeconds(0.15f);

        }finally{
            
            StopCoroutine(ref vibrationCoroutine);

            State = originState;
            Rigidbody.gravityScale = originGravity;
            this.SpriteRenderer.color = originColor;
        }
    }

    protected void shufflingSkill(List<IEnumerator> shuffleList) // 스킬 셔플
    {
        for (int i = 0; i < shuffleList.Count; i++)
        {
            int tempNum = UnityEngine.Random.Range(i, shuffleList.Count);
            IEnumerator temp = shuffleList[i];
            shuffleList[i] = shuffleList[tempNum];
            shuffleList[tempNum] = temp;
        }
    }

    protected void SelectNextSkill() // 다음으로 사용될 스킬 지정 
    {
        selectSkill++;

        if(selectSkill >= skillCoroutineList.Count)
        {
            RegistrationSkill();
            selectSkill = 0;
        }

        //private _scanRange = 2;
        //private _attackRange = 0.5f;
    }

    protected virtual void RegistrationSkill() // 스킬 리스트 등록
    {
        skillCoroutineList.Clear();
    }
    
    protected void ActiveHitBox(int hitBoxNum) // 히트 박스 활성화
    {
        if (_hitBoxList == null) return;
        if (hitBoxNum < 0 || _hitBoxList.Count <= hitBoxNum) return;
        
        if(_hitBoxList[hitBoxNum] != null)
            _hitBoxList[hitBoxNum].SetActive(true);
    }

    public void DeactivateHitBox() // 히트 박스 비활성화 
    {
        if(_hitBoxList == null ||  _hitBoxList.Count == 0) return;

        foreach (GameObject hitBox in _hitBoxList)
        {
            if(hitBox != null) 
                hitBox.SetActive(false);
        }
    }
}

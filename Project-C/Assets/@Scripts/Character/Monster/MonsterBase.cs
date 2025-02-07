using Assets.PixelFantasy.PixelMonsters.Common.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
     * 객체 관리 필드 
     */

    protected Data.MonsterData DataRecorder;
    protected Data.MonsterTypeData TypeRecorder;

    [SerializeField] public int MonsterID;
    [SerializeReference] private GameObject _qMark; // 상태전환 확인용 오브젝트 (?)
    [SerializeReference] private GameObject _eMark; // 상태전환 확인용 오브젝트 (!)
    [SerializeReference] protected List<GameObject> hitBoxList; // 히트 박스 보관 리스트 
    [SerializeReference] private EBehaviorPattern _INIT_STAIT = EBehaviorPattern.ScanMove;

    protected GameObject TargetGameObject;       // 타겟 오브젝트
    private Coroutine _battleTimerCoroutine;    // 전투 종료 타이머 코루틴
    private Coroutine _surveillanceCoroutine;   // 방향 전환 코루틴
    private Coroutine _skillRoutine;             // 스킬 사용 관리 코루틴
    private bool _isCompleteLoad = false;       

    private EBehaviorPattern _originBehaviorPattern;

    protected bool isCanAttack = true;          // 공격 가능 여부 확인
    protected int selectSkill = 0;              // 현재 스킬 순서
    protected List<Tuple<int, IEnumerator>> skillList; // 스킬리스트 

    private float _attackRange = 1f; // 공격 범위
    private int _hp;    //남은 HP

    private const float _MARK_POPUP_TIME = 0.3f;    // 마크 표시 시간
    private const float _HURT_TIME = 0.15f;         // 피격 모션 시간
    private const float _TRANSPARENCY_RISING_CYCLE_FOR_DEAD = 0.03f; //캐릭터 사망시 투명해지는 주기
    private const float _GROUND_DETECTION_OFFSET = 0.3f;    // 바닥 검사 보정거리 

    protected override void UpdateController()
    {
        if (!_isCompleteLoad) return;
        base.UpdateController();

        if(BehaviorPattern != _originBehaviorPattern){
            changeBehaviorPattern();
        }

        if (State == ECreatureState.Hurt || State == ECreatureState.Dead) return;

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

        _isCompleteLoad = false;

        ObjectType = EObjectType.Monster;                              // 오브젝트 타입
        skillList = new List<Tuple<int, IEnumerator>>();

        this.Rigidbody.velocity = Vector3.zero;                        // 움직임 0으로 초기화 
        MoveSpeed = 0;

        _behaviorPattern = _INIT_STAIT;                                 // 시작 행동 패턴 
        _originBehaviorPattern = _behaviorPattern;
       
        if (_qMark != null && _eMark != null)
        {
            _qMark.SetActive(false);
            _eMark.SetActive(false);
        }

        StartCoroutine(LoadData());

        return true;
    }

    public IEnumerator LoadData()
    {
        SpriteRenderer.color = Color.black;

        Task <Data.MonsterData> dataTask = Data.MonsterDataLoader.MonsterDataLoad(MonsterID);
        yield return new WaitUntil(() => dataTask.IsCompleted);

        if (dataTask.IsFaulted)
            yield break;

        DataRecorder = dataTask.Result;

        Task<Data.MonsterTypeData> typeTask = Data.MonsterDataLoader.MonsterTypeLoad(DataRecorder.Type);
        yield return new WaitUntil(() => typeTask.IsCompleted);

        if (dataTask.IsFaulted)
            yield break;

        TypeRecorder = typeTask.Result;

        if (DataRecorder != null && TypeRecorder != null)
        {
            settingData();
            SpriteRenderer.color = Color.white;
        }
    }

    private void settingData()
    {
        TargetGameObject = GameObject.FindGameObjectWithTag(DataRecorder.AttackTarget);
        MoveSpeed = TypeRecorder.Base.MovementSpeed;
        Rigidbody.gravityScale = TypeRecorder.Base.DefaultGravity;
        _hp = DataRecorder.Dynamics.HP;

        if (DataRecorder.IsSpawnViewRight)
        {
            LookLeft = false;
            MoveDir = Vector2.right;
        }
        else
        {
            LookLeft = true;
            MoveDir = Vector2.left;
        }

        _isCompleteLoad = true;
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
            Hit();
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

        float targetDistance = Vector2.Distance(this.transform.position, TargetGameObject.transform.position);

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
            float targetPosX = TargetGameObject.transform.position.x;
            float myPosX = this.transform.position.x;

            isCanAttack = false;

            if ((targetPosX - 1) < myPosX && myPosX < (targetPosX + 1))
                StopMove();
            else if (CheckFrontGround() && !CheckWall())
                State = ECreatureState.Run;
        }

        if (TargetGameObject.transform.position.x < this.transform.position.x)
        {
            if (!LookLeft) TurnObject();
        }
        else
        {
            if (LookLeft) TurnObject();
        }
    }

    private void changeBehaviorPattern()
    {
        switch (BehaviorPattern)
        {
            case EBehaviorPattern.ScanMove:
            case EBehaviorPattern.ScanStand:
                PatternChangeToScan();
                break;

            case EBehaviorPattern.Battle:
                PatternChangeToBattle();
                break;
        }
        _originBehaviorPattern = BehaviorPattern;
    }

    private void PatternChangeToBattle() // 배틀 상태 전환시
    {
        MoveSpeed = TypeRecorder.Base.MovementSpeed * TypeRecorder.Battle.MovementMultiplier;
    }

    private void PatternChangeToScan() // 스캔 상태 전환시 
    {
        MoveSpeed = TypeRecorder.Base.MovementSpeed;
    }

    protected bool CheckFrontGround() // 전방 아래의 타일 체크 
    {
        float groundCheckDistance = Collider.bounds.extents.y + _GROUND_DETECTION_OFFSET;   // 바닥 감지 거리
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
        if(TypeRecorder.Scan.ViewAngle <= 0) return false;

        //int layerMask = ~LayerMask.GetMask("Monster");
        int layerMask = LayerMask.GetMask("Player");
        float angle = TypeRecorder.Scan.MinScanAngle;

        while(angle <= TypeRecorder.Scan.MaxScanAngle)
        {
            Vector2 unitVector = GetUnitVectorFromAngle(angle);
            Vector2 scanVector = new Vector2(unitVector.x * MoveDir.x, unitVector.y);

            // 레이캐스트에서 레이어를 제외하는 방식으로 감지하면 명확하게 못 찾는다
            // 그래서 ~LayerMask.GetMask("Monster") 대신에 LayerMask.GetMask("Player")를 사용하였다.
            // 기왕이면 AddLayer도 써주라
            RaycastHit2D hit = Physics2D.Raycast(Rigidbody.position, scanVector, TypeRecorder.Scan.Distance, layerMask);
            Debug.DrawRay(Rigidbody.position, scanVector * TypeRecorder.Scan.Distance, Color.blue);

            angle += TypeRecorder.Scan.ViewAngle;

            if (hit.collider != null && hit.collider.CompareTag(DataRecorder.AttackTarget))
                return true;
        }

        return false;
    }

    protected bool SearchingTargetInBattleState() // 전투 상태에서 타겟 서칭
    {
        int layerMask = LayerMask.GetMask("Player");

        Collider2D hit = Physics2D.OverlapCircle(transform.position, TypeRecorder.Scan.Distance, layerMask);

        if(hit != null && hit.CompareTag(DataRecorder.AttackTarget))
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
            yield return new WaitForSeconds(DataRecorder.SurveillanceTime);
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
        StartCoroutine(ref _skillRoutine, UsingSkill());
    }

    private IEnumerator UsingSkill() // 스킬 사용 
    {
        ECreatureState originState = State;
        State = ECreatureState.Skill;
        isCanAttack = false;

        try
        {
            yield return StartCoroutine(skillList[selectSkill].Item2);
        }
        finally
        {
            SelectNextSkill();
        }

        State = originState;
        StopCoroutine(ref _skillRoutine);
    }

    private IEnumerator battleTimerCoroutine() // 전투 이탈후 탐지 상태로 돌아가기 위한 배틀 종료 타이머 
    {
        yield return new WaitForSeconds(TypeRecorder.Battle.BattleEndTime);

        StartCoroutine(PopUpStateTransitionIconCoroutine(_qMark));
        isCanAttack = false;
        _behaviorPattern = _INIT_STAIT;
    }

    private IEnumerator PopUpStateTransitionIconCoroutine(GameObject mark) // 상태전환 아이콘 (!,?) 출력
    {
        mark.SetActive(true);
        yield return new WaitForSeconds(_MARK_POPUP_TIME);
        mark.SetActive(false);
    }

    protected IEnumerator HurtCoroutine()  // 데미지를 받았을 때 
    {

        Coroutine vibrationCoroutine = null;
        ECreatureState originState = (State == ECreatureState.Skill ? ECreatureState.Idle: State);

        State = ECreatureState.Hurt;

        float originGravity = Rigidbody.gravityScale;
        float originSpeed = MoveSpeed;
        Color originColor = this.SpriteRenderer.color;

        try
        {
            Rigidbody.gravityScale = 0;
            MoveSpeed = 0;
            this.SpriteRenderer.color = Color.red;

            StartCoroutine(ref vibrationCoroutine, ObjectVibrationCoroutine(50f, 0));
            yield return new WaitForSeconds(_HURT_TIME); 

        }finally{
            
            StopCoroutine(ref vibrationCoroutine);

            Rigidbody.gravityScale = originGravity;
            MoveSpeed = originSpeed;
            this.SpriteRenderer.color = originColor;

            State = originState;
        }
    }

    protected virtual IEnumerator Dead()
    {
        State = ECreatureState.Dead;
        Coroutine vibrationCoroutine = null;
        Color color = this.SpriteRenderer.color;

        try
        {
            StartCoroutine(ref vibrationCoroutine, ObjectVibrationCoroutine(50f, 0));

            for(float i = 1f; i >= 0; i -= 0.1f)
            {
                color.a = i;
                SpriteRenderer.color = color;
                yield return new WaitForSeconds(_TRANSPARENCY_RISING_CYCLE_FOR_DEAD); 
            }
        }
        finally
        {
            StopCoroutine(ref vibrationCoroutine);
            GameObject.Destroy(this.gameObject);
        }
    }

    public void Hit(int DMG = 1) // 공격 받았을 경우의 처리 
    {
        if (State == ECreatureState.Dead || State == ECreatureState.Hurt) return;

        _hp -= DMG;

        if (_hp <= 0)
        {
            StartCoroutine(Dead());
        }
        else
        {
            StartCoroutine(HurtCoroutine());
        }
    }

    protected void shufflingSkill(List<Tuple<int, IEnumerator>> shuffleList) // 스킬 셔플
    {
        for (int i = 0; i < shuffleList.Count; i++)
        {
            int tempNum = UnityEngine.Random.Range(i, shuffleList.Count);
            Tuple<int, IEnumerator> temp = shuffleList[i];
            shuffleList[i] = shuffleList[tempNum];
            shuffleList[tempNum] = temp;
        }
    }

    protected void SelectNextSkill() // 다음으로 사용될 스킬 지정 
    {
        selectSkill++;

        if(selectSkill >= skillList.Count)
        {
            RegistrationSkill();
            selectSkill = 0;
        }

        _attackRange = TypeRecorder.Battle.Attack[skillList[selectSkill].Item1].AttackRange;
    }

    protected virtual void RegistrationSkill() // 스킬 리스트 등록
    {
        skillList.Clear();
    }
    
    protected void ActiveHitBox(int hitBoxNum) // 히트 박스 활성화
    {
        if (hitBoxList == null) return;
        if (hitBoxNum < 0 || hitBoxList.Count <= hitBoxNum) return;
        
        if(hitBoxList[hitBoxNum] != null)
            hitBoxList[hitBoxNum].SetActive(true);
    }

    public void DeactivateHitBox() // 히트 박스 비활성화 
    {
        if(hitBoxList == null ||  hitBoxList.Count == 0) return;

        foreach (GameObject hitBox in hitBoxList)
        {
            if(hitBox != null) 
                hitBox.SetActive(false);
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        // 트리거가 활성화된 오브젝트들이 활성화(SetActive(true))가 되어야 플레이어와 충돌을 감지한다.
        // 그러므로, 기본적으로 바디 히트 박스가 활성화되어야 한다.

        // 사망했다면 충돌 감지를 할 필요없다
        if (State == ECreatureState.Dead)
            return;

        // 플레이어 충돌 확인
        // 기존에는 플레이어에서 OnTriggerStay2D를 사용하여 몬스터와의 충돌을 감지하였다.
        // 플레이어와 스킬 히트 박스가 부모와 자식 관계이며, 둘다 트리거를 활성화되어 있다.
        // 자식(스킬 히트 박스)에게 트리거 이벤트가 발생하면 부모(플레이어)의 트리거 이벤트도 같이 호출된다.
        // 그래서 비교적 안전하게 몬스터에서 플레이어와의 충돌을 감지하도록 수정하였다.
        Player player = collision.gameObject.GetComponent<Player>();

        if (player != null)
        {
            Debug.Log($"{player.name} 충돌");
            player.OnDamaged(attacker: this);
        }

        // TODO: 몬스터가 장애물과 충돌한다면?
    }
}

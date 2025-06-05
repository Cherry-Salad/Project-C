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
using static UnityEngine.UI.Image;

public class MonsterProjectile
{
    public GameObject Object;
    public MonsterProjectileBase Projectile;

    public MonsterProjectile(GameObject Object, MonsterProjectileBase Projectile)
    {
        this.Object = Object;
        this.Projectile = Projectile;
    }
}

public struct StartData
{
    public Vector2 StartPoint;
    public Vector2 StartDir;
}

public class MonsterBase : Creature
{
    public enum EBehaviorPattern
    {
        ScanMove,
        ScanStand,
        Battle,
        Return,
        Init
    }

    private EBehaviorPattern _behaviorPattern;
    public EBehaviorPattern BehaviorPattern { get { return _behaviorPattern; } }
    /*
     * 객체 관리 필드 
     */

    protected Data.MonsterData DataRecorder;
    protected Data.MonsterTypeData TypeRecorder;
    protected StartData StartDataRecorder;

    [Header("Monster Default Setting"), Space(10)]
    [SerializeField] public int MonsterID;
    [SerializeReference] private EBehaviorPattern _INIT_STAIT = EBehaviorPattern.ScanMove;
    

    [Header("Monster Sub Object Setting"), Space(10)]
    [SerializeReference] private GameObject _qMark; // 상태전환 확인용 오브젝트 (?)
    [SerializeReference] private GameObject _eMark; // 상태전환 확인용 오브젝트 (!)
    [SerializeReference] protected List<GameObject> hitBoxList; // 히트 박스 보관 리스트 
    [SerializeReference] protected List<GameObject> effectList; // 이펙트 보관 리스트
    [SerializeReference] protected Color originColor = Color.white; // 기본 색

    protected GameObject TargetGameObject;        // 타겟 오브젝트
    private Coroutine _battleTimerCoroutine;      // 전투 종료 타이머 코루틴
    private Coroutine _surveillanceCoroutine;     // 방향 전환 코루틴
    private Coroutine _skillRoutine;              // 스킬 사용 관리 코루틴
    
    protected bool isCompleteLoad = false;

    private EBehaviorPattern _originBehaviorPattern;

    protected bool isCanAttack = true;          // 공격 가능 여부 확인
    protected int selectSkill = 0;              // 현재 스킬 순서
    protected List<Tuple<int, IEnumerator>> skillList; // 스킬리스트 
    protected bool canActivateHitBox = true;

    private float _attackRange = 1f; // 공격 범위
    protected int hp;    //남은 HP

    private const float _MARK_POPUP_TIME = 0.3f;    // 마크 표시 시간
    private const float _HURT_TIME = 0.15f;         // 피격 모션 시간
    private const float _TRANSPARENCY_RISING_CYCLE_FOR_DEAD = 0.03f; //캐릭터 사망시 투명해지는 주기
    private const float _GROUND_DETECTION_OFFSET = 0.3f;    // 바닥 검사 보정거리 

    protected override void UpdateController()
    {
        if (!isCompleteLoad) return;
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

            case EBehaviorPattern.Return:
                UpdateReturn();
                break;

            case EBehaviorPattern.Init:
                _behaviorPattern = _INIT_STAIT;
                break;
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        isCompleteLoad = false;

        ObjectType = EObjectType.Monster;                              // 오브젝트 타입
        skillList = new List<Tuple<int, IEnumerator>>();

        this.Rigidbody.velocity = Vector3.zero;                        // 움직임 0으로 초기화 
        MoveSpeed = 0;

        _behaviorPattern = _INIT_STAIT;                                 // 시작 행동 패턴 
        _originBehaviorPattern = EBehaviorPattern.Init;
       
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
            SpriteRenderer.color = originColor;            
        }
    }

    private void settingData()
    {
        this.transform.position = new Vector3(transform.position.x, transform.position.y, DataRecorder.MonsterLoadNumber);
        TargetGameObject = GameObject.FindGameObjectWithTag(DataRecorder.AttackTarget);
        MoveSpeed = TypeRecorder.Base.MovementSpeed;
        Rigidbody.gravityScale = TypeRecorder.Base.DefaultGravity;
        hp = DataRecorder.Dynamics.HP;

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

        StartDataRecorder = new StartData();
        StartDataRecorder.StartPoint = this.transform.position;
        StartDataRecorder.StartDir = MoveDir;

        SettingProjectile();
        SelectNextSkill();
        SettingSubData();
        isCompleteLoad = true;
    }

    protected virtual void SettingSubData()
    {
        
    }

    protected override void UpdateIdle()
    {
        if (!isCompleteLoad) return;
        base.UpdateIdle();
        
        if (isCanAttack && BehaviorPattern == EBehaviorPattern.Battle)
            Attack();
    }

    protected override void UpdateRun()
    {
        if (!isCompleteLoad) return;
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

    protected virtual void UpdateScanMove() // 동적 감지 상태의 동작
    {
        CheackTargetSearching();

        if (CheckGround() && (State == ECreatureState.Idle || State == ECreatureState.Jump))
            State = ECreatureState.Run;
    }

    protected void UpdateScanStand() // 정적 감지 상태의 동작
    {
        if(CheackTargetSearching())
            StopSurveillance();
        else
            StartSurveillance();
    }

    protected void UpdateBattle() // 전투상태일 때의 동작
    {
        if (State == ECreatureState.Skill) return;
        if (TargetGameObject == null)
        {
            _behaviorPattern = EBehaviorPattern.Return;
            return;
        }

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
            OnTargetExitAttackRange();
        }

        ViewTarget();
    }

    protected virtual void UpdateReturn()
    {
        _behaviorPattern = _INIT_STAIT;
    }
                
    protected virtual bool CheackTargetSearching() // 타겟이 탐색범위내에 감지되는지 확인 
    {
        if (SearchingTargetInScanState())
        {
            _behaviorPattern = EBehaviorPattern.Battle;
            PopupEMark();
            return true;
        }
        else 
            return false;
    }

    protected virtual void OnTargetExitAttackRange() // 타겟이 공격 범위 밖에 있을 경우 실행
    {
        float targetPosX = TargetGameObject.transform.position.x;
        float myPosX = this.transform.position.x;

        isCanAttack = false;

        if ((targetPosX - 1) < myPosX && myPosX < (targetPosX + 1))
            StopMove();
        else if (CheckFrontGround() && !CheckWall())
            State = ECreatureState.Run;
    }

    protected virtual void ViewTarget() // 몬스터를 타겟을 바라보도록 지정
    {
        if (TargetGameObject.transform.position.x < this.transform.position.x)
        {
            if (!LookLeft) TurnObject();
        }
        else
        {
            if (LookLeft) TurnObject();
        }
    }

    private void changeBehaviorPattern() // 행동 패턴 변화시 초기화 
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

            case EBehaviorPattern.Return:
                PatternChangeToReturn();
                break;
        }
        _originBehaviorPattern = BehaviorPattern;
    }

    protected virtual void PatternChangeToBattle() // 배틀 상태 전환시
    {
        MoveSpeed = TypeRecorder.Base.MovementSpeed * TypeRecorder.Battle.MovementMultiplier;
    }

    protected virtual void PatternChangeToScan() // 스캔 상태 전환시 
    {
        MoveSpeed = TypeRecorder.Base.MovementSpeed;
    }

    protected virtual void PatternChangeToReturn() // 리턴 상태 전환시
    {
        _behaviorPattern = _INIT_STAIT;
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

        int layerMask = ~LayerMask.GetMask("Monster", "Default");
        float angle = TypeRecorder.Scan.MinScanAngle;

        while(angle <= TypeRecorder.Scan.MaxScanAngle)
        {
            Vector2 unitVector = GetUnitVectorFromAngle(angle);
            Vector2 scanVector = new Vector2(unitVector.x * MoveDir.x, unitVector.y);

            RaycastHit2D hit = Physics2D.Raycast(Rigidbody.position, scanVector, TypeRecorder.Scan.Distance, layerMask);      
            Debug.DrawRay(Rigidbody.position, scanVector * TypeRecorder.Scan.Distance, Color.blue);

            angle += TypeRecorder.Scan.ViewAngle;

            if (hit.collider != null && hit.collider.CompareTag(DataRecorder.AttackTarget))
                return true;
        }

        return false;
    }

    protected virtual bool SearchingTargetInBattleState() // 전투 상태에서 타겟 서칭
    {
        int layerMask = LayerMask.GetMask("Player");

        Collider2D hit = Physics2D.OverlapCircle(transform.position, TypeRecorder.Scan.Distance, layerMask);

        if(hit != null && hit.CompareTag(DataRecorder.AttackTarget))
            return true;

        return false;
    }

    protected new void TurnObject() // 오브젝트 회전, 크리처에 이미 있으므로 오버라이드 하거나 지워주세요. 만약 오버라이드 할 필요가 없다면, virtual를 지워주세요.
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

    protected void SimpleStopMove() // 오브젝트 정지 (상태전환 없음)
    {
        this.Rigidbody.velocity = Vector2.zero;
    }

    protected void SimpleStopHorizontalMove() // 오브젝트 수평 정지 (상태전환 없음)
    {
        this.Rigidbody.velocity = new Vector2(0, this.Rigidbody.velocity.y);
        
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

    protected void StartCoroutine(ref Coroutine coroutine, IEnumerator routine) // 특정 코루틴 시작 (코루틴 필드 제어)
    {
        if (coroutine == null)
            coroutine = StartCoroutine(routine);
    }

    protected void StartCoroutine(ref bool flag, IEnumerator routine) // 특정 코루틴 시작 (코루틴 플래그 제어)
    {
        if (!flag)
        {
            flag = true;
            StartCoroutine(routine);
        }
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

    protected void Attack() // 공격
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
            EndSkill(originState);
        }
    }

    protected void EndSkill(ECreatureState returnState)
    {
        StopCoroutine(skillList[selectSkill].Item2);

        SelectNextSkill();
        State =  returnState;
        StopCoroutine(ref _skillRoutine);
    }

    private IEnumerator battleTimerCoroutine() // 전투 이탈후 탐지 상태로 돌아가기 위한 배틀 종료 타이머 
    {
        yield return new WaitForSeconds(TypeRecorder.Battle.BattleEndTime);

        PopupQMark();
        isCanAttack = false;
        _behaviorPattern = EBehaviorPattern.Return;
    }

    protected IEnumerator popUpStateTransitionIconCoroutine(GameObject mark) // 상태전환 아이콘 (!,?) 출력
    {
        mark.SetActive(true);
        yield return new WaitForSeconds(_MARK_POPUP_TIME);
        mark.SetActive(false);
    }

    public void PopupEMark()
    {
        StartCoroutine(popUpStateTransitionIconCoroutine(_eMark));
    }

    public void PopupQMark()
    {
        StartCoroutine(popUpStateTransitionIconCoroutine(_qMark));
    }

    protected IEnumerator HurtCoroutine(ECreatureState originState)  // 데미지를 받았을 때 
    {
        Coroutine vibrationCoroutine = null;

        float originGravity = Rigidbody.gravityScale;
        float originSpeed = MoveSpeed;

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
            Rigidbody.velocity = Vector2.zero;
                
            State = originState;
            HitEnd();
        }
    }

    protected virtual IEnumerator Dead() //죽었을 때 처리 
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
                DeadOrganize();
                yield return new WaitForSeconds(_TRANSPARENCY_RISING_CYCLE_FOR_DEAD); 
            }
        }
        finally
        {
            Managers.Map.DespawnObject(this);
            StopCoroutine(ref vibrationCoroutine);
            GameObject.Destroy(this.gameObject);
        }
    }

    public virtual void Hit(int DMG = 1) // 공격 받았을 경우의 처리 
    {
        if (State == ECreatureState.Dead || State == ECreatureState.Hurt) return;
        canActivateHitBox = false;

        ECreatureState originState = State;

        if (originState == ECreatureState.Skill)
        {
            originState = ECreatureState.Idle;
            EndSkill(ECreatureState.Hurt);

        }else{
            State = ECreatureState.Hurt;
        }

        Animator.Play("Idle");
        DeactivateHitBox();

        hp -= DMG;

        if (hp <= 0)
            StartCoroutine(Dead());
        else
            StartCoroutine(HurtCoroutine(originState));
    }

    protected virtual void DeadOrganize() // 죽은후 뒷처리 
    {

    }

    protected virtual void HitEnd() // 공격 받은후 뒷처리 
    {
        canActivateHitBox = true;
        Rigidbody.velocity = Vector2.zero;

        if (BehaviorPattern != EBehaviorPattern.Battle)
        {
            _behaviorPattern = EBehaviorPattern.Battle;
            PopupEMark();
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

        if (selectSkill >= skillList.Count)
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

    protected virtual void SettingProjectile() // 총알 세팅 
    {
        
    }

    protected virtual MonsterProjectile MakeProjectile(int skillNumber) // 탄막 생성
    {
        string PrefabName = TypeRecorder.Battle.Attack[skillNumber].ProjectileName;
        GameObject newObject = Managers.Resource.Instantiate(PrefabName);
        MonsterProjectileBase newProjectile = newObject.GetComponent<MonsterProjectileBase>();

        newProjectile.SetData(TypeRecorder.Battle.Attack[skillNumber].ProjectileID);
        newObject.SetActive(false);

        return new MonsterProjectile(newObject, newProjectile);
    }

    protected void ActiveHitBox(int hitBoxNum) // 히트 박스 활성화
    {
        if (!canActivateHitBox) return;
        activeList(hitBoxList, hitBoxNum);
    }

    public void DeactivateHitBox() // 히트 박스 비활성화 
    {
        deactivateList(hitBoxList);
    }

    protected virtual void ActiveEffect(int effectNum) // 이펙트 활성화
    {
        activeList(effectList, effectNum);
    }

    public virtual void DeactivateEffect() // 이펙트 비활성화 
    {
        deactivateList(effectList);
    }

    private void activeList(List<GameObject> list,int listNum) // 오브젝트 리스트 번호 기반 활성화 
    {
        if (list == null) return;
        if (listNum < 0 || list.Count <= listNum) return;

        if (list[listNum] != null)
            list[listNum].SetActive(true);
    }

    private void deactivateList(List<GameObject> list) // 오브젝트 리스트 비활성화 
    {
        if (list == null || list.Count == 0) return;

        foreach (GameObject member in list)
            member?.SetActive(false);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            // 즉시 사망
            Hit(hp);
        }
    }
}

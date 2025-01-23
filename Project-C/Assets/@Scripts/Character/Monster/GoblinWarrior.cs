using Data;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static Define;
using static UnityEngine.GraphicsBuffer;

public class GoblinWarrior : MonsterBase
{
    [SerializeField] public int MonsterID;

    private float _MAKING_HITBOX_POS = 0.7f;
    private float _ATTACK_RECOVERY_TIME = 0.8f;

    private const int _HITBOX_NUM_BODY = 0;
    private const int _HITBOX_NUM_SWORD_ATTACK = 1;

    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();

        switch (State)
        {
            case ECreatureState.Skill:
                Animator.Play("Attack");
                break;
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        RegistrationSkill();
        DeactivateHitBox();

        StartCoroutine(LoadData());

        return true;
    }
    
    public IEnumerator LoadData()
    {
        SpriteRenderer.color = Color.black;

        Task<Data.MonsterData> dataTask = Data.MonsterDataLoader.MonsterDataLoad(MonsterID);
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
            _isCompleteLoad = true;
            SpriteRenderer.color = Color.white;
        }
    }

    protected override void RegistrationSkill()
    {
        skillCoroutineList.Clear();
        skillCoroutineList.Add(SwordAttack());
        shufflingSkill(skillCoroutineList);
    }


    IEnumerator SwordAttack()
    {
        float posX = this.transform.position.x + (MoveDir.x * _MAKING_HITBOX_POS);
        _hitBoxList[_HITBOX_NUM_SWORD_ATTACK].transform.position = new Vector2(posX, this.transform.position.y);

        yield return new WaitForSeconds(_ATTACK_RECOVERY_TIME);
    }

    public void SwordAttackActiveHitBox()
    {
        ActiveHitBox(_HITBOX_NUM_SWORD_ATTACK);
    }
}

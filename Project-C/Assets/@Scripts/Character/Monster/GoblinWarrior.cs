using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using static UnityEngine.GraphicsBuffer;

public class GoblinWarrior : MonsterBase
{
    
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

        return true;
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

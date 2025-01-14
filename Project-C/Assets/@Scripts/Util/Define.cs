using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum EObjectType
    {
        None,
        Player,
        Monster,
        Npc,
        Projectile,
        Env,    // Environment, 환경(ex: 나무, 돌)
    }

    public enum ECreatureState
    {
        None,
        Idle,
        Run,
        Jump,
        DoubleJump,
        WallJump,
        Skill,
        Dash,
        /// <summary>
        /// 벽 타기
        /// </summary>
        WallClimbing,
        /// <summary>
        /// 벽 매달림
        /// </summary>
        WallCling,
        Hurt,
        Dead,
    }
}

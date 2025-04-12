using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum EScene
    {
        None,
        MainScene,
        TutorialScene,
        BossMonsterScene,
    }

    public enum EObjectType
    {
        None,
        Player,
        Monster,
        Npc,
        Projectile,
        Env,    // Environment, 환경(ex: 나무, 돌)
        Checkpoint,
        SavePoint,
        StartPoint
    }

    public enum ELayer
    {
        Default,
        TransparentFX,
        IgnoreRaycast,
        Water = 4,
        UI, // 5부터 시작
        Wall,
        Ground,
        Monster,
        HitBox,
        Player,
        Projectile,
        Trap,
        Cherry,
        NPC,
        Env,
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

    public static int PLAYER_ID = 100001;
}

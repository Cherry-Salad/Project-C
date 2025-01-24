using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    #region PlayerData
    [Serializable]
    public class PlayerData
    {
        public int DataId;
        public string Name; // 플레이어 이름
        public int Hp;
        public int MaxHp;
        public int HpLevel; // HP 강화 레벨
        public int Mp;
        public int MaxMp;
        public int MpLevel;   // MP 강화 레벨
        public float Atk;   // 공격력
        public int AtkLevel;   // 공격력 강화 레벨
        public float Speed;
        public int AccessorySlot;
        public List<int> SkillIdList;   // 플레이어 스킬 리스트
    }

    [Serializable]
    public class PlayerDataLoader : ILoader<int, PlayerData>
    {
        public List<PlayerData> player = new List<PlayerData>();
        public Dictionary<int, PlayerData> MakeDict()
        {
            Dictionary<int, PlayerData> dict = new Dictionary<int, PlayerData>();
            foreach (PlayerData playerData in player)
                dict.Add(playerData.DataId, playerData);
            return dict;
        }
    }
    #endregion

    #region SkillData
    [Serializable]
    public class SkillData  // 몬스터 스킬과 플레이어 스킬의 베이스
    {
        public int DataId;
        public string CodeName; // 코드상 이름
        public string AnimationName;    // 스킬 애니메이션 이름
        public float CastingTime;   // 시전 시간
        public float RecoveryTime;  // 시전 후 회복 시간(후 딜레이)
        public float CoolTime;  // 쿨타임
        public int HealingValue;    // 회복량
        public int ProjectileId;   // 투사체 데이터 아이디
        public int NumberOfShots;   // 투사체 발사 횟수
        public int DelayBetweenShots;   // 투사체 발사간 시간 간격
        public int InitialAngle;    // 투사체 발사 각도
        public float DamageMultiplier;    // 스킬 데미지곱
        public float AttackRange; // 공격 범위
        public float RetentionTime;   // 공격 유지 시간
    }

    [Serializable]
    public class SkillDataLoader : ILoader<int, SkillData>
    {
        public List<SkillData> skills = new List<SkillData>();
        public Dictionary<int, SkillData> MakeDict()
        {
            Dictionary<int, SkillData> dict = new Dictionary<int, SkillData>();
            foreach (SkillData skillData in skills)
                dict.Add(skillData.DataId, skillData);
            return dict;
        }
    }
    #endregion

    #region PlayerSkillData
    /// <summary>
    /// 스킬 변화 데이터
    /// </summary>
    [Serializable]
    public class Dynamics
    {
        public int Level;   // 스킬 레벨
        public bool IsUnlock;   // 해금 여부
        public KeyCode Key; // 현재 할당 키
    }

    /// <summary>
    /// 게임 UI 출력 관련 (한글)
    /// </summary>
    [Serializable]
    public class DisplayKR
    {
        public string Name; // 출력 이름
        public string Description;  // 스킬 설명
    }

    [Serializable]
    public class PlayerSkillData : SkillData
    {
        public int MPCost;  // 소모 MP
        public int MAXLevel;    // 최대 레벨
        public Dynamics Dynamics;
        public DisplayKR DisplayKR;
    }

    [Serializable]
    public class PlayerSkillDataLoader : ILoader<int, PlayerSkillData>
    {
        public List<PlayerSkillData> PlayerSkill = new List<PlayerSkillData>();
        public Dictionary<int, PlayerSkillData> MakeDict()
        {
            Dictionary<int, PlayerSkillData> dict = new Dictionary<int, PlayerSkillData>();
            foreach (PlayerSkillData skillData in PlayerSkill)
                dict.Add(skillData.DataId, skillData);
            return dict;
        }
    }
    #endregion

    #region ProjectileData
    [Serializable]
    public class ProjectileData
    {
        public int DataId;
        public string Name;
        public int DefaultGravity;
        public float BaseSpeed;
    }

    [Serializable]
    public class ProjectileDataLoader : ILoader<int, ProjectileData>
    {
        public List<ProjectileData> Projectile = new List<ProjectileData>();
        public Dictionary<int, ProjectileData> MakeDict()
        {
            Dictionary<int, ProjectileData> dict = new Dictionary<int, ProjectileData>();
            foreach (ProjectileData projectileData in Projectile)
                dict.Add(projectileData.DataId, projectileData);
            return dict;
        }
    }
    #endregion
}

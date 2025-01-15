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
    /// <summary>
    /// 스킬 효과 관련
    /// </summary>
    [Serializable]
    public class Effect
    {
        public List<float> CastTime;    // 시전 시간
        public int RecoveryTime;    // 시전후 회복 시간
        public List<int> HealingValue;  // 회복량
        public string ProjectileName;   // 투사체 이름
        public int NumberOfShots;   // 투사체 발사 횟수
        public int DelayBetweenShots;   // 투사체 발사간 시간 간격
        public int InitialAngle;    // 투사체 발사 각도
        public float DamageMultiplier;    // 스킬 데미지곱
        public float AttackRange; // 공격 범위
        public float RetentionTime;   // 공격 유지 시간
    }

    [Serializable]
    public class SkillData  // 몬스터 스킬과 플레이어 스킬의 베이스
    {
        //public int DataId;
        public int SkillID;
        public string CodeName; // 코드상 이름
        public string Type; // 스킬 유형, Enum도 나쁘지 않을듯..?
        public string UsageCondition;   // 사용 조건, Enum도 나쁘지 않을듯..?
        public Effect Effect;   // 효과 관련
    }

    [Serializable]
    public class SkillDataLoader : ILoader<int, SkillData>
    {
        public List<SkillData> skills = new List<SkillData>();
        public Dictionary<int, SkillData> MakeDict()
        {
            Dictionary<int, SkillData> dict = new Dictionary<int, SkillData>();
            foreach (SkillData skillData in skills)
                dict.Add(skillData.SkillID, skillData);
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
        //public KeyCode BoundingKey; // 현재 할당 키
        public string BoundingKey;
        public int AdditionalValue; // 추가 연산 값
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
        //public KeyCode DefaultBoundKey; // 기본 할당 키, KeyCode(Enum) 타입도 좋을 듯?
        public string DefaultBoundKey;
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
                dict.Add(skillData.SkillID, skillData);
            return dict;
        }
    }
    #endregion
}

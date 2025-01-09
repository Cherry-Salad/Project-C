using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    #region PlayerData
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
        public int MaxAccessorySlot;
        public List<int> SkillIdList;   // 플레이어 스킬 리스트
    }

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
}

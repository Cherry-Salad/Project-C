using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;
using Object = UnityEngine.Object;

namespace Data
{

    #region MonsterData

    [Serializable]
    public class MonsterDataDictionaryMaker : ILoader<int, MonsterData>
    {
        [JsonProperty("MonsterData")]
        private List<MonsterData> list = new List<MonsterData>();

        public Dictionary<int, MonsterData> MakeDict()
        {
            Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();

            foreach (MonsterData data in list)
                dict.Add(data.DataID, data);
            return dict;
        }
    }

    [Serializable]
    public class MonsterData
    {
        public int DataID;
        public string Type;
        public string CodeName;

        public string TypeLevel;
        public string AttackTarget;
        public List<int> SpawnPosition;
        public bool IsSpawnViewRight;
        public bool Resurrection;
        public float SurveillanceTime;
        public float MonsterLoadNumber;

        public int MaxHP;
        public int AttackSuccessDropMp;

        public MonsterDynamics Dynamics;
        public MonsterDrop Drop;
        public MonsterEvent Event;
        public MonsterGroggy Groggy;
        public MonsterDisplayKR DisplayKR;
    }

    [Serializable]
    public class MonsterDynamics
    {
        public int HP;
        public int GroggyGauge;
        public string State;
        public List<string> StatusEffects;
        public List<bool> isActiveEvent;
    }

    [Serializable]
    public class MonsterDrop
    {
        public int MP;
        public int Money;
        public List<int> Item;
        public float ItemDropProbability;
    }

    [Serializable]
    public class MonsterEvent
    {
        public bool IsEventTrigger;
        public List<int> EventTriggerType;
        public List<string> TriggerEvent;
    }

    [Serializable]
    public class MonsterGroggy
    {
        public bool IsPossible;
        public int MAXGauge;
        public int DurationTime;
        public int DropMP;
    }

    [Serializable]
    public class MonsterDisplayKR
    {
        public string Name;
    }

    #endregion

    #region MonsterType 


    [Serializable]
    public class MonsterTypeDictionaryMaker : ILoader<string, MonsterTypeData>
    {
        [JsonProperty("MonsterTypeData")]
        private List<MonsterTypeData> list = new List<MonsterTypeData>();

        public Dictionary<string, MonsterTypeData> MakeDict()
        {
            Dictionary<string, MonsterTypeData> dict = new Dictionary<string, MonsterTypeData>();

            foreach (MonsterTypeData data in list)
                dict.Add(data.Type, data);
            return dict;
        }
    }

    [Serializable]
    public class MonsterTypeData
    {
        public string Type;

        public MonsterTypeBase Base;    
        public MonsterTypeScan Scan;
        public MonsterTypeBattle Battle;
        
    }

    [Serializable]
    public class MonsterTypeBase
    {
        public string Role;
        public float MovementSpeed;
        public int BelligerenceLevel;
        public float DefaultGravity;
    }

    [Serializable]
    public class MonsterTypeScan
    {
        public float Distance;
        public float MaxScanAngle;
        public float MinScanAngle;
        public float ViewAngle;
    }

    [Serializable]
    public class MonsterTypeBattle
    {
        public float BattleEndTime;
        public float MovementMultiplier;

        public List<MonsterSkillData> Attack;
    }

    [Serializable]
    public class MonsterSkillData
    {
        public string Name;
        public string Type;
        public bool IsBodyHitBox;
        public float RecoveryTime;
        public float WindUpTime;

        public float AttackRange;
        public float RetentionTime;
        public float MovementMultiplier;
        public int InitialAngle;
        public float HitBoxPos;

        public string ProjectileName;
        public int ProjectileID;
        public int NumberOfShots;
        public float DelayBetweenShots;
    }

    #endregion

    public static class MonsterDataLoader
    {
        private static Dictionary<int, MonsterData> _monsterDataDic;
        private static Dictionary<string, MonsterTypeData> _monsterTypeDataDic;
        private static TaskCompletionSource<bool> _loadTaskCompletion = new TaskCompletionSource<bool>();
        private static float _monsterMakingNumber = 1;

        private const string _MONSTER_DATA_PATH = "MonsterData";
        private const string _MONSTER_TYPE_DATA_PATH = "MonsterTypeData";
        private const string _MONSTER_DATA_GROUP = "MonsterDataSet";

        private static bool _isFirst = true;
        private static bool _isLoad = false;

        public static void loadMonsterDataGroup()
        {
            if (_isLoad) return;
            _isFirst = false;

            Managers.Resource.LoadAllAsync<Object>(_MONSTER_DATA_GROUP, (key, loadCount, totalCount) =>
            {
                _isLoad = true;

                if(loadCount == totalCount)
                {
                    _monsterDataDic = Managers.Data.LoadJson<MonsterDataDictionaryMaker, int, MonsterData>(_MONSTER_DATA_PATH).MakeDict();
                    _monsterTypeDataDic = Managers.Data.LoadJson<MonsterTypeDictionaryMaker, string, MonsterTypeData>(_MONSTER_TYPE_DATA_PATH).MakeDict();

                    _isLoad = false;
                    _loadTaskCompletion.TrySetResult(true);
                }
            });
        }

        public static async Task LoadData()
        {
            if (_isFirst) loadMonsterDataGroup();
            
            await _loadTaskCompletion.Task;
        }

        public static async Task<MonsterData> MonsterDataLoad(int id)
        {
            await LoadData();
            _monsterDataDic[id].MonsterLoadNumber = _monsterMakingNumber++;

            return _monsterDataDic[id];
        }

        public static async Task<MonsterTypeData> MonsterTypeLoad(string typeName)
        {
            await LoadData();

            return _monsterTypeDataDic[typeName];
        }

    }
}



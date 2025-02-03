using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public Dictionary<int, Data.PlayerData> PlayerDataDic { get; private set; } = new Dictionary<int, Data.PlayerData>();
    public Dictionary<int, Data.PlayerSkillData> PlayerSkillDataDic { get; private set; } = new Dictionary<int, Data.PlayerSkillData>();
    public Dictionary<int, Data.ProjectileData> ProjectileDataDic { get; private set; } = new Dictionary<int, Data.ProjectileData>();

    public void Init()
    {
        PlayerDataDic = LoadJson<Data.PlayerDataLoader, int, Data.PlayerData>("PlayerData").MakeDict();
        PlayerSkillDataDic = LoadJson<Data.PlayerSkillDataLoader, int, Data.PlayerSkillData>("PlayerSkillData").MakeDict();
        ProjectileDataDic = LoadJson<Data.ProjectileDataLoader, int, Data.ProjectileData>("ProjectileData").MakeDict();
    }

    public Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"{path}");
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class GameData
{
    public Data.PlayerData Player = new Data.PlayerData();
    public CurrentSavePointData CurrentSavePoint = new CurrentSavePointData();
    public List<Data.EnvData> Env = new List<Data.EnvData>();   // TODO
}

[Serializable]
public class CurrentSavePointData
{
    public Vector3 Position;
    public Define.EScene SceneType;
}

public class GameManager
{
    GameData _gameData = new GameData();
    public GameData GameData 
    { 
        get { return _gameData; } 
        set { _gameData = value; } 
    }

    public string Path { get { return Application.persistentDataPath + "/GameData.json"; } }

    public Player Player { get; set; }

    public void Init()
    {
        //if (File.Exists(Path))
        //    return;

        // Player
        var player = Managers.Data.PlayerDataDic.Values.ToList();
        foreach (Data.PlayerData p in player)
            GameData.Player = p;

        // 세이브 포인트
        GameData.CurrentSavePoint.SceneType = Define.EScene.None;

        // Map

        // Accessory
    }

    /// <summary>
    /// 게임 데이터 저장
    /// </summary>
    public void Save()
    {
        if (Player != null)
        {
            var player = GameData.Player;
            #region 플레이어 정보
            player.DataId = Player.Data.DataId;
            player.Name = Player.Data.Name;
            player.Hp = Player.Hp;
            player.MaxHp = Player.MaxHp;
            player.HpLevel = Player.HpLevel;
            player.Mp = Player.Mp;
            player.MaxMp = Player.MaxMp;
            player.MpLevel = Player.MpLevel;
            player.Atk = Player.Atk;
            player.AtkLevel = Player.AtkLevel;
            player.Speed = Player.MoveSpeed;
            player.AccessorySlot = Player.AccessorySlot;

            player.SkillIdList = new List<int>();
            var skillList = Player.Skills;

            foreach (var skill in skillList)
                player.SkillIdList.Add(skill.DataId);
            #endregion
            
            GameData.Player = player;
        }

        // 현재 활성화된 세이브 포인트를 찾는다
        if (Managers.Map.CurrentSavePoint != null)
        {
            GameData.CurrentSavePoint.Position = Managers.Map.CurrentSavePoint.transform.position;
            GameData.CurrentSavePoint.SceneType = Managers.Map.CurrentSavePoint.SceneType;
        }

        // Map

        // Accessory

        string jsonStr = JsonUtility.ToJson(GameData);
        File.WriteAllText(Path, jsonStr);
        Debug.Log($"게임 데이터 저장 {Path}");
    }

    /// <summary>
    /// 게임 데이터 불러오기
    /// </summary>
    /// <returns></returns>
    public bool Load()
    {
        if (File.Exists(Path) == false)
        {
            Debug.Log("저장된 데이터가 없어!");
            return false;
        }

        string fileStr = File.ReadAllText(Path);
        GameData data = JsonUtility.FromJson<GameData>(fileStr);

        if (data == null)
        {
            Debug.Log("저장된 데이터가 없어!");
            return false;
        }

        GameData = data;
        return true;
    }
}
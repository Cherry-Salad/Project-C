using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Define;

[Serializable]
public class GameData
{
    public Data.PlayerData Player = new Data.PlayerData();
    public CurrentSavePointData CurrentSavePoint = new CurrentSavePointData();
    public List<MapData> Maps = new List<MapData>();
}

[Serializable]
public class CurrentSavePointData
{
    public Vector3 Position;
    public EScene SceneType; // None이라면 활성화된 세이브 포인트가 없다
}

[Serializable]
public class MapData
{
    public string MapName;
    public List<MapObjectInfo> ObjectInfos = new List<MapObjectInfo>();
}

[Serializable]
public struct MapObjectInfo
{
    public int DataId;
    public string Name;
    public EObjectType ObjectType;
    public Vector3 WorldPos;
    public Vector3Int CellPos;
    public bool FlipX;
    public bool FlipY;
    public bool IsRespawn;  // 파괴되면 더이상 스폰 안 한다.

    public MapObjectInfo(int dataId, string name, EObjectType type, Vector3 worldPos, Vector3Int cellPos, bool flipX, bool flipY, bool isRespawn)
    {
        DataId = dataId;
        Name = name;
        ObjectType = type;
        WorldPos = worldPos;
        CellPos = cellPos;
        FlipX = flipX;
        FlipY = flipY;
        IsRespawn = isRespawn;
    }
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
        if (File.Exists(Path))
        {
            Debug.Log("이미 플레이어한 데이터가 있습니다. 게임을 새로 시작하겠습니까");
            // return;
        }

        // Player
        var player = Managers.Data.PlayerDataDic.Values;
        foreach (Data.PlayerData p in player)
            GameData.Player = p;

        // 세이브 포인트
        GameData.CurrentSavePoint.SceneType = EScene.None;

        // Map
        GameData.Maps.Clear();

        // Accessory
    }

    /// <summary>
    /// 게임 데이터 저장
    /// </summary>
    public void Save()
    {
        // Player
        if (Player != null)
        {
            var playerInfo = GameData.Player;
            #region 플레이어 정보
            playerInfo.DataId = PLAYER_ID;
            playerInfo.Name = Player.Name;
            playerInfo.Hp = Player.Hp;
            playerInfo.MaxHp = Player.MaxHp;
            playerInfo.HpLevel = Player.HpLevel;
            playerInfo.Mp = Player.Mp;
            playerInfo.MaxMp = Player.MaxMp;
            playerInfo.MpLevel = Player.MpLevel;
            playerInfo.Atk = Player.Atk;
            playerInfo.AtkLevel = Player.AtkLevel;
            playerInfo.Speed = Player.MoveSpeed;
            playerInfo.AccessorySlot = Player.AccessorySlot;

            playerInfo.SkillIdList = new List<int>();
            var skillList = Player.Skills;

            foreach (var skill in skillList)
                playerInfo.SkillIdList.Add(skill.DataId);
            #endregion
            
            GameData.Player = playerInfo;
        }

        // 세이브 포인트는 플레이어가 활성화 할 때만 저장되므로, Save에서 작성할 필요가 없다 

        // Map
        string name = Managers.Map.MapName;
        if (name != null)
        {
            for (int i = 0; i < GameData.Maps.Count; i++)
            {
                if (GameData.Maps[i].MapName == name)
                {
                    GameData.Maps.RemoveAt(i);
                    break;
                }
            }

            List<MapObjectInfo> infos = new List<MapObjectInfo>(Managers.Map.ObjectInfos.Values);
            MapData mapData = new MapData() { MapName = name, ObjectInfos = infos };
            GameData.Maps.Add(mapData);
        }

        // Accessory, TODO

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

        Debug.Log("게임 데이터 불러오기 성공");
        GameData = data;
        return true;
    }
}
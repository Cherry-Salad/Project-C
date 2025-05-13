using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;

public class MapManager
{
    public GameObject Map { get; private set; }
    public string MapName { get; private set; }
    public Grid CellGrid { get; private set; }

    /// <summary>
    /// 찾으려는 타일맵의 이름은 반드시 SpawnObject로 설정할 것
    /// </summary>
    public Tilemap SpawnObject { get; private set; }

    /// <summary>
    /// Room을 찾기 위해 부모 오브젝트의 이름은 반드시 @Rooms로 설정할 것
    /// </summary>
    public HashSet<Room> Rooms { get; private set; } = new HashSet<Room>();

    public HashSet<Vector3> Checkpoints = new HashSet<Vector3>();
    public HashSet<SavePoint> SavePoints = new HashSet<SavePoint>();

    public Dictionary<GameObject, MapObjectInfo> ObjectInfos = new Dictionary<GameObject, MapObjectInfo>();

    Room _room = null;
    public Room CurrentRoom 
    {
        get { return _room; }
        set
        {
            if (_room != value)
            {
                _room = value;
                Managers.Camera.SetCurrentCamera(_room.CameraBoundary);
            }
        }
    }

    /// <summary>
    /// 현재 활성화된 체크포인트 위치
    /// </summary>
    public Vector3 CurrentCheckpoint { get; set; }

    public void LoadMap(string mapName)
    {
        DestroyMap();

        GameObject map = GameObject.Find(mapName);
        if (map == null)
            map = Managers.Resource.Instantiate(mapName);
        
        map.transform.position = Vector3.zero;

        Map = map;
        MapName = mapName;
        CellGrid = map.GetComponent<Grid>();
        SpawnObject = Util.FindChild<Tilemap>(map, "SpawnObject");

        Transform rooms = Util.FindChild<Transform>(map, "@Rooms");
        foreach (Transform child in rooms)
        {
            if (child.TryGetComponent<Room>(out var room) == false)
                continue;

            Rooms.Add(room);
            room.GetComponent<TilemapRenderer>().enabled = false;
        }

        CurrentCheckpoint = Vector3.zero;   // 체크포인트 초기화
        
        SpawnObjects();
    }

    public void DestroyMap()
    {
        if (Map != null)
            Managers.Resource.Destroy(Map);

        MapName = null;
        
        Rooms.Clear();
        Checkpoints.Clear();
        SavePoints.Clear();

        ObjectInfos.Clear();

        Managers.Camera.Clear();
    }

    void SpawnObjects()
    {
        // 저장된 맵 데이터를 통해서 스폰
        if (SpawnObjectsFromData())
            return;

        if (SpawnObject == null)
            return;

        Debug.Log("SpawnObjects");

        // Init
        for (int y = SpawnObject.cellBounds.yMax; y >= SpawnObject.cellBounds.yMin; y--)
        {
            for (int x = SpawnObject.cellBounds.xMin; x <= SpawnObject.cellBounds.xMax; x++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                MapObjectTile tile = SpawnObject.GetTile(cellPos) as MapObjectTile;

                if (tile != null)
                {
                    // 타일 크기를 고려하여 타일 중심으로 조정
                    Vector3 worldPos = CellGrid.CellToWorld(cellPos);
                    Vector3 tileOffset = CellGrid.cellSize * 0.5f;  // 타일 크기의 절반
                    worldPos += tileOffset;

                    // 맵 오브젝트 정보 저장
                    MapObjectInfo info = new MapObjectInfo()
                    {
                        DataId = tile.DataId,
                        Name = tile.Name,
                        ObjectType = tile.ObjectType,
                        WorldPos = worldPos,
                        CellPos = cellPos,
                        FlipX = tile.FlipX,
                        FlipY = tile.FlipY,
                        IsRespawn = tile.IsRespawn,
                    };

                    GameObject obj = Managers.Resource.Instantiate(tile.Name, pooling: true);   // 소환
                    StoreObject(obj, info);
                }
            }
        }

        SpawnObject.GetComponent<TilemapRenderer>().enabled = false;
    }

    /// <summary>
    /// 저장된 맵 데이터를 통해서 오브젝트를 소환한다.
    /// </summary>
    /// <returns></returns>
    bool SpawnObjectsFromData()
    {
        MapData data = null;
        foreach (MapData map in Managers.Game.GameData.Maps)
        {
            if (MapName == map.MapName)
            {
                data = map;
                break;
            }
        }

        if (data == null)
        {
            Debug.Log("아직 이 맵에 대한 저장된 정보가 없다.");
            return false;
        }

        Debug.Log("SpawnObjectsFromData");

        if (SpawnObject != null)
            SpawnObject.GetComponent<TilemapRenderer>().enabled = false;

        foreach (MapObjectInfo info in data.ObjectInfos)
        {
            GameObject obj = Managers.Resource.Instantiate(info.Name, pooling: true);
            StoreObject(obj, info);
        }

        return true;
    }

    void StoreObject(GameObject obj, MapObjectInfo info)
    {
        ObjectInfos.Add(obj, info);

        switch (info.ObjectType)
        {
            case EObjectType.StartPoint:
                SavePoint startPoint = obj.GetComponent<SavePoint>();
                startPoint.SetInfo(info.WorldPos, Managers.Scene.CurrentScene.SceneType);
                // 활성화된 세이브 포인트가 아무것도 없다면, 시작 포인트를 세이브 포인트로 활성화
                if (Managers.Game.GameData.CurrentSavePoint.SceneType == EScene.None)
                {
                    Debug.Log("시작 포인트 활성화");
                    Managers.Game.GameData.CurrentSavePoint.SceneType = startPoint.SceneType;
                    Managers.Game.GameData.CurrentSavePoint.Position = startPoint.transform.position;
                }
                SavePoints.Add(startPoint);
                break;

            case EObjectType.Checkpoint:
                obj.transform.position = info.WorldPos;
                Checkpoints.Add(info.WorldPos);
                break;

            case EObjectType.SavePoint:
                SavePoint savePoint = obj.GetComponent<SavePoint>();
                savePoint.SetInfo(info.WorldPos, Managers.Scene.CurrentScene.SceneType);
                SavePoints.Add(savePoint);
                break;

            case EObjectType.Env:
                Env env = obj.GetComponent<Env>();
                env.SetInfo(info.DataId, info.WorldPos, info.FlipX, info.FlipY);
                break;

            case EObjectType.Monster:
                obj.transform.position = info.WorldPos;
                break;

            case EObjectType.Npc:
                obj.transform.position = info.WorldPos;
                break;
        }
    }

    /// <summary>
    /// IsRespawn이 false인 맵 오브젝트가 한 번이라도 파괴(죽음)된 경우 재스폰되지 않는다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    public void DespawnObject<T>(T obj) where T : BaseObject
    {
        if (ObjectInfos.ContainsKey(obj.gameObject) == false)
            return;

        var info = ObjectInfos[obj.gameObject];

        // IsRespawn이 true라면 다시 스폰해야 하므로 지우면 안된다
        if (info.IsRespawn == false)
        {
            Debug.Log("DespawnObject");
            ObjectInfos.Remove(obj.gameObject);
        }
    }

    public void ChangeCurrentRoom(Vector3 pos, bool DisableCameraBlend = false)
    {
        foreach (Room room in Rooms)
        {
            if (room.Tilemap == null) 
                continue;

            Vector3Int cellPos = CellGrid.WorldToCell(pos);
            if (room.Tilemap.HasTile(cellPos))
            {
                CurrentRoom = room;
                break;
            }
        }
    }

    /// <summary>
    /// 플레이어를 체크포인트로 리스폰한다.
    /// </summary>
    public void RespawnAtCheckpoint(GameObject go)
    {
        if (Checkpoints.Count <= 0)
            return;

        // 활성화된 체크포인트가 없다면 가장 가까운 체크포인트로 이동
        if (CurrentCheckpoint == Vector3.zero)
        {
            Vector3 closestCheckpoint = Vector3.zero; // 가장 가까운 체크포인트 위치
            float minDistance = float.MaxValue;

            foreach (Vector3 pos in Checkpoints)
            {
                float distance = Vector3.Distance(go.transform.position, pos);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCheckpoint = pos;
                }
            }

            if (closestCheckpoint == Vector3.zero)    // 가까운 체크포인트를 못 찾았다면 버그다
            {
                Debug.LogError("와 샌즈");
                return;
            }

            CurrentCheckpoint = closestCheckpoint;
        }

        // For 최혁도, TODO: 체크포인트로 이동하는 연출 
        // From 최혁도 : 음... 뭐가 있을까?
        go.transform.position = CurrentCheckpoint;  // 플레이어를 체크 포인트로 이동
    }
}

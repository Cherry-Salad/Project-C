using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;

[CreateAssetMenu]
public class MapObject : Tile
{
    public int DataId;
    public string Name;
    public EObjectType ObjectType;
    public bool FlipX = false;
    public bool FlipY = false;
    public bool IsRespawn = false;  // 파괴되면 더이상 스폰 안 한다.
}

public class MapManager
{
    public GameObject Map { get; private set; }
    public string MapName { get; private set; }
    public Grid CellGrid { get; private set; }
    public Tilemap SpawnObject { get; private set; }

    public HashSet<Room> Rooms { get; private set; } = new HashSet<Room>();
    public HashSet<Vector3> Checkpoints = new HashSet<Vector3>();
    public HashSet<SavePoint> SavePoints = new HashSet<SavePoint>();

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

    /// <summary>
    /// 현재 활성화된 세이브 포인트
    /// </summary>
    public SavePoint CurrentSavePoint { get; set; } = null;

    public void LoadMap(string mapName)
    {
        DestroyMap();

        //GameObject map = Managers.Resource.Instantiate(mapName);
        GameObject map = GameObject.Find(mapName);
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

        Checkpoints.Clear();
        Managers.Camera.Clear();
    }

    void SpawnObjects()
    {
        if (SpawnObject == null)
            return;

        for (int y = SpawnObject.cellBounds.yMax; y >= SpawnObject.cellBounds.yMin; y--)
        {
            for (int x = SpawnObject.cellBounds.xMin; x <= SpawnObject.cellBounds.xMax; x++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                MapObject tile = SpawnObject.GetTile(cellPos) as MapObject;

                if (tile != null)
                {
                    // 타일 크기를 고려하여 타일 중심으로 조정
                    Vector3 worldPos = CellGrid.CellToWorld(cellPos);
                    Vector3 tileOffset = CellGrid.cellSize * 0.5f;  // 타일 크기의 절반
                    worldPos += tileOffset;

                    GameObject obj = Managers.Resource.Instantiate(tile.Name, pooling: true);

                    switch (tile.ObjectType)
                    {
                        case EObjectType.StartPoint:
                            SavePoint startPoint = obj.GetComponent<SavePoint>();
                            startPoint.SetInfo(worldPos, Managers.Scene.CurrentScene.SceneType);
                            if (Managers.Game.GameData.CurrentSavePoint.SceneType == EScene.None)
                                CurrentSavePoint = startPoint;
                            SavePoints.Add(startPoint);
                            break;
                        case EObjectType.Checkpoint:
                            obj.transform.position = worldPos;
                            Checkpoints.Add(worldPos);
                            break;
                        case EObjectType.SavePoint:
                            SavePoint savePoint = obj.GetComponent<SavePoint>();
                            savePoint.SetInfo(worldPos, Managers.Scene.CurrentScene.SceneType);
                            SavePoints.Add(savePoint);
                            break;
                        case EObjectType.Env:
                            Env env = obj.GetComponent<Env>();
                            env.SetInfo(tile.DataId, worldPos, tile.FlipX, tile.FlipY);
                            break;
                    }
                }
            }
        }

        SpawnObject.GetComponent<TilemapRenderer>().enabled = false;
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
            Vector3 closest = Vector3.zero; // 가장 가까운 체크포인트 위치
            float minDistance = float.MaxValue;

            foreach (Vector3 pos in Checkpoints)
            {
                float distance = Vector3.Distance(go.transform.position, pos);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = pos;
                }
            }

            if (closest == Vector3.zero)    // 가까운 체크포인트를 못 찾았다면 버그다
            {
                Debug.LogError("와 샌즈");
                return;
            }

            CurrentCheckpoint = closest;
        }

        // TODO: 체크포인트로 이동하는 연출
        go.transform.position = CurrentCheckpoint;  // 플레이어를 체크 포인트로 이동
    }

    public void RespawnAtSavePoint(GameObject go)
    {
        if (CurrentSavePoint == null)
        {
            Debug.LogError("와 파피루스");
            return;
        }

        // TODO: 활성화된 세이브 포인트가 현재 씬과 다르면 씬 이동
        Managers.Scene.LoadScene(CurrentSavePoint.SceneType);

        go.transform.position = CurrentSavePoint.transform.position;
    }
}

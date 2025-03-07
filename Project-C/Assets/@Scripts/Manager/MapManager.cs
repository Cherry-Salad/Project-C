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
}

public class MapManager
{
    public GameObject Map { get; private set; }
    public string MapName { get; private set; }
    public Grid CellGrid { get; private set; }
    public Tilemap SpawnObject { get; private set; }
    public HashSet<Room> Rooms { get; private set; } = new HashSet<Room>();

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

    public Transform CheckpointRoot
    {
        get
        {
            GameObject root = GameObject.Find("@Checkpoints");
            if (root == null)
                root = new GameObject { name = "@Checkpoints" };

            return root.transform;
        }
    }

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

        CurrentCheckpoint = Vector3.zero;
        SpawnObjects();
    }

    public void DestroyMap()
    {
        if (Map != null)
            Managers.Resource.Destroy(Map);

        Managers.Camera.Clear();
    }

    void SpawnObjects() // TODO: 오브젝트 풀링 사용
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

                    GameObject obj = Managers.Resource.Instantiate(tile.name);
                    obj.transform.position = worldPos;

                    switch (tile.ObjectType)
                    {
                        case EObjectType.Checkpoint:
                            obj.transform.parent = CheckpointRoot;
                            break;
                    }
                }
            }
        }

        SpawnObject.GetComponent<TilemapRenderer>().enabled = false;
    }

    public void ChangeCurrentRoom(Vector3 pos)
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
    public void RespawnAtCheckpoint(BaseObject go)
    {
        if (CheckpointRoot == null || CheckpointRoot.childCount <= 0)
            return;

        // 활성화된 체크포인트가 없다면 가장 가까운 체크포인트로 이동
        if (CurrentCheckpoint == Vector3.zero)
        {
            Vector3 closest = Vector3.zero; // 가장 가까운 체크포인트 위치
            float minDistance = float.MaxValue;

            foreach (Transform checkpoint in CheckpointRoot)
            {
                float distance = Vector3.Distance(go.transform.position, checkpoint.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = checkpoint.position;
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

        // 플레이어를 체크 포인트로 이동
        go.transform.position = CurrentCheckpoint;
    }
}

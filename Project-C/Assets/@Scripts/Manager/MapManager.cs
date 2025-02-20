using System.Collections;
using System.Collections.Generic;
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
    public Transform CameraBoundaryRoot
    {
        get
        {
            GameObject root = GameObject.Find("@CameraBoundarys");
            if (root == null)
                root = new GameObject { name = "@CameraBoundarys" };

            return root.transform;
        }
    }

    public void LoadMap(string mapName)
    {
        DestroyMap();

        GameObject map = Managers.Resource.Instantiate(mapName);
        map.transform.position = Vector3.zero;
        map.name = $"@Map_{mapName}";

        Map = map;
        MapName = mapName;
        CellGrid = map.GetComponent<Grid>();
        SpawnObject = Util.FindChild<Tilemap>(map, "SpawnObject");
        
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

    /// <summary>
    /// 룸 기반으로 카메라를 배치하고 각 룸에 설정된 카메라 경계를 가져온다.
    /// </summary>
    public void SpawnRoomCameras()  // 사실 동적으로 생성하는 것보다 맵 프리팹에서 직접 생성하는게 더 효율적일지도..
    {
        GameObject p = GameObject.Find("Player");  // TODO: 게임 매니저에서 플레이어를 찾는다
        Player player = p.GetComponent<Player>();

        foreach (Transform boundary in CameraBoundaryRoot)
        {
            if (boundary.TryGetComponent<PolygonCollider2D>(out var collider) == false)
                continue;

            var ca = Managers.Camera.Spawn(player, collider);
            ca.name = boundary.name;
        }
    }

    /// <summary>
    /// 플레이어를 체크포인트로 리스폰한다.
    /// </summary>
    public void RespawnAtCheckpoint(BaseObject go)
    {
        if (SpawnObject == null || CurrentCheckpoint == Vector3.zero)
            return;

        // TODO: 체크포인트로 이동하는 연출

        // 플레이어를 체크 포인트로 이동
        go.transform.position = CurrentCheckpoint;
    }
}

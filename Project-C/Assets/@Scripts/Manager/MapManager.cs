using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;

public struct ObjectSpawnInfo
{
    public ObjectSpawnInfo(string name, Vector3Int cellPos, Vector3 worldPos, EObjectType type)
    {
        Name = name;
        CellPos = cellPos;
        WorldPos = worldPos;
        ObjectType = type;
    }

    public string Name;
    public Vector3Int CellPos;
    public Vector3 WorldPos;
    public EObjectType ObjectType;
}

public class MapManager
{
    public GameObject Map { get; private set; }
    public string MapName { get; private set; }
    public Grid CellGrid { get; private set; }
    public Tilemap Checkpoint { get; private set; }

    public List<ObjectSpawnInfo> Checkpoints = new List<ObjectSpawnInfo>(); // TODO: 맵에 소환된 오브젝트들
    
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
        Checkpoint = Util.FindChild<Tilemap>(map, "Checkpoint");

        CurrentCheckpoint = Vector3.zero;
        SpawnCheckpoints();
    }

    public void DestroyMap()
    {
        if (Map != null)
            Managers.Resource.Destroy(Map);

        Managers.Camera.Clear();
        // TODO: 다른 상호작용 오브젝트 디스폰, 오브젝트 풀링 사용
        //foreach (Transform checkpoint in CheckpointRoot)
        //    Managers.Resource.Destroy(checkpoint.gameObject);
    }

    void SpawnCheckpoints() // TODO: 체크 포인트 말고 다른 상호작용 오브젝트도 소환, 오브젝트 풀링 사용
    {
        if (Checkpoint == null)
            return;

        for (int y = Checkpoint.cellBounds.yMax; y >= Checkpoint.cellBounds.yMin; y--)
        {
            for (int x = Checkpoint.cellBounds.xMin; x <= Checkpoint.cellBounds.xMax; x++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                if (Checkpoint.HasTile(cellPos))
                {
                    // 타일 크기를 고려하여 타일 중심으로 조정
                    Vector3 worldPos = CellGrid.CellToWorld(cellPos);
                    Vector3 tileOffset = CellGrid.cellSize * 0.5f;  // 타일 크기의 절반
                    worldPos += tileOffset;

                    GameObject checkpoint = Managers.Resource.Instantiate("Checkpoint");
                    checkpoint.transform.position = worldPos;
                    checkpoint.transform.parent = CheckpointRoot;

                    ObjectSpawnInfo info = new ObjectSpawnInfo("Checkpoint", cellPos, worldPos, EObjectType.Checkpoint);
                    Checkpoints.Add(info);
                }
            }
        }
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
        if (Checkpoint == null || CurrentCheckpoint == Vector3.zero)
            return;

        // TODO: 체크포인트로 이동하는 연출

        // 플레이어를 체크 포인트로 이동
        go.transform.position = CurrentCheckpoint;
    }
}

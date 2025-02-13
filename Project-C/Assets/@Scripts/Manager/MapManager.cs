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
    public PolygonCollider2D CameraBounds { get; private set; } // 카메라의 이동 범위 제한

    public List<ObjectSpawnInfo> Checkpoints = new List<ObjectSpawnInfo>();
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

    public Vector3 CurrentCheckpoint { get; set; }

    // 맵 경계
    //public Vector2 MinBound { get; private set; }
    //public Vector2 MaxBound { get; private set; }

    public void LoadMap(string mapName)
    {
        DestroyMap();

        GameObject map = Managers.Resource.Instantiate(mapName);
        map.transform.position = Vector3.zero;
        map.name = $"@Map_{mapName}";

        Map = map;
        MapName = mapName;
        CellGrid = map.GetComponent<Grid>();
        CameraBounds = Util.FindChild<PolygonCollider2D>(map, "CameraBounds");
        Checkpoint = Util.FindChild<Tilemap>(map, "Checkpoint");
        CurrentCheckpoint = Vector3.zero;

        SpawnCheckpoints();

        // 카메라 위치를 월드 경계로 제한
        //CameraController camera = Camera.main.GetComponent<CameraController>();
        //camera.Confiner.m_BoundingShape2D = CameraBounds;

        // 모든 타일맵의 경계 계산
        //CalculateMapBounds();
    }

    public void DestroyMap()
    {
        if (Map != null)
            Managers.Resource.Destroy(Map);

        // TODO: 다른 상호작용 오브젝트 디스폰, 오브젝트 풀링 사용
        foreach (Transform checkpoint in CheckpointRoot)
            Managers.Resource.Destroy(checkpoint.gameObject);
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

    public void RespawnAtCheckpoint()
    {
        if (Checkpoint == null)
            return;

        if (CurrentCheckpoint == Vector3.zero)
            return;
        
        // TODO: 체크포인트로 이동하는 연출
        
        GameObject go = GameObject.Find("Player");  // TODO: 게임 매니저에서 플레이어를 찾는다
        Player player = go.GetComponent<Player>();

        // 플레이어를 체크 포인트로 이동
        player.transform.position = CurrentCheckpoint;
    }

    /// <summary>
    /// 타일맵의 경계를 계산한다.
    /// </summary>
    //void CalculateMapBounds()
    //{
    //    // 배경 타일맵을 찾는다
    //    Tilemap backgroundTilemap = null;
    //    foreach (Tilemap tilemap in Map.GetComponentsInChildren<Tilemap>())
    //    {
    //        if (tilemap.name.Contains("Background"))
    //        {
    //            backgroundTilemap = tilemap;
    //            break;
    //        }
    //    }

    //    if (backgroundTilemap == null)
    //        return;

    //    // 배경 타일맵의 셀 경계를 월드 좌표로 변환
    //    BoundsInt cellBounds = backgroundTilemap.cellBounds;
    //    Vector3 worldMin = backgroundTilemap.CellToWorld(cellBounds.min);   // 제일 왼쪽 아래
    //    Vector3 worldMax = backgroundTilemap.CellToWorld(cellBounds.max);  // 제일 오른쪽 위

    //    // 맵 경계 설정
    //    MinBound = new Vector2(worldMin.x, worldMin.y);
    //    MaxBound = new Vector2(worldMax.x, worldMax.y);
        
    //    Debug.Log($"Map Bounds - Min: {MinBound}, Max: {MaxBound}");
    //}
}

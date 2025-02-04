using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager
{
    public GameObject Map { get; private set; }
    public string MapName { get; private set; }
    public Grid CellGrid { get; private set; }

    // 맵 경계
    public Vector2 MinBound { get; private set; }
    public Vector2 MaxBound { get; private set; }

    public void LoadMap(string mapName)
    {
        DestroyMap();

        GameObject map = Managers.Resource.Instantiate(mapName);
        map.transform.position = Vector3.zero;
        map.name = $"@Map_{mapName}";

        Map = map;
        MapName = mapName;
        CellGrid = map.GetComponent<Grid>();

        // 모든 타일맵의 경계 계산
        CalculateMapBounds();
    }

    public void DestroyMap()
    {
        if (Map != null)
            Managers.Resource.Destroy(Map);
    }

    /// <summary>
    /// 모든 타일맵의 경계를 계산한다.
    /// </summary>
    void CalculateMapBounds()
    {
        // 배경 타일맵을 찾는다
        Tilemap backgroundTilemap = null;
        foreach (Tilemap tilemap in Map.GetComponentsInChildren<Tilemap>())
        {
            if (tilemap.name.Contains("Background"))
            {
                backgroundTilemap = tilemap;
                break;
            }
        }

        if (backgroundTilemap == null)
            return;

        // 배경 타일맵의 셀 경계를 월드 좌표로 변환
        BoundsInt cellBounds = backgroundTilemap.cellBounds;
        Vector3 worldMin = backgroundTilemap.CellToWorld(cellBounds.min);   // 제일 왼쪽 아래
        Vector3 worldMax = backgroundTilemap.CellToWorld(cellBounds.max);  // 제일 오른쪽 위

        // 맵 경계 설정
        MinBound = new Vector2(worldMin.x, worldMin.y);
        MaxBound = new Vector2(worldMax.x, worldMax.y);
        
        Debug.Log($"Map Bounds - Min: {MinBound}, Max: {MaxBound}");
    }
}

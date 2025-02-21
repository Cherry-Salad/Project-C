using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : InitBase
{
    public Tilemap Tilemap { get; private set; }
    public PolygonCollider2D CameraBoundary;    // 인스펙터에서 직접 설정

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Tilemap = GetComponent<Tilemap>();
        return true;
    }

    public bool IsInRoom(Vector3 pos)
    {
        Vector3Int cellPos = Tilemap.layoutGrid.WorldToCell(pos);
        return Tilemap.HasTile(cellPos);
    }
}

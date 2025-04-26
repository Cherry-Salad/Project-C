using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;

[CreateAssetMenu]
public class MapObjectTile : Tile
{
    public int DataId;
    public string Name;
    public EObjectType ObjectType;
    public bool FlipX = false;
    public bool FlipY = false;
    public bool IsRespawn = false;  // 파괴되면 더이상 스폰 안 한다.
}

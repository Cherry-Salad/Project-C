using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;

public class FakeWall : InitBase
{
    TilemapRenderer _renderer;
    CompositeCollider2D _collider;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _renderer = GetComponent<TilemapRenderer>();
        _collider = GetComponent<CompositeCollider2D>();

        _collider.isTrigger = true;

        // 트리거 필터링
        LayerMask includeLayers = 0;
        includeLayers.AddLayer(ELayer.Player);
        _collider.includeLayers = includeLayers;

        return true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        //if (collision.CompareTag("Player"))
        //{
        //    Debug.Log($"가짜벽 비활성화 {collision.name}");
        //    _renderer.enabled = false;
        //}

        if (collision.GetComponent<Player>() != null)
        {
            Debug.Log($"가짜벽 비활성화 {collision.name}");
            _renderer.enabled = false;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        //if (collision.CompareTag("Player"))
        //{
        //    Debug.Log($"가짜벽 활성화 {collision.name}");
        //    _renderer.enabled = true;
        //}

        if (collision.GetComponent<Player>() != null)
        {
            Debug.Log($"가짜벽 활성화 {collision.name}");
            _renderer.enabled = true;
        }
    }
}

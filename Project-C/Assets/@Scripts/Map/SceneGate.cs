using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SceneGate : InitBase
{
    public EScene TransitionSceneType = EScene.None;    // 인스펙터 창에서 직접 설정
    
    BoxCollider2D _collider;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _collider = GetComponent<BoxCollider2D>();
        _collider.isTrigger = true;

        // 트리거 필터링
        LayerMask includeLayers = 0;
        includeLayers.AddLayer(ELayer.Player);
        _collider.includeLayers = includeLayers;

        return true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"{TransitionSceneType.ToString()} 전환");
        Managers.Game.Save();
        // For 최혁도, TODO: 씬 전환하는 연출 필요, (ex: 플레이어가 걸어가는 모습, 까맣게 암전되는 모습 등)
        Managers.Scene.LoadScene(TransitionSceneType);
    }
}

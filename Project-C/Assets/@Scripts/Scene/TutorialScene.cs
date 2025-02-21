using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScene : BaseScene
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        SceneType = Define.EScene.TutorialScene;

        // Test, TODO: 메인 화면에서 PreLoad 어드레서블을 모두 불러온다
        #region PreLoad 어드레서블 모두 로드
        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, loadCount, totalCount) =>
        {
            // 모두 로드
            if (loadCount == totalCount)
            {
                Managers.Data.Init();
                Managers.Map.LoadMap("TutorialMap");

                // 플레이어 소환, TODO: 맵마다 플레이어 스폰 위치를 다르게 설정
                GameObject player = Managers.Resource.Instantiate("Player");
                player.transform.position = Vector3.zero;   // TODO: 맵에서 플레이어 소환 위치 설정

                Managers.Camera.Load();
            }
        });
        #endregion

        return true;
    }

    public override void Clear()
    {
        base.Clear();
    }
}

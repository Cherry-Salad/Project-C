using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VillageScene : BaseScene
{    
    private GameObject player;

    private Vector3 _startPos = new Vector3(-9.2f, -2.5f, 0);
    

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        SceneType = Define.EScene.VillageScene;

        // Test, TODO: 메인 화면에서 PreLoad 어드레서블을 모두 불러온다
        #region PreLoad 어드레서블 모두 로드
        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, loadCount, totalCount) =>
        {
            // 모두 로드
            if (loadCount == totalCount)
            {
                Managers.Data.Init();

                if (Managers.Game.Load() == false)
                {
                    Managers.Game.Init();
                    Managers.Game.Save();
                }

                // 플레이어 소환, TODO: 맵마다 플레이어 스폰 위치를 다르게 설정
                player = Managers.Resource.Instantiate("Player");
                Managers.Game.Player = player.GetComponent<Player>();
                player.transform.position = _startPos;   // TODO: 맵에서 플레이어 소환 위치 설정                
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

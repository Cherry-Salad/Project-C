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

        SceneType = Define.EScene.TutorialScene;
        
        Managers.Map.LoadMap("VillageMap");

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

                player = Managers.Resource.Instantiate("Player");
                player.transform.position = _startPos;             

                Managers.Game.Player = player.GetComponent<Player>();
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

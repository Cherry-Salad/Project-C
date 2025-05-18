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

                Managers.Map.LoadMap("VillageMap");
                
                player = Managers.Resource.Instantiate("Player");
                Managers.Game.Player = player.GetComponent<Player>();
                
                Managers.Camera.Load();

                // 활성화된 세이브 포인트 업데이트 및 저장
                SavePoint sv = GameObject.Find("SavePoint").GetComponent<SavePoint>();
                if (sv != null)
                {
                    Managers.Game.GameData.CurrentSavePoint.SceneType = sv.SceneType;
                    Managers.Game.GameData.CurrentSavePoint.Position = sv.transform.position;
                    
                    _startPos = sv.transform.position;
                    player.transform.position = _startPos;

                    Managers.Game.Save();
                }
            }
        });


        AudioManager.Instance.StartBGM(AudioManager.Instance.VlilageBackGround);
        #endregion

        return true;
    }

    public override void Clear()
    {
        base.Clear();
    }

}

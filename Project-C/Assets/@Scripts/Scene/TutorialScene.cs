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
                
                if (Managers.Game.Load() == false)
                {
                    Managers.Game.Init();
                    Managers.Game.Save();
                }
                
                Managers.Map.LoadMap("TutorialMap");

                // 활성화된 세이브 포인트 찾기
                if (Managers.Game.GameData.CurrentSavePoint.SceneType != Define.EScene.None)
                {
                    foreach (var sp in Managers.Map.SavePoints)
                    {
                        if (Managers.Game.GameData.CurrentSavePoint.Position == sp.transform.position)
                        {
                            Managers.Map.CurrentSavePoint = sp;
                            break;
                        }
                    }
                }

                // Test, 플레이어 소환
                GameObject player = Managers.Resource.Instantiate("Player");
                Managers.Game.Player = player.GetComponent<Player>();
                player.transform.position = Managers.Map.CurrentSavePoint.transform.position;   // Test, 플레이어 위치 설정
                // TODO: 플레이어가 지상에 있는데도 점프 상태로 시작하는 버그 수정 필요

                Managers.Camera.Load();
            }
        });
        #endregion

        return true;
    }

    public override void Clear()
    {
        base.Clear();
        // TODO: 맵 상태 저장
    }
}

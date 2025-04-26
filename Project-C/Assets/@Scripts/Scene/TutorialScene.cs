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

        if (Managers.Game.Load() == false)
        {
            // Test: 저장한 데이터가 없다면 새로 시작
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
        player.transform.position = Managers.Game.GameData.CurrentSavePoint.Position;   // 플레이어 위치 설정

        Managers.Camera.Load();
        
        return true;
    }

    public override void Clear()
    {
        base.Clear();
        // TODO: 맵 상태 저장
    }
}

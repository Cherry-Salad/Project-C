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

        Managers.Map.LoadMap("TutorialMap");    // 맵 정보 로드, 세이브 포인트 활성화

        GameObject player = Managers.Resource.Instantiate("Player");
        Managers.Game.Player = player.GetComponent<Player>();

        // 활성화된 세이브 포인트 찾기
        if (SceneType == Managers.Game.GameData.CurrentSavePoint.SceneType)
        {
            foreach (var sp in Managers.Map.SavePoints) // 정보가 일치하는지 확인
            {
                if (sp.transform.position == Managers.Game.GameData.CurrentSavePoint.Position)
                {
                    // 플레이어 위치 설정
                    player.transform.position = sp.transform.position;
                    break;
                }
            }
        }
        
        Managers.Camera.Load();
        
        return true;
    }

    public override void Clear()
    {
        base.Clear();
    }
}

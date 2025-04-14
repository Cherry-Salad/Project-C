using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScene : BaseScene
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        SceneType = Define.EScene.MainScene;

        #region PreLoad 어드레서블 모두 로드
        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, loadCount, totalCount) =>
        {
            // 모두 로드
            if (loadCount == totalCount)
            {
                Debug.Log("에셋 모두 로드 완료!");

                Managers.Data.Init();

                // Test: 새로 시작 테스트용
                Managers.Game.Init();
                Managers.Game.Save();
            }
        });
        #endregion

        return true;
    }
}

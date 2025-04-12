using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class BossMonsterScene : BaseScene
{
    [SerializeField] private GameObject _battleRoom;
    [SerializeField] private GameObject _bossMonster;
    [SerializeField] private GameObject _bossHpBar;

    private GameObject player;

    private Slider _hpBar;
    private BossMonsterBase _bossMonsterCode;

    private bool _isFight = false;
    private Vector3 _startPos = new Vector3(-9.2f, -2.5f, 0);
    private const float _FIGHT_POS_X = -6;
    private const float _EXIT_POS_X = 9.5f;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        SceneType = Define.EScene.BossMonsterScene;

        _battleRoom.SetActive(false);
        _bossHpBar.SetActive(false);

        _bossMonsterCode = _bossMonster.GetComponent<BossMonsterBase>();
        _hpBar = _bossHpBar.GetComponent<Slider>();
        _isFight = false;

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

    private void Update()
    {
        if(_bossMonster == null)
        {
            if (_isFight && _battleRoom.activeSelf == true)
            {
                OffBossFight();
            }

            if(!_isFight && player.transform.position.x >= _EXIT_POS_X)
                SceneManager.LoadScene("VillageScene");

        }
        else if(player != null)
        {
            if (!_isFight && _bossMonster.activeSelf == true && player.transform.position.x >= _FIGHT_POS_X)
            {
                OnBossFight();
            }
        }

        if(_isFight && _bossHpBar.activeSelf == true)
        {
            _hpBar.value = _bossMonsterCode.HpPercent();
        }
    }

    public void OnBossFight()
    {
        _isFight = true;
        _bossMonsterCode.AwakeMonster();

        _battleRoom?.SetActive(true);
        _bossHpBar?.SetActive(true);        
    }

    public void OffBossFight()
    {
        _isFight = false;
        _battleRoom?.SetActive(false);
        _bossHpBar?.SetActive(false);
    }
}

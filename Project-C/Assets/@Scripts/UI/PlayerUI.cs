using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Player player;

    //HP : 현재 체력
    //MaxHp : 최대 체력(최소 3)
    //HpLevel : 레벨에 따라 최대체력 증가(레벨 1당 1씩 MaxHp 증가

    public Sprite heartSprite;

    public Image[] totalHp;
    public Image[] totalMp;

    private Color fullHpColor = new Color(1f, 0f, 0f);        // 빨강 (Full HP) - #FF0000
    private Color emptyHpColor = new Color(0.62f, 0.38f, 0.38f); // 연한 빨강 (Empty HP) - #9F6262
    private Color fullMpColor = new Color(0f, 0.28f, 1f);     // 파랑 (Full MP) - #0047FF
    private Color emptyMpColor = new Color(0.43f, 0.5f, 0.58f); // 회색 파랑 (Empty MP) - #6E7F95


    private void Start()
    {
        StartCoroutine(WaitForPlayerDataLoad());
    }

    //`Player` 데이터가 로드될 때까지 대기 후 UI 초기화
    private IEnumerator WaitForPlayerDataLoad()
    {
        //Player 찾기
        while (player == null)
        {
            player = GameObject.FindWithTag("Player")?.GetComponent<Player>(); //이거 왜 Tag로 못찾지..??
            if (player == null)
            {
                player = FindObjectOfType<Player>();
            }
            yield return null;
        }
        Debug.Log($"PlayerUI - Player 찾음: {player.gameObject.name}");

        //Player.cs의 OnDataLoaded 이벤트가 실행될 때까지 대기
        yield return new WaitUntil(() => player != null && player.Hp > 0 && player.MaxHp > 0);

        Debug.Log("Player 데이터 로드 완료! UI 초기화 시작...");

            //UI 초기화
        UpdateUI(totalHp, player.Hp, player.MaxHp, fullHpColor, emptyHpColor, "Hp");
        UpdateUI(totalMp, player.Mp, player.MaxMp, fullMpColor, emptyMpColor, "Mp");

            //이벤트 구독
        player.OnHpChanged += UpdateHpUI;
        player.OnMpChanged += UpdateMpUI;
    }

    //HP 변경 시 UI 업데이트
    private void UpdateHpUI()
    {
        UpdateUI(totalHp, player.Hp, player.MaxHp, fullHpColor, emptyHpColor, "Hp");
    }

    //MP 변경 시 UI 업데이트
    private void UpdateMpUI()
    {
        UpdateUI(totalMp, player.Mp, player.MaxMp, fullMpColor, emptyMpColor, "Mp");
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnHpChanged -= UpdateHpUI;
            player.OnMpChanged -= UpdateMpUI;
        }
    }


        //Hp 또는 Mp Udate
    private void UpdateUI(Image[] images, float currentValue, float maxValue, Color fullColor, Color emptyColor, string label) //확인용 label (나중에 지우기)
    {
        Debug.Log($"UI 업데이트({label}) => 최대 : {maxValue}, 현재  : {currentValue}");

        for (int i = 0; i < images.Length; i++)
        {
            if (i < maxValue) // 최대 체력/마나만큼 활성화
            {
                images[i].enabled = true;
                images[i].color = (i < currentValue) ? fullColor : emptyColor;
            }
            else
            {
                images[i].enabled = false;
            }
        }
    }
}
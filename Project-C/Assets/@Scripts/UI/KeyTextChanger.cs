using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeyTextChanger : MonoBehaviour
{
    public TMP_Text[] txt; // UI에 표시할 키 텍스트 배열

    private void Start()
    {
        // 🔥 KeySetting.keys가 초기화될 때까지 대기
        StartCoroutine(WaitForKeyMappings());

        // KeySetting이 변경될 때마다 UI 텍스트를 업데이트하도록 이벤트를 구독
        OptionControl.OnKeyChanged += UpdateKeyTexts;
    }

    private void OnDestroy()
    {
        // 더 이상 필요하지 않으면 이벤트 구독을 해제
        OptionControl.OnKeyChanged -= UpdateKeyTexts;
    }

    private IEnumerator WaitForKeyMappings()
    {
        // 🔍 KeySetting.keys가 초기화될 때까지 대기
        while (KeySetting.keys.Count == 0)
        {
            yield return null; // 다음 프레임까지 대기
        }

        UpdateKeyTexts(); // 초기화 후 텍스트 갱신
    }

    // 키 텍스트를 업데이트하는 메서드
    public void UpdateKeyTexts()
    {
        if (KeySetting.keys.Count == 0)
        {
            Debug.LogError("🚨 KeySetting.keys가 비어 있음! OptionControl 실행을 확인하세요.");
            return;
        }

        int updateCount = Mathf.Min(txt.Length, (int)KeyInput.KEYCOUNT); // 최대 업데이트할 수 있는 키 개수
        for (int i = 0; i < updateCount; i++)
        {
            if (KeySetting.keys.ContainsKey((KeyInput)i))
            {
                txt[i].text = KeySetting.keys[(KeyInput)i].ToString(); // 키 텍스트 업데이트
            }
        }
    }
}

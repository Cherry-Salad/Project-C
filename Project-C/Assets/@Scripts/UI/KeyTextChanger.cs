using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyTextChanger : MonoBehaviour
{
    public TMP_Text[] txt; // UI에 표시할 키 텍스트 배열

    private void Start()
    {
        UpdateKeyTexts();
        OptionControl.OnKeyChanged += UpdateKeyTexts;
    }

    private void OnDestroy()
    {
        OptionControl.OnKeyChanged -= UpdateKeyTexts;
    }

    public void UpdateKeyTexts()
    {
        int updateCount = Mathf.Min(txt.Length, (int)KeyInput.KEYCOUNT);
        for (int i = 0; i < updateCount; i++)
        {
            if (KeySetting.keys.ContainsKey((KeyInput)i))
            {
                txt[i].text = KeySetting.keys[(KeyInput)i].ToString();
            }
        }
    }
}

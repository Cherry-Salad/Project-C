using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public enum KeyInput
{
    UP,         // 0
    DOWN,       // 1
    LEFT,       // 2
    RIGHT,      // 3
    JUMP,       // 4
    DASH,       // 5
    ATTACK,     // 6
    HEAL,       // 7
    NEXT,       // 8
    SKILL1,     // 9
    MENU,       // 10
    MAP,        // 11
    INVENTORY,  // 12
    SKILL,      // 13
    KEYCOUNT    // 14 (키 개수 카운트)
}

public static class KeySetting
{
    public static Dictionary<KeyInput, KeyCode> keys = new Dictionary<KeyInput, KeyCode>();
}

public class OptionControl : MonoBehaviour
{
    public static event Action OnKeyChanged; // 키 변경 이벤트 추가
    [SerializeField] private GameObject alertPanel;
    [SerializeField] private TMP_Text alertText;

    private KeyCode[] defaultKeys = new KeyCode[]
    {
        KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D,
        KeyCode.LeftControl, KeyCode.LeftShift, KeyCode.Z, KeyCode.X,
        KeyCode.Space, KeyCode.Alpha1, KeyCode.Tab, KeyCode.M,
        KeyCode.I, KeyCode.K
    };

    private int key = -1; // 현재 키 변경 대기 상태

    void Awake()
    {
        KeySetting.keys.Clear(); //키 딕셔너리 초기화
        LoadKeyMappings(); // 저장된 키 로드
        alertPanel.SetActive(false);
    }

    private void Update()
    {
        if (key >= 0)
        {
            DetectKeyChange();
        }
    }

    private void DetectKeyChange()
    {
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                // ESC 키 변경 불가
                if (keyCode == KeyCode.Escape)
                {
                    ShowAlert("ESC can't change");
                    key = -1;
                    return;
                }

                //이미 사용중인 키 확인
                if (KeySetting.keys.ContainsValue(keyCode))
                {
                    ShowAlert($"'{keyCode}' is already use");
                    key = -1;
                    return;
                }

                ShowAlert($"Key Changed: {keyCode}");
                KeySetting.keys[(KeyInput)key] = keyCode;
                key = -1;
                SaveKeyMappings(); //변경 즉시 저장
                OnKeyChanged?.Invoke();
                return;
            }
        }
    }

    public void ChangeKey(int num)
    {
        if ((KeyInput)num == KeyInput.MENU) // ESC 키 변경 방지
        {
            ShowAlert("ESC can't change");
            return;
        }

        Debug.Log(($"키 변경 대기 상태: {num} ({(KeyInput)num})"));
        key = num;
    }

    private void SaveKeyMappings()
    {
        foreach (var key in KeySetting.keys)
        {
            PlayerPrefs.SetString(key.Key.ToString(), key.Value.ToString());
        }
        PlayerPrefs.Save(); // 변경 사항을 즉시 저장
        Debug.Log("키 설정이 저장되었습니다.");
    }

    private void LoadKeyMappings()
    {
        foreach (var key in KeySetting.keys.Keys)
        {
            if (PlayerPrefs.HasKey(key.ToString())) // 저장된 값이 있는지 확인
            {
                string savedKey = PlayerPrefs.GetString(key.ToString());
                if (Enum.TryParse(savedKey, out KeyCode loadedKey)) // KeyCode로 변환 성공 여부 확인
                {
                    KeySetting.keys[key] = loadedKey;
                }
                else
                {
                    Debug.LogWarning($"키 로드 실패: {key}에 대한 저장된 값 '{savedKey}'을(를) 변환할 수 없습니다.");
                }
            }
        }
        Debug.Log("키 설정이 로드되었습니다.");
    }

    private void ShowAlert(string message)
    {
        alertText.text = message;
        alertPanel.SetActive(true);
        StartCoroutine(ShowAlertDelay(2f)); //2초간 보여주기
    }

    private IEnumerator ShowAlertDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        alertPanel.SetActive(false);
    }
}

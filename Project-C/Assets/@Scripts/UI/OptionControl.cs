using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    SKILL2,     // 10
    SKILL3,     // 11
    MENU,       // 12
    MAP,        // 13
    INVENTORY,  // 14
    SKILL,      // 15
    KEYCOUNT    // 16 (키 개수 카운트)
}

public static class KeySetting
{
    public static Dictionary<KeyInput, KeyCode> keys = new Dictionary<KeyInput, KeyCode>();

    //아래 3가지 코드를 사용하면 바인딩이 가능합니다~
    public static bool GetKeyDown(KeyInput key) => keys.ContainsKey(key) && Input.GetKeyDown(keys[key]); // 키 누를 때
    public static bool GetKey(KeyInput key) => keys.ContainsKey(key) && Input.GetKey(keys[key]);         // 키 눌려있는 상태
    public static bool GetKeyUp(KeyInput key) => keys.ContainsKey(key) && Input.GetKeyUp(keys[key]);     // 키 땔 때
}

public class OptionControl : MonoBehaviour
{
    public static event Action OnKeyChanged; // 키 변경 이벤트
    [SerializeField] private GameObject alertPanel;
    [SerializeField] private TMP_Text alertText;

    private static readonly KeyCode[] defaultKeys = new KeyCode[]
    {
        KeyCode.UpArrow,    // 0  (UP)
        KeyCode.DownArrow,  // 1  (DOWN)
        KeyCode.LeftArrow,  // 2  (LEFT)
        KeyCode.RightArrow, // 3  (RIGHT)
        KeyCode.LeftControl,// 4  (JUMP)
        KeyCode.LeftShift,  // 5  (DASH)
        KeyCode.Z,          // 6  (ATTACK)
        KeyCode.X,          // 7  (HEAL)
        KeyCode.Space,      // 8  (NEXT)
        KeyCode.A,          // 9  (SKILL1)
        KeyCode.S,          // 10 (SKILL2)
        KeyCode.Alpha1,     // 11 (SKILL3)
        KeyCode.Escape,     // 12 (MENU)
        KeyCode.M,          // 13 (MAP)
        KeyCode.I,          // 14 (INVENTORY)
        KeyCode.K,          // 15 (SKILL)
    };

    private int key = -1; // 현재 키 변경 대기 상태

    void Awake()
    {
        if (FindObjectsOfType<OptionControl>().Length > 1) // 씬에 OptionControl이 여러 개 존재하는 경우
        {
            Destroy(gameObject); // 다른 객체는 파괴
            return;
        }

        DontDestroyOnLoad(gameObject); // 씬이 변경되어도 설정값 유지

        KeySetting.keys.Clear(); // 기존 키 딕셔너리 초기화
        AddDefaultKeys(); // 기본 키 추가
        LoadKeyMappings(); // 저장된 키 불러오기
        SaveKeyMappings(); // 기본 키를 강제로 저장
        OnKeyChanged?.Invoke(); // UI 업데이트
        alertPanel.SetActive(false); // 경고 패널 비활성화
    }

    private void Update()
    {
        if (key >= 0) DetectKeyChange(); // 키 변경 대기 상태일 때 키 입력 감지
    }

    // 기본 키를 KeySetting.keys에 추가
    private void AddDefaultKeys()
    {
        KeyInput[] keyInputs = (KeyInput[])Enum.GetValues(typeof(KeyInput));
        for (int i = 0; i < keyInputs.Length - 1; i++) // keyInputs.Length - 1 : KeyCount 제외
        {
            if (!KeySetting.keys.ContainsKey(keyInputs[i]))
            {
                KeySetting.keys[keyInputs[i]] = defaultKeys[i]; // KeySetting.keys에 기본키 추가
            }
        }
    }

    // 키 변경 감지 및 처리
    private void DetectKeyChange()
    {
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                if (keyCode == KeyCode.Escape) // ESC 변경 방지
                {
                    ShowAlert("ESC can't change");
                    key = -1;
                    return;
                }

                if (KeySetting.keys.ContainsValue(keyCode)) // 중복 키 확인
                {
                    ShowAlert($"'{keyCode}' is already used.");
                    key = -1;
                    return;
                }

                KeySetting.keys[(KeyInput)key] = keyCode; // 해당 KeyInput에 새 키 바인딩
                key = -1;
                SaveKeyMappings(); // 변경 즉시 저장
                OnKeyChanged?.Invoke(); // UI 업데이트
                return;
            }
        }
    }

    // 사용자가 버튼을 눌렀을 때 키 변경 대기 상태로 전환 
    public void ChangeKey(int num)
    {
        if ((KeyInput)num == KeyInput.MENU) // ESC 변경 방지
        {
            ShowAlert("ESC can't change");
            return;
        }

        key = num; // 키 변경 대기 상태 활성화
        OnKeyChanged?.Invoke(); // UI 업데이트
    }

    // 저장된 키 로드 (없을 경우 기본 키 적용)
    private void LoadKeyMappings()
    {
        KeyInput[] keyInputs = (KeyInput[])Enum.GetValues(typeof(KeyInput));
        for (int i = 0; i < keyInputs.Length - 1; i++) // KeyCount 제외
        {
            if (PlayerPrefs.HasKey(keyInputs[i].ToString())) // 저장된 키가 있을 경우
            {
                string savedKey = PlayerPrefs.GetString(keyInputs[i].ToString());
                if (Enum.TryParse(savedKey, out KeyCode loadedKey))
                {
                    KeySetting.keys[keyInputs[i]] = loadedKey; // 저장된 키로 바인딩
                }
                else
                {
                    KeySetting.keys[keyInputs[i]] = defaultKeys[i]; // 유효하지 않으면 기본값 사용
                }
            }
            else
            {
                KeySetting.keys[keyInputs[i]] = defaultKeys[i]; // 저장된 값이 없으면 기본값 사용
            }
        }
    }

    // 현재 키 설정을 PlayerPrefs에 저장
    private void SaveKeyMappings()
    {
        foreach (var key in KeySetting.keys)
        {
            PlayerPrefs.SetString(key.Key.ToString(), key.Value.ToString()); // 키와 키 코드를 저장
        }
        PlayerPrefs.Save(); // PlayerPrefs 저장
    }

    // 경고 메시지를 화면에 표시
    private void ShowAlert(string message)
    {
        alertText.text = message;
        alertPanel.SetActive(true);
        StartCoroutine(ShowAlertDelay(2f)); // 2초 후 경고 메시지 숨기기
    }

    private IEnumerator ShowAlertDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        alertPanel.SetActive(false);
    }
}
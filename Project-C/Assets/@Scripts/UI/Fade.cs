using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    public static Fade Instance { get; private set; }

    [SerializeField] private Image Panel;
    private float FadeTime = 1f; // 페이드 지속 시간

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    //Fade-In 후 완료시 onComplete호출 => StartGame()이 비동기식이라 해줘야함
    public void FadeIn(Action onComplete)
    {
        StartCoroutine(FadeInFlow(onComplete));
    }

    private IEnumerator FadeInFlow(Action onComplete)
    {
        Panel.gameObject.SetActive(true);
        float elapsed = 0f;
        Color c = Panel.color;
        c.a = 0f;
        Panel.color = c;

        while (elapsed < FadeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / FadeTime);
            c.a = t;
            Panel.color = c;
            yield return null;
        }

        c.a = 1f;
        Panel.color = c;

        onComplete?.Invoke();
    }

    //Fade-Out 후 완료시 onComplete호출 => StartGame()이 비동기식이라 해줘야함
    public void FadeOut()
    {
        StartCoroutine(FadeOutFlow());
    }

    private IEnumerator FadeOutFlow()
    {
        Panel.gameObject.SetActive(true);
        float elapsed = 0f;
        Color c = Panel.color;
        c.a = 1f;
        Panel.color = c;

        while (elapsed < FadeTime)
        {
            elapsed += Time.deltaTime;
            c.a = 1f - Mathf.Clamp01(elapsed / FadeTime);
            Panel.color = c;
            yield return null;
        }

        c.a = 0f;
        Panel.color = c;
        Panel.gameObject.SetActive(false);
    }

    public void OnBlackScreen(bool isBlack)
    {
        Panel.gameObject.SetActive(isBlack);
    }
}
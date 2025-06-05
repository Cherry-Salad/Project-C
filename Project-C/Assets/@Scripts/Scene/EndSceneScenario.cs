using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndSceneScenario : MonoBehaviour
{
    public GameObject Cat;
    public GameObject BlackSceen;
    public GameObject text;

    private EndSceneCherry cherry;

    void Start()
    {
        text.SetActive(false);
        AudioManager.Instance.StartBGM(AudioManager.Instance.EndSceneBackGround);
        cherry = Cat.GetComponent<EndSceneCherry>();
        Fade.Instance.OnBlackScreen(true);
        StartCoroutine(CoScenario());
    }

    private IEnumerator CoScenario()
    {
        yield return new WaitForSeconds(1f);
        Fade.Instance.FadeOut();
        yield return new WaitForSeconds(1.5f);
        cherry.MoveEndPoint();

        while (true)
        {
            if (!Cat.activeSelf) break;
            yield return null;
        }

        Fade.Instance.FadeIn(() =>
        {
            text.SetActive(true);
        });

        yield return new WaitForSeconds(5f);

        Fade.Instance.OnBlackScreen(false);
        SceneManager.LoadScene("MainScene");
    }

}

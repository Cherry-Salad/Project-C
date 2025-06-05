using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;

public class SceneManagerEX
{
    public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }

    public void LoadScene(EScene type)
    {
        string name = System.Enum.GetName(typeof(EScene), type);
        SceneManager.LoadScene(name);
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }
}

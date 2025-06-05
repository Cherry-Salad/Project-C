using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static bool s_init = false;

    // Singleton pattern
    static Managers s_instance;
    static Managers Instance { get { Init(); return s_instance; } }

    ResourceManager _resource = new ResourceManager();
    PoolManager _pool = new PoolManager();
    DataManager _data = new DataManager();
    SceneManagerEX _scene = new SceneManagerEX();
    GameManager _game = new GameManager();
    MapManager _map = new MapManager();
    CameraManager _camera = new CameraManager();
    
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static DataManager Data { get { return Instance._data; } }
    public static SceneManagerEX Scene { get { return Instance._scene; } }
    public static GameManager Game { get { return Instance._game; } }
    public static MapManager Map { get { return Instance._map; } }
    public static CameraManager Camera { get { return Instance._camera; } }

    public static void Init()
    {
        // 초기화 반복 방지
        if (s_init)
            return;

        GameObject go = GameObject.Find("@Managers");   // go는 GameObject의 줄임말
        if (go == null)
        {
            // @Managers이 없다면 생성
            go = new GameObject() { name = "@Managers" };
            go.AddComponent<Managers>();
        }

        DontDestroyOnLoad(go);
        s_instance = go.GetComponent<Managers>();
        s_init = true;

    }

    public static void Clear()
    {
        Resource.Clear();
        Pool.Clear();
        Camera.Clear();
    }
}

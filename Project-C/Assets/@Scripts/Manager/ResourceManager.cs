using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class ResourceManager
{
    // 로드된 리소스를 캐싱하기 위한 Dictionary
    Dictionary<string, Object> _resources = new Dictionary<string, Object>();

    // Addressable 로드 핸들을 저장하는 Dictionary
    Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();

    /// <summary>
    /// 로드된 리소스를 반환
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Load<T>(string key) where T : Object
    {
        if (_resources.TryGetValue(key, out Object resource))
            return resource as T;

        // Sprite 타입의 리소스는 ".sprite"가 필요하여 자동으로 처리
        if (typeof(T) == typeof(Sprite) && key.Contains(".sprite") == false)
        {
            if (_resources.TryGetValue($"{key}.sprite", out resource))
                return resource as T;
        }

        // 리소스를 찾지 못했다
        return null;
    }

    /// <summary>
    /// 로드된 프리팹을 인스턴스화
    /// </summary>
    /// <param name="key"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public GameObject Instantiate(string key, Transform parent = null, bool pooling = false)
    {
        GameObject prefab = Load<GameObject>($"{key}");
        if (prefab == null)
        {
            Debug.LogError($"Failed to load prefab : {key}");
            return null;
        }

        if (pooling)
            return Managers.Pool.Pop(prefab);

        GameObject go = Object.Instantiate(prefab, parent);
        go.name = prefab.name;
        return go;
    }

    public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        Object.Destroy(go);
    }

    #region Addressable
    /// <summary>
    /// Addressable 리소스 비동기 로드
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="callback"></param>
    public void LoadAsync<T>(string key, Action<T> callback = null) where T : Object
    {
        // 이미 캐싱된 리소스가 있으면 바로 반환
        if (_resources.TryGetValue(key, out Object resource))
        {
            callback?.Invoke(resource as T);
            return;
        }

        // Sprite 리소스 처리를 위하여 이름 포맷 변경
        string loadkey = key;
        if (key.Contains(".sprite"))
            loadkey = $"{key}[{key.Replace(".sprite", "")}]";

        // Addressables API로 비동기 로드
        var asyncOperation = Addressables.LoadAssetAsync<T>(key);
        asyncOperation.Completed += (op) =>
        {
            // 로드 완료 후 캐싱 및 콜백 호출
            _resources.Add(key, op.Result);
            _handles.Add(key, asyncOperation);
            callback?.Invoke(op.Result);
        };
    }

    /// <summary>
    /// 특정 라벨의 모든 Addressable 리소스를 비동기 로드
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="label"></param>
    /// <param name="callback"></param>
    public void LoadAllAsync<T>(string label, Action<string, int, int> callback = null) where T : Object
    {
        var opHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
        opHandle.Completed += (op) =>
        {
            int loadCount = 0;
            int totalCount = op.Result.Count;

            foreach (var result in op.Result)
            {
                // 만약 ".sprite"가 없다면 Sprite로 인식하지 않는다. 진짜 왜?
                if (result.PrimaryKey.Contains(".sprite"))
                {
                    LoadAsync<Sprite>(result.PrimaryKey, (obj) =>
                    {
                        loadCount++;
                        callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                    });
                }
                else
                {
                    LoadAsync<T>(result.PrimaryKey, (obj) =>
                    {
                        loadCount++;
                        callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                    });
                }
            }
        };
    }

    /// <summary>
    /// 로드된 리소스와 핸들 해제
    /// </summary>
    public void Clear()
    {
        _resources.Clear(); // 리소스 캐시 초기화

        foreach (var handle in _handles)
            Addressables.Release(handle);
        _handles.Clear();
    }
    #endregion
}

using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager
{
    public CinemachineBrain Brain;
    public CameraController CurrentCamera { get; private set; }
    public int CurrentCameraIndex { get; private set; } = -1;

    public List<CameraController> Cameras = new List<CameraController>();
    public Transform CameraRoot
    {
        get
        {
            GameObject root = GameObject.Find("@Cameras");
            if (root == null)
                root = new GameObject { name = "@Cameras" };

            return root.transform;
        }
    }

    int _defaultPriority = 10;
    int _activePriority = 20;

    public CameraController Spawn<T>(T target, PolygonCollider2D boundary, int priority = 10) where T : BaseObject
    {
        if (Brain == null)
            Brain = Camera.main.GetComponent<CinemachineBrain>();

        // TODO: 오브젝트 풀링
        GameObject go = Managers.Resource.Instantiate("VirtualCamera", CameraRoot);
        CameraController camera = go.GetComponent<CameraController>();
        camera.SetInfo(target, boundary, priority);
        Cameras.Add(camera);

        return camera;
    }

    public void Clear()
    {
        CurrentCamera = null;
        CurrentCameraIndex = -1;
        Cameras.Clear();
        
        foreach (CameraController ca in Cameras)
            Managers.Resource.Destroy(ca.gameObject);
    }

    public void SetCurrentCamera(int idx)
    {
        if (idx <= -1 || Cameras.Count <= idx || CurrentCameraIndex == idx)
            return;
        
        if (CurrentCamera != null)
        {
            // 이전 카메라는 기본 우선순위로 설정
            CurrentCamera.Priority = _defaultPriority;
        }
        // TODO: 맵을 로드하고 처음으로 설정한 카메라는 블렌드 효과가 필요없다

        CurrentCamera = Cameras[idx];
        CurrentCameraIndex = idx;

        // 카메라 활성화
        CurrentCamera.Priority = _activePriority;
    }

    public void SetCurrentCamera(CameraController camera)
    {
        if (CurrentCamera == camera)
            return;

        if (CurrentCamera != null)
        {
            // 이전 카메라는 기본 우선순위로 설정
            CurrentCamera.Priority = _defaultPriority;
        }
        // TODO: 맵을 로드하고 처음으로 설정한 카메라는 블렌드 효과가 필요없다

        CurrentCamera = camera;
        CurrentCameraIndex = Cameras.IndexOf(camera);

        // 카메라 활성화
        CurrentCamera.Priority = _activePriority;
    }

    public void SetCurrentCamera(PolygonCollider2D boundary, string name = null)
    {
        if (CurrentCamera != null && CurrentCamera.Boundary == boundary)
            return;

        foreach (CameraController camera in Cameras)
        {
            bool find = string.IsNullOrEmpty(name) && camera.Boundary == boundary;
            bool findName = camera.Boundary.name == name && camera.Boundary == boundary;

            if (find || findName)
            {
                if (CurrentCamera != null)
                {
                    // 이전 카메라는 기본 우선순위로 설정
                    CurrentCamera.Priority = _defaultPriority;
                }

                // TODO: 맵을 로드하고 처음으로 설정한 카메라는 블렌드 효과가 필요없다

                CurrentCamera = camera;
                CurrentCameraIndex = Cameras.IndexOf(camera);

                // 카메라 활성화
                CurrentCamera.Priority = _activePriority;
                break;
            }
        }
    }
}
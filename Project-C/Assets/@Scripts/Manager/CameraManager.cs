using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager
{
    public CinemachineBrain Brain;
    public CameraController CurrentCamera { get; private set; }

    public HashSet<CameraController> Cameras = new HashSet<CameraController>();
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

    /// <summary>
    /// 메인 카메라를 가져오고, 룸 기반으로 카메라를 스폰한다.
    /// </summary>
    public void Load()
    {
        Clear();

        GameObject p = GameObject.Find("Player");  // TODO: 게임 매니저에서 플레이어를 찾는다
        Player player = p.GetComponent<Player>();

        Brain = Camera.main.GetComponent<CinemachineBrain>();

        // 룸 기반 카메라
        foreach (Room room in Managers.Map.Rooms)
        {
            if (room.CameraBoundary == null)
                continue;

            CameraController camera = Spawn(player, room.CameraBoundary);
            camera.name = room.name;
        }


        // 블렌드 효과를 임시로 비활성화
        // Brain.m_CustomBlends를 임시로 null하면 카메라가 잠깐 깜빡거려서 Brain 자체를 비활성화하였다
        // Hoxy... 더 좋은 방법이 있다면 알려주세용
        Brain.enabled = false;  // 문제가 생길줄 알았는데, 이게 되네..?
    }

    public CameraController Spawn<T>(T target, PolygonCollider2D boundary, int priority = 10) where T : BaseObject
    {
        GameObject go = Managers.Resource.Instantiate("VirtualCamera", CameraRoot);
        CameraController camera = go.GetComponent<CameraController>();
        camera.SetInfo(target, boundary, priority);
        Cameras.Add(camera);

        return camera;
    }

    public void Clear()
    {
        CurrentCamera = null;
        Cameras.Clear();
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
        else
            Brain.enabled = true;   // 블렌드 효과 활성화

        // 카메라 활성화
        CurrentCamera = camera;
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
                else
                    Brain.enabled = true;   // 블렌드 효과 활성화

                // 카메라 활성화
                CurrentCamera = camera;
                CurrentCamera.Priority = _activePriority;
                break;
            }
        }
    }
}
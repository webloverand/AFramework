/*******************************************************************
* Copyright(c)
* 文件名称: AFSDK_AFoudation.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/


namespace AFramework
{
#if AF_ARSDK_AFoudation
    using UnityEngine;
    using System.Collections.Generic;
    using UnityEngine.XR.ARFoundation;
    using System;
    using UnityEngine.XR.ARSubsystems;

    [RequireComponent(typeof(ARRaycastManager))]
    public class AFSDK_AFDPlaneHandle : MonoBehaviour
    {
        Action<GameObject> PlacedCall;

        ARRaycastManager m_RaycastManager;
        ARPlaneManager ARplaneManager;
        ARPointCloudManager ARpointCloudManager;
        List<ARPlane> TrackPlane = new List<ARPlane>();
        List<ARPointCloud> TrackPointPlane = new List<ARPointCloud>();
        List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
        //点击放置的GameObject
        GameObject m_PlacedPrefab;
        //实例化后的物体
        public GameObject spawnedObject { get; private set; }

        void Awake()
        {
            m_RaycastManager = GetComponent<ARRaycastManager>();
            ARplaneManager = GetComponent<ARPlaneManager>();
            ARpointCloudManager = GetComponent<ARPointCloudManager>();
            ARplaneManager.planesChanged += PlaneChanged;
            if(ARpointCloudManager != null)
                ARpointCloudManager.pointCloudsChanged += PointChanged;
        }
        public void Init(GameObject obj, Action<GameObject> PlacedCall)
        {
            m_PlacedPrefab = obj;
            RegisterPlacedCall(PlacedCall);
        }
        public void RegisterPlacedCall(Action<GameObject> PlacedCall)
        {
            this.PlacedCall = PlacedCall;
        }

        private void Update()
        {
            if (!TryGetTouchPosition(out Vector2 touchPosition))
                return;

            if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon) && !PosHelper.IsPointerOverGameObject())
            {
                var hitPose = s_Hits[0].pose;
                if (spawnedObject == null)
                {
                    spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
                }
                else
                {
                    spawnedObject.transform.position = hitPose.position;
                }
                PlacedCall.InvokeGracefully(spawnedObject);
            }
        }
        /// <summary>
        /// 根据运行环境获取点击点
        /// </summary>
        /// <param name="touchPosition"></param>
        /// <returns></returns>
        bool TryGetTouchPosition(out Vector2 touchPosition)
        {
#if UNITY_EDITOR
            if (Input.GetMouseButton(0))
            {
                var mousePosition = Input.mousePosition;
                touchPosition = new Vector2(mousePosition.x, mousePosition.y);
                return true;
            }
#else
        if (Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }
#endif

            touchPosition = default;
            return false;
        }

        /// <summary>
        /// 检测到的平面变化回调
        /// </summary>
        /// <param name="changeEvent"></param>
        public void PlaneChanged(ARPlanesChangedEventArgs changeEvent)
        {
            for (int i = 0; i < changeEvent.added.Count; i++)
            {
                TrackPlane.Add(changeEvent.added[i]);
            }
            for (int i = 0; i < changeEvent.removed.Count; i++)
            {
                if (TrackPlane.Contains(changeEvent.removed[i]))
                    TrackPlane.Remove(changeEvent.removed[i]);
            }
        }
        /// <summary>
        /// 检测anchor变化回调
        /// </summary>
        /// <param name="changeEvent"></param>
        public void PointChanged(ARPointCloudChangedEventArgs changeEvent)
        {
            for (int i = 0; i < changeEvent.added.Count; i++)
            {
                TrackPointPlane.Add(changeEvent.added[i]);
            }
            for (int i = 0; i < changeEvent.removed.Count; i++)
            {
                if (TrackPointPlane.Contains(changeEvent.removed[i]))
                    TrackPointPlane.Remove(changeEvent.removed[i]);
            }
        }
        /// <summary>
        /// 隐藏平面
        /// </summary>
        public void HidePlane()
        {
            for (int i = 0; i < TrackPlane.Count; i++)
            {
                TrackPlane[i].gameObject.SetActive(false);
            }

            if (ARpointCloudManager && ARpointCloudManager.enabled)
            {
                ARpointCloudManager.enabled = false;
                for (int i = 0; i < TrackPointPlane.Count; i++)
                {
                    TrackPointPlane[i].gameObject.SetActive(false);
                }
            }
        }

    }
#endif
}


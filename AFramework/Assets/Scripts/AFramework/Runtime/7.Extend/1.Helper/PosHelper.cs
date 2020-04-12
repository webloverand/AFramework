namespace AFramework
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class PosHelper
    {
        /// <summary>
        /// 朝向camera
        /// </summary>
        /// <param name="augmentation"></param>
        /// <param name="CameraToward"></param>
        public static void RotateTowardCamera(GameObject augmentation, Transform CameraToward)
        {
            if (CameraToward != null)
            {
                var lookAtPosition = CameraToward.position - augmentation.transform.position;
                lookAtPosition.y = 0;
                var rotation = Quaternion.LookRotation(lookAtPosition);
                augmentation.transform.rotation = rotation;
            }
        }
        /// <summary>
        /// 判断是否点击在UI上
        /// </summary>
        /// <returns></returns>
        public static bool IsPointerOverGameObject()
        {
            PointerEventData eventData = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            eventData.pressPosition = Input.mousePosition;
            eventData.position = Input.mousePosition;

            List<RaycastResult> list = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, list);
            return list.Count > 0;
        }
    }

}

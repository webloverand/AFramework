


namespace AFramework
{
    using System.Diagnostics;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using Debug = UnityEngine.Debug;
    /// <summary>
    /// 在Hierarchy面板按鼠标中键相当于开关GameObject/在Project面板按鼠标中键相当于Show In Explorer
    /// </summary>
    internal class SelectionHelper
    {
        [InitializeOnLoadMethod]
        private static void Start()
        {
            //在Hierarchy面板按鼠标中键相当于开关GameObject
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGui;

            //在Project面板按鼠标中键相当于Show In Explorer
            EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGui;
        }

        private static void ProjectWindowItemOnGui(string guid, Rect selectionRect)
        {
            if (Event.current.type == EventType.MouseDown
            && Event.current.button == 2
            && selectionRect.Contains(Event.current.mousePosition))
            {
                string strPath = AssetDatabase.GUIDToAssetPath(guid);
                if (Event.current.alt)
                {
                    Debug.Log(strPath);
                    Event.current.Use();
                    return;
                }

                if (Path.GetExtension(strPath) == string.Empty) //文件夹
                {
                    Process.Start(Path.GetFullPath(strPath));
                }
                else //文件
                {
                    Process.Start("explorer.exe", "/select," + Path.GetFullPath(strPath));
                }

                Event.current.Use();
            }
        }

        private static void HierarchyWindowItemOnGui(int instanceID, Rect selectionRect)
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Space:
                        ToggleGameObjcetActiveSelf();
                        e.Use();
                        break;
                }
            }
            else if (e.type == EventType.MouseDown && e.button == 2)
            {
                if (e.alt)
                {
                    Selection.gameObjects.ForEach(go => Debug.Log(go.transform.GetPath()));
                    e.Use();
                }
                else
                {
                    ToggleGameObjcetActiveSelf();
                    e.Use();
                }
            }
        }

        internal static void ToggleGameObjcetActiveSelf()
        {
            Undo.RecordObjects(Selection.gameObjects, "Active");
            Selection.gameObjects.ForEach(go =>
            {
                go.SetActive(!go.activeSelf);
            });

        }
    }
}

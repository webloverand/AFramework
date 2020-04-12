using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AFramework
{

    internal static class GameObjectMenuItem
    {
        [MenuItem("GameObject/Transform/Group &G", false)]
        private static void Group()
        {
            //Debug.Log("Group");
            GameObject[] gameObjects = Selection.gameObjects;
            if (gameObjects.Length == 0)
            {
                return;
            }

            Transform parent = gameObjects[0].transform.parent;
            int nSiblingIndex = gameObjects[0].transform.GetSiblingIndex();
            GameObject go = new GameObject("Group");
            Undo.RegisterCreatedObjectUndo(go, "CreateEmpty");
            Undo.FlushUndoRecordObjects();
            for (int i = 0; i < gameObjects.Length; i++)
            {
                GameObject gameObject = gameObjects[i];
                Undo.SetTransformParent(gameObject.transform, go.transform, "Group");
            }

            go.transform.SetParent(parent);
            go.transform.SetSiblingIndex(nSiblingIndex);
            EditorApplication.DirtyHierarchyWindowSorting();
            EditorGUIUtility.PingObject(gameObjects[0]);
        }

        [MenuItem("GameObject/Transform/Sort &S", false)]
        private static void Sort()
        {
            GameObject[] gameObjects = Selection.gameObjects;
            if (gameObjects.Length == 0)
            {
                return;
            }

            for (int i = 0; i < Selection.transforms.Length; i++)
            {
                Transform transform = Selection.transforms[i];
                Sort(transform);
            }

            EditorApplication.DirtyHierarchyWindowSorting();
        }

        private static void Sort(Transform transform)
        {
            Undo.RegisterFullObjectHierarchyUndo(transform, "Sort");
            int count = transform.childCount;
            int nLast = count - 1;
            for (int i = 1; i < count; i++)
            {
                Transform tLast = transform.GetChild(nLast);
                tLast.SetSiblingIndex(i);
                for (int j = 0; j < i; j++)
                {
                    Transform next = transform.GetChild(j);
                    int n = EditorUtility.NaturalCompare(tLast.name, next.name);
                    if (n < 0)
                    {
                        tLast.SetSiblingIndex(j);
                        break;
                    }
                }
            }
        }
    }
}

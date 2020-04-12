using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
namespace AFramework
{ 
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Transform), true)]
    internal class TransformInspector : CustomCustomEditor
    {
        private float scale = 1;
        private SerializedProperty spLocalPosition;
        private SerializedProperty spLocalRotation;
        private SerializedProperty spLocalScale;
        private TransformRotationGUI rotationGUI;
        //Button的Style
        private static GUIStyle style;
        //初始化CustomCustomEditor
        internal TransformInspector()
            : base("TransformInspector") { }

        protected override void OnSceneGUI() { }

        protected void OnEnable()
        {
            //按名称查找序列化的属性。
            spLocalPosition = serializedObject.FindProperty("m_LocalPosition");
            spLocalRotation = serializedObject.FindProperty("m_LocalRotation");
            spLocalScale = serializedObject.FindProperty("m_LocalScale");
            //在相对于当前属性的相对路径中检索SerializedProperty。
            scale = spLocalScale.FindPropertyRelative("x").floatValue;
            if (s_Contents == null)
            {
                s_Contents = new Contents();
            }

            if (rotationGUI == null)
            {
                rotationGUI = new TransformRotationGUI();
            }

            rotationGUI.OnEnable(spLocalRotation, s_Contents.rotationContent);
        }

        private static Vector3 Round(Vector3 v3Value, int nDecimalPoint = 0)
        {
            int nScale = 1;
            for (int i = 0; i < nDecimalPoint; i++)
            {
                nScale *= 10;
            }

            v3Value *= nScale;
            v3Value.x = Mathf.RoundToInt(v3Value.x);
            v3Value.y = Mathf.RoundToInt(v3Value.y);
            v3Value.z = Mathf.RoundToInt(v3Value.z);
            return v3Value / nScale;
        }

        public override void OnInspectorGUI()
        {
            if (s_Contents == null)
            {
                s_Contents = new Contents();
            }
            //更新序列化对象的表示形式。
            serializedObject.Update();

            if (style == null)
            {
                style = new GUIStyle("button");
                style.fixedWidth = 18;
                style.stretchWidth = true;
                style.fixedHeight = 16;
                style.margin = new RectOffset(0, 0, 1, 2);
            }
            //保留给Editor GUI控件标签的宽度（以像素为单位）。
            EditorGUIUtility.labelWidth = 24f;
            //编辑器GUI当前处于宽屏模式吗？
            if (!EditorGUIUtility.wideMode)
            {
                EditorGUIUtility.wideMode = true;
            }
            //为SerializedProperty创建一个字段。
            EditorGUILayout.PropertyField(spLocalPosition, s_Contents.positionContent);
            rotationGUI.RotationField();
            EditorGUILayout.PropertyField(spLocalScale, s_Contents.scaleContent);

            Rect rect = GUILayoutUtility.GetLastRect();
            rect.width = style.fixedWidth;
            rect.y -= 36;
            if (GUI.Button(rect, s_Contents.positionContent, style))
            {
                spLocalPosition.vector3Value = Vector3.zero;
            }

            rect.y += 18;
            if (GUI.Button(rect, s_Contents.rotationContent, style))
            {
                Undo.RecordObjects(targets, "rotationContent");
                MethodInfo method =
                    typeof(Transform).GetMethod("SetLocalEulerAngles", BindingFlags.Instance | BindingFlags.NonPublic);
                object[] clear = { Vector3.zero, 0 };
                for (int i = 0; i < targets.Length; i++)
                {
                    method.Invoke(targets[i], clear);
                }

                Event.current.type = EventType.Used;
            }

            rect.y += 18;
            if (GUI.Button(rect, s_Contents.scaleContent, style))
            {
                scale = 1;
                spLocalScale.vector3Value = Vector3.one;
            }

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUIUtility.labelWidth = 37f;
                float newScale = EditorGUILayout.FloatField("Scale", scale);
                if (!Mathf.Approximately(scale, newScale))
                {
                    scale = newScale;
                    spLocalScale.vector3Value = Vector3.one * scale;
                }

                EditorGUILayout.LabelField("Round", GUILayout.Width(42f));
                if (GUILayout.Button(".", "MiniButtonLeft"))
                {
                    Undo.RecordObjects(targets, "Round");
                    for (int i = 0; i < targets.Length; i++)
                    {
                        Transform o = targets[i] as Transform;
                        o.localPosition = Round(o.localPosition);
                        o.localScale = Round(o.localScale);
                    }
                }

                if (GUILayout.Button(".0", "MiniButtonMid"))
                {
                    Undo.RecordObjects(targets, "Round");
                    for (int i = 0; i < targets.Length; i++)
                    {
                        Transform o = targets[i] as Transform;
                        o.localPosition = Round(o.localPosition, 1);
                        o.localScale = Round(o.localScale, 1);
                    }
                }

                if (GUILayout.Button(".00", "MiniButtonRight"))
                {
                    Undo.RecordObjects(targets, "Round");
                    for (int i = 0; i < targets.Length; i++)
                    {
                        Transform o = targets[i] as Transform;
                        o.localPosition = Round(o.localPosition, 2);
                        o.localScale = Round(o.localScale, 2);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // Copy
            EditorGUILayout.BeginHorizontal();
            {
                var c = GUI.color;
                GUI.color = new Color(1f, 1f, 0.5f, 1f);
                using (new EditorGUI.DisabledScope(Selection.objects.Length != 1))
                {
                    if (GUILayout.Button("Copy", "ButtonLeft"))
                    {
                        TransformInspectorCopyData.localPositionCopy = spLocalPosition.vector3Value;
                        TransformInspectorCopyData.localRotationCopy = spLocalRotation.quaternionValue;
                        TransformInspectorCopyData.loacalScaleCopy = spLocalScale.vector3Value;
                        Transform t = target as Transform;
                        TransformInspectorCopyData.positionCopy = t.position;
                        TransformInspectorCopyData.rotationCopy = t.rotation;
                    }
                }

                bool isGlobal = Tools.pivotRotation == PivotRotation.Global;
                GUI.color = new Color(1f, 0.5f, 0.5f, 1f);
                if (GUILayout.Button("Paste", "ButtonMid"))
                {
                    Undo.RecordObjects(targets, "Paste Local");
                    if (isGlobal)
                    {
                        PastePosition();
                        PasteRotation();
                    }
                    else
                    {
                        spLocalPosition.vector3Value = TransformInspectorCopyData.localPositionCopy;
                        spLocalRotation.quaternionValue = TransformInspectorCopyData.localRotationCopy;
                        spLocalScale.vector3Value = TransformInspectorCopyData.loacalScaleCopy;
                    }
                }

                if (GUILayout.Button("PPos", "ButtonMid"))
                {
                    Undo.RecordObjects(targets, "PPos");
                    if (isGlobal)
                    {
                        PastePosition();
                    }
                    else
                    {
                        spLocalPosition.vector3Value = TransformInspectorCopyData.localPositionCopy;
                    }
                }

                if (GUILayout.Button("PRot", "ButtonMid"))
                {
                    Undo.RecordObjects(targets, "PRot");
                    if (isGlobal)
                    {
                        PasteRotation();
                    }
                    else
                    {
                        spLocalRotation.quaternionValue = TransformInspectorCopyData.localRotationCopy;
                    }
                }

                using (new EditorGUI.DisabledScope(isGlobal))
                {
                    if (GUILayout.Button("PSca", "ButtonMid"))
                    {
                        Undo.RecordObjects(targets, "PSca");
                        spLocalScale.vector3Value = TransformInspectorCopyData.loacalScaleCopy;
                    }
                }

                //GUI.color = new Color(1f, 0.75f, 0.5f, 1f);
                GUIContent pivotRotationContent = s_Contents.pivotPasteGlobal;
                pivotRotationContent.text = "Global";
                if (Tools.pivotRotation == PivotRotation.Local)
                {
                    pivotRotationContent = s_Contents.pivotPasteLocal;
                    pivotRotationContent.text = "Local";
                }

                GUI.color = c;
                if (GUILayout.Button(pivotRotationContent, "ButtonRight", GUILayout.MaxHeight(18)))
                {
                    Tools.pivotRotation = Tools.pivotRotation == PivotRotation.Local
                        ? PivotRotation.Global
                        : PivotRotation.Local;
                }
            }
            EditorGUILayout.EndHorizontal();

            DrawBottomPanel(target, targets);

            Transform transform = target as Transform;
            Vector3 position = transform.position;
            if (Mathf.Abs(position.x) > 100000f || Mathf.Abs(position.y) > 100000f || Mathf.Abs(position.z) > 100000f)
            {
                EditorGUILayout.HelpBox(Contents.floatingPointWarning, MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void PasteRotation()
        {
            foreach (Object o in targets)
            {
                SerializedObject so = new SerializedObject(o);
                so.FindProperty("m_LocalRotation").quaternionValue = TransformInspectorCopyData.rotationCopy;
            }
        }

        private void PastePosition()
        {
            foreach (Object o in targets)
            {
                SerializedObject so = new SerializedObject(o);
                so.FindProperty("m_LocalPosition").vector3Value = TransformInspectorCopyData.localPositionCopy;
            }
        }


        internal static void DrawBottomPanel(Object target, IEnumerable<Object> targets)
        {
            Event e = Event.current;
            if (e != null)
            {
                if (e.type == EventType.MouseDown)
                {
                    if (e.button == 2)
                    {
                        if (e.alt)
                        {
                            e.Use();
                            return;
                        }

                        SelectionHelper.ToggleGameObjcetActiveSelf();
                        e.Use();
                    }
                }
            }
        }

        private static Contents s_Contents;

        private class Contents
        {
            public readonly GUIContent positionContent = new GUIContent("P",
                "The local position of this Game Object relative to the parent. Click the button to 0.");

            public readonly GUIContent scaleContent = new GUIContent("S",
                "The local scaling of this Game Object relative to the parent. Click the button to 1.");

            public readonly GUIContent rotationContent = new GUIContent("R",
                "The local rotation of this Game Object relative to the parent. Click the button to 0.");

            public const string floatingPointWarning =
                "Due to floating-point precision limitations, it is recommended to bring the world coordinates of the GameObject within a smaller range.";

            public readonly GUIContent pivotPasteLocal =
                EditorGUIUtility.IconContent("ToolHandleLocal", "Local|Tool handles are in local paste.");

            public readonly GUIContent pivotPasteGlobal =
                EditorGUIUtility.IconContent("ToolHandleGlobal", "Global|Tool handles are in global paste.");
        }

        private class TransformRotationGUI
        {
            private object transformRotationGUI;
            private MethodInfo onEnable;
            private MethodInfo rotationField;

            public TransformRotationGUI()
            {
                if (transformRotationGUI == null)
                {
                    Type type = Type.GetType("UnityEditor.TransformRotationGUI,UnityEditor");
                    onEnable = type.GetMethod("OnEnable");
                    rotationField = type.GetMethod("RotationField", new Type[] { });
                    transformRotationGUI = Activator.CreateInstance(type);
                }
            }

            public void OnEnable(SerializedProperty property, GUIContent content)
            {
                onEnable.Invoke(transformRotationGUI, new object[] { property, content });
            }

            public void RotationField() { rotationField.Invoke(transformRotationGUI, null); }
        }
    }
}
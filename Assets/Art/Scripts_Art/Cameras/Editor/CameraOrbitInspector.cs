using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SocialPoint.Tools
{
    [CustomEditor(typeof(CameraOrbit_v2))]
    public class CameraOrbitInspector : Editor
    {
        private CameraOrbit_v2 cam;
        private SerializedProperty alternativeCamera;
        private SerializedProperty alternativeTarget;
        private SerializedProperty movementDirection;
        private SerializedProperty aaa,bbb,ccc;

        void OnEnable()
        {
            cam = (CameraOrbit_v2)target;
            alternativeCamera = serializedObject.FindProperty("alternativeCamera");
            alternativeTarget = serializedObject.FindProperty("alternativeTarget");
            movementDirection = serializedObject.FindProperty("direction");
            aaa = serializedObject.FindProperty("aaa");
            bbb = serializedObject.FindProperty("bbb");
            ccc = serializedObject.FindProperty("ccc");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUI.BeginChangeCheck();

            SectionBasicProperties();
            SectionLimits();
            SectionBehaviour();
            SectionAutomaticMovement();

            //EditorGUILayout.PropertyField(aaa);
            //EditorGUILayout.PropertyField(bbb);
            //EditorGUILayout.PropertyField(ccc);

            if (EditorGUI.EndChangeCheck())
                Undo.RegisterCompleteObjectUndo(cam, "Camera Orbit");

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed) EditorUtility.SetDirty(cam);
        }

        public void OnSceneGUI()
        {
            if (cam.useAlternativeTarget) return;

            EditorGUI.BeginChangeCheck();
            Vector3 newTargetPosition = Handles.PositionHandle(cam.targetPosition, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(cam, "Change Target Position");
                cam.targetPosition = newTargetPosition;
            }
        }

        private void SectionBasicProperties()
        {
            Header("Camera & Target");

            EditorGUIUtility.labelWidth = 200;
            EditorGUILayout.PropertyField(alternativeCamera);
            cam.useAlternativeTarget = EditorGUILayout.Toggle("Use an alternative target", cam.useAlternativeTarget);
            EditorGUI.indentLevel++;
            if (cam.useAlternativeTarget)
                EditorGUILayout.PropertyField(alternativeTarget);
            else
                cam.targetPosition = EditorGUILayout.Vector3Field("Target initial position", cam.targetPosition);
            EditorGUI.indentLevel--;
            GUILayout.Space(3);
            EditorGUIUtility.labelWidth = 270;
            cam.distance = EditorGUILayout.FloatField("Initial distance between target & camera", cam.distance);
            EditorGUILayout.LabelField(string.Format("(Current distance: {0})", GetCurrentDistance()), EditorStyles.miniLabel);

            Footer();
        }

        private float GetCurrentDistance()
        {
            Vector3 origin, end;

            if (alternativeCamera == null)
                origin = cam.transform.position;
            else
            {
                try { origin = cam.alternativeCamera.transform.position; }
                catch { origin = cam.transform.position; }
            }

            if (cam.useAlternativeTarget)
            {
                try { end = cam.alternativeTarget.transform.position; }
                catch { end = Vector3.zero; }
            }
            else end = cam.targetPosition;

            return Vector3.Distance(origin, end);
        }

        private void SectionLimits()
        {
            Header("Limits");
            
            EditorGUIUtility.labelWidth = 90;
            CreateSlider("Horizontal", ref cam.limitX, 0, 360);
            GUILayout.Space(3);
            CreateSlider("Vertical", ref cam.limitY, -90, 90);
            GUILayout.Space(3);
            CreateSlider("Near / Far", ref cam.limitCam, 0, 1000);
            Footer();
        }

        private void CreateSlider(string label, ref Vector2 reference, float minLimit, float maxLimit)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(label, GUILayout.MaxWidth(80));
                float limitX = EditorGUILayout.FloatField("Min value: ", reference.x);
                GUILayout.FlexibleSpace();
                float limitY = EditorGUILayout.FloatField("Max value: ", reference.y);
                reference = new Vector2(limitX, limitY);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.MinMaxSlider(ref reference.x, ref reference.y, minLimit, maxLimit);
        }

        private void SectionBehaviour()
        {
            Header("Behaviour");

            EditorGUIUtility.labelWidth = 200;
            cam.sensibility = EditorGUILayout.FloatField("Orbit sensibility: ", cam.sensibility);
            cam.activeSmoothness = EditorGUILayout.Toggle("Smooth camera", cam.activeSmoothness);
            EditorGUI.BeginDisabledGroup(!cam.activeSmoothness);
                cam.deacceleration = EditorGUILayout.FloatField("Deacceleration: ", cam.deacceleration);
            EditorGUI.EndDisabledGroup();
            cam.zoomSensibility = EditorGUILayout.Slider("Zoom sensibility: ", cam.zoomSensibility, 0.01f, 1);
            cam.panSensibility = EditorGUILayout.Slider("Pan sensibility: ", cam.panSensibility, 0.01f, 2);

            Footer();
        }

        private void SectionAutomaticMovement()
        {
            Header("Automatic Movement");

            EditorGUIUtility.labelWidth = 200;
            cam.automaticMovement = EditorGUILayout.Toggle("Enable automatic movement", cam.automaticMovement);

            EditorGUI.BeginDisabledGroup(!cam.automaticMovement);
            {
                cam.timeToStartMoving = EditorGUILayout.FloatField("Time to Wake Up: ", cam.timeToStartMoving);
                cam.speedMovement = EditorGUILayout.FloatField("Speed: ", cam.speedMovement);
                EditorGUILayout.PropertyField(movementDirection);
            }
            EditorGUI.EndDisabledGroup();

            Footer();
        }

        private GUIStyle GetGUIStyle(Color color)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = color;
            style.fontStyle = FontStyle.Bold;

            return style;
        }

        private void Header(string title)
        {
            GUI.color = new Color(0.75f, 0.75f, 0.75f);
            EditorGUILayout.BeginVertical("Box");
            //GUIStyle font = new GUIStyle { fontSize = 12, fontStyle = FontStyle.Bold };
            GUILayout.Space(1);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            GUI.color = Color.white;
            GUILayout.Space(3);
            EditorGUI.indentLevel++;
        }

        private void Footer()
        {
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
    }
}
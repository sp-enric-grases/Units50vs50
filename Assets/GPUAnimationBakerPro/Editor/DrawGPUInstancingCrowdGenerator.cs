using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GPUInstancingCrowdGenerator))]
public class DrawGPUInstancingCrowdGenerator : Editor
{
    void OnSceneGUI()
    {
        GPUInstancingCrowdGenerator crowdGenerator = (GPUInstancingCrowdGenerator)target;

        if (crowdGenerator.crowdShapeType == GPUInstancingCrowdGenerator.CrowdShapeType.Rectangle)
        {
            GPUInstancingCrowdGenerator.RectangleArea rectangleArea = crowdGenerator.generateInCuboidArea;

            Handles.color = Color.green;

            Vector3 centerPos = crowdGenerator.transform.position;
            rectangleArea.width = Mathf.Clamp(rectangleArea.width, 0.0f, rectangleArea.width);
            rectangleArea.length = Mathf.Clamp(rectangleArea.length, 0.0f, rectangleArea.length);
            rectangleArea.height = Mathf.Clamp(rectangleArea.height, 0.0f, rectangleArea.height);

            Matrix4x4 localToWorldMatrix = crowdGenerator.transform.localToWorldMatrix;

            float halfWidth = rectangleArea.width / 2.0f;
            float halfLength = rectangleArea.length / 2.0f;
            float halfHeight = rectangleArea.height / 2.0f;

            Vector3 upLeftFrontLocalPoint = new Vector3(-halfWidth, halfHeight, halfLength);
            Vector3 upRightFrontLocalPoint = new Vector3(halfWidth, halfHeight, halfLength);
            Vector3 upLeftBackLocalPoint = new Vector3(-halfWidth, halfHeight, -halfLength);
            Vector3 upRightBackLocalPoint = new Vector3(halfWidth, halfHeight, -halfLength);

            Vector3 upLeftFrontWorldPoint = localToWorldMatrix.MultiplyPoint(upLeftFrontLocalPoint);
            Vector3 upRightFrontWorldPoint = localToWorldMatrix.MultiplyPoint(upRightFrontLocalPoint);
            Vector3 upRightBackWorldPoint = localToWorldMatrix.MultiplyPoint(upRightBackLocalPoint);
            Vector3 upLeftBackWorldPoint = localToWorldMatrix.MultiplyPoint(upLeftBackLocalPoint);

            Vector3[] upPointArray = new Vector3[5] { upLeftFrontWorldPoint, upRightFrontWorldPoint, upRightBackWorldPoint, upLeftBackWorldPoint, upLeftFrontWorldPoint };
            Handles.DrawPolyLine(upPointArray);

            Vector3 downLeftFrontLocalPoint = new Vector3(-halfWidth, -halfHeight, halfLength);
            Vector3 downRightFrontLocalPoint = new Vector3(halfWidth, -halfHeight, halfLength);
            Vector3 downLeftBackLocalPoint = new Vector3(-halfWidth, -halfHeight, -halfLength);
            Vector3 downRightBackLocalPoint = new Vector3(halfWidth, -halfHeight, -halfLength);

            Vector3 downLeftFrontWorldPoint = localToWorldMatrix.MultiplyPoint(downLeftFrontLocalPoint);
            Vector3 downRightFrontWorldPoint = localToWorldMatrix.MultiplyPoint(downRightFrontLocalPoint);
            Vector3 downRightBackWorldPoint = localToWorldMatrix.MultiplyPoint(downRightBackLocalPoint);
            Vector3 downLeftBackWorldPoint = localToWorldMatrix.MultiplyPoint(downLeftBackLocalPoint);
            Vector3[] downPointArray = new Vector3[5] { downLeftFrontWorldPoint, downRightFrontWorldPoint, downRightBackWorldPoint, downLeftBackWorldPoint, downLeftFrontWorldPoint };

            Handles.DrawPolyLine(downPointArray);
            Handles.DrawLine(upLeftFrontWorldPoint, downLeftFrontWorldPoint);
            Handles.DrawLine(upRightFrontWorldPoint, downRightFrontWorldPoint);
            Handles.DrawLine(upRightBackWorldPoint, downRightBackWorldPoint);
            Handles.DrawLine(upLeftBackWorldPoint, downLeftBackWorldPoint);
        }
        else
        {
            Handles.color = Color.green;
            GPUInstancingCrowdGenerator.CycleArea cycleArea = crowdGenerator.generateCycleArea;
            Vector3 centerTransformPos = crowdGenerator.transform.position;
            Vector3 centerUpDir = crowdGenerator.transform.up;
            cycleArea.innerHollowRadius = Mathf.Clamp(cycleArea.innerHollowRadius, 0.0f, cycleArea.innerHollowRadius);
            cycleArea.radius = Mathf.Clamp(cycleArea.radius, 0.0f, cycleArea.radius);
            Handles.DrawWireDisc(centerTransformPos, centerUpDir, cycleArea.innerHollowRadius);
            Handles.DrawWireDisc(centerTransformPos, centerUpDir, cycleArea.radius);
        }
 
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //base.OnInspectorGUI();

        GPUInstancingCrowdGenerator crowdGenerator = (GPUInstancingCrowdGenerator)target;

        EditorGUILayout.BeginVertical(GUI.skin.button);
        EditorGUILayout.Space();
        SerializedProperty animatePrefabProperty = serializedObject.FindProperty("animatePrefab");
        EditorGUILayout.PropertyField(animatePrefabProperty, true);
        SerializedProperty randomSpeedRangeProperty = serializedObject.FindProperty("randomSpeedRange");
        EditorGUILayout.PropertyField(randomSpeedRangeProperty, true);
        //SerializedProperty randomPlayNoiseProperty = serializedObject.FindProperty("randomPlayNoise");
        //EditorGUILayout.PropertyField(randomPlayNoiseProperty, true);
        SerializedProperty crowdShapeTypeProperty = serializedObject.FindProperty("crowdShapeType");
        EditorGUILayout.PropertyField(crowdShapeTypeProperty, true);

        if (crowdGenerator.crowdShapeType == GPUInstancingCrowdGenerator.CrowdShapeType.Rectangle)
        {
            SerializedProperty generateInCuboidAreaProperty = serializedObject.FindProperty("generateInCuboidArea");
            EditorGUILayout.PropertyField(generateInCuboidAreaProperty, true);
        }
        else if (crowdGenerator.crowdShapeType == GPUInstancingCrowdGenerator.CrowdShapeType.Cycle)
        {
            SerializedProperty generateCycleAreaProperty = serializedObject.FindProperty("generateCycleArea");
            EditorGUILayout.PropertyField(generateCycleAreaProperty, true);
        }

        SerializedProperty generateNumProperty = serializedObject.FindProperty("generateNum");
        EditorGUILayout.PropertyField(generateNumProperty, true);
        SerializedProperty castShadowProperty = serializedObject.FindProperty("castShadow");
        EditorGUILayout.PropertyField(castShadowProperty, true);
        SerializedProperty receiveShadowProperty = serializedObject.FindProperty("receiveShadow");
        EditorGUILayout.PropertyField(receiveShadowProperty, true);
     
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

}

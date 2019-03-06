using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SP.TA.Tools
{
    public class ReplacePrefabs : ScriptableWizard
    {
        static List<GameObject> replacement = new List<GameObject>();
        static List<GameObject> objectsToReplace = new List<GameObject>();
        static bool applyRotation = true;
        static Vector2 rotation = new Vector2(0, 360);
        static bool applyScale = true;
        static Vector2 scale = new Vector2(0.9f, 1.1f);
        static bool absScale = false;
        static bool delete = false;
        static bool hide = true;

        [Header("REPLACEMENT OBJECT(S)")]
        public List<GameObject> replacementObject = new List<GameObject>();

        [Space(10)]
        [Header("ROTATION PARAMS")]
        public bool applyRotationOnYAxis = true;
        public float minRotation = 0;
        public float maxRotation = 360;

        [Space(10)]
        [Header("SCALE PARAMS")]
        public bool applyRandomScale = true;
        public bool scaleIsAbsolute = false;
        public float minScale = 0.9f;
        public float maxScale = 1.1f;
   
        [Space(10)]
        [Header("OTHER PARAMS")]
        public bool deleteSelectedObjects = true;
        public bool hideSelectedObjects = false;

        [MenuItem("TA/Replace Selection...")]
        static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard("Object replacer", typeof(ReplacePrefabs), "Apply & Close", "Apply");
        }

        public ReplacePrefabs()
        {
            replacementObject = replacement;
            minRotation = rotation.x;
            maxRotation = rotation.y;
            applyRotationOnYAxis = applyRotation;
            applyRandomScale = applyScale;
            minScale = scale.x;
            maxScale = scale.y;
            scaleIsAbsolute = absScale;
            deleteSelectedObjects = delete;
            hideSelectedObjects = hide;
        }

        void OnWizardUpdate()
        {
            replacement = replacementObject;
            applyRotation = applyRotationOnYAxis;
            rotation = new Vector2(minRotation, maxRotation);
            applyScale = applyRandomScale;
            scale = new Vector2(minScale, maxScale);
            absScale = scaleIsAbsolute;
            delete = deleteSelectedObjects;
            hide = hideSelectedObjects;
        }

        void OnWizardCreate()
        {
            OnWizardOtherButton();
        }

        void OnWizardOtherButton()
        {
            Transform[] transforms = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable);

            if (transforms.Length == 0)
                EditorUtility.DisplayDialog("Object replacer", "There are no objects selected.", "OK");
            else
            {
                foreach (var item in replacement)
                    if (item != null) objectsToReplace.Add(item);

                if (objectsToReplace.Count > 0)
                    ReplaceObjectsSelected(transforms);
                else
                    ApplyRandomTransformations(transforms);

                HideCurrentObjects();
                DeleteSelectedObjects();
                CleanLists(transforms);
            }
        }

        private static void ReplaceObjectsSelected(Transform[] transforms)
        {
            foreach (Transform t in transforms)
            {
                GameObject go;
                int rnd = Random.Range(0, objectsToReplace.Count);

                PrefabType pref = PrefabUtility.GetPrefabType(objectsToReplace[rnd]);

                if (pref == PrefabType.Prefab || pref == PrefabType.ModelPrefab)
                    go = (GameObject)PrefabUtility.InstantiatePrefab(objectsToReplace[rnd]);
                else
                    go = (GameObject)Editor.Instantiate(objectsToReplace[rnd]);

                CreatePrefab(t, go, rnd);
            }
        }

        private static void CreatePrefab(Transform t, GameObject go, int rnd)
        {
            go.transform.parent = t.parent;
            go.name = objectsToReplace[rnd].name;
            go.transform.localPosition = t.localPosition;
            go.transform.localScale = t.localScale;
            ApplyRotation(go, t);
            ApplyScale(go, t);
        }

        private static void ApplyRandomTransformations(Transform[] transforms)
        {
            foreach (var item in transforms)
            {
                if (applyRotation) ApplyRotation(item.gameObject, item);
                if (applyScale) ApplyScale(item.gameObject, item);
            }
        }

        private static void ApplyRotation(GameObject go, Transform item)
        {
            go.transform.rotation = item.rotation;

            if (!applyRotation) return;

            float rot = Random.Range(rotation.x, rotation.y);
            go.transform.rotation *= Quaternion.Euler(0, rot, 0);
        }

        private static void ApplyScale(GameObject go, Transform item)
        {
            float scl = Random.Range(scale.x, scale.y);
            if (absScale)
                go.transform.localScale = new Vector3(scl, scl, scl);
            else
            {
                go.transform.localScale = item.localScale;
                go.transform.localScale *= scl;
            }
        }

        private void HideCurrentObjects()
        {
            if (hide && objectsToReplace.Count > 0)
            {
                foreach (GameObject g in Selection.gameObjects)
                    g.SetActive(false);
            }
        }

        private void DeleteSelectedObjects()
        {
            if (delete && objectsToReplace.Count > 0)
            {
                foreach (GameObject g in Selection.gameObjects)
                    GameObject.DestroyImmediate(g);
            }
        }

        private void CleanLists(Transform[] transforms)
        {
            transforms = null;
            objectsToReplace.Clear();
        }
    }
}
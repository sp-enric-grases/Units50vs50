using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;

/// <summary>
/// Helper editor class for locating object references.
/// </summary>
public class ReferenceFinder
{
    #if UNITY_EDITOR
    private const string SCENE_FILE_EXTENSION = ".unity";


    public static T[] ReturnProjectReferences<T>(UnityEngine.Object[] objs,bool flag = false) where T : UnityEngine.Object
    {
        string filter = string.Empty;
        string[] allAssetGuids = AssetDatabase.FindAssets(filter);
        List<UnityEngine.Object> foundObjects = new List<UnityEngine.Object>();
        List<T> foundTypes = new List<T>();

        int numberOfAssets = allAssetGuids.Length;
        float i = 0f;

        // Iterate over all assets in the project.
        foreach (string asset in allAssetGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(asset);

            UnityEngine.Object currentObject = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

            if (currentObject != null)
            {
                if (path.EndsWith(SCENE_FILE_EXTENSION))
                {
                    EditorBuildSettingsScene scene = EditorBuildSettings.scenes.Where(s => s.path == path).FirstOrDefault();

                    if (scene == null || scene.enabled == false)
                    {
                        continue;
                    }
                }

                UnityEngine.Object[] dependencies = EditorUtility.CollectDependencies(new[] { currentObject });

                for (int k = 0;k<objs.Length;k++)
                {
                    UnityEngine.Object obj = objs[k];
                    if (dependencies.Any(d => d == obj
                                   && d != currentObject
                                   && foundObjects.Contains(d) == false
                                   && foundObjects.Contains(currentObject) == false)
                                   && currentObject.GetType() == typeof(T))
                    {
                        foundObjects.Add(currentObject);
                        T currentType = currentObject as T;
                        foundTypes.Add(currentType);
                    }
                }

              
            }

            i++;
        }

        return foundTypes.ToArray();

    }

#endif
}
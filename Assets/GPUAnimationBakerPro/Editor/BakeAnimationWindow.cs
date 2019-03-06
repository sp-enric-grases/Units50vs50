using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

/// <summary>
/// This class is use for showing the operation window of GPU Animation Baker Pro
/// </summary>
public class BakeAnimationWindow : EditorWindow
{
    private Editor editor;

    [MenuItem("Window/Bake GPU Animation")]
    public static void ShowWindow()
    {
        // Get existing open window or if none, make a new one:
        BakeAnimationWindow window = EditorWindow.GetWindow<BakeAnimationWindow>(true, "Bake GPU Animation", true);
        window.minSize = new Vector2(500.0f, 200.0f);
        window.maxSize = new Vector2(1000.0f,400.0f);
        window.editor = Editor.CreateEditor(BakeAnimationConfiguration);
    }

    //The GPU Animation Baker configuration file located folder path
    public const string ConfigurationFilePath = "Assets/GPUAnimationBakerPro/ConfigFile/";
    //The GPU Animation Baker configuration file name
    public const string ConfigurationFileName = "ConfigurationFile.asset";
    //The baked animation texture located folder path
    public const string BakeAnimationTextureFolderPath = "Assets/GPUAnimationBakerPro/BakedGPUAnimation/";

    ///The GPU Animation Baker configuration data
    public static BakeAnimationConfigureScriptableObject bakeAnimationConfiguration;

    public static BakeAnimationConfigureScriptableObject BakeAnimationConfiguration
    {
        get
        {
            if (bakeAnimationConfiguration == null)
            {
                if (Directory.Exists(ConfigurationFilePath) == false)
                {
                    Directory.CreateDirectory(ConfigurationFilePath);
                }

                string configurationFileFullPath = Path.Combine(ConfigurationFilePath, ConfigurationFileName);

                bakeAnimationConfiguration = AssetDatabase.LoadAssetAtPath<BakeAnimationConfigureScriptableObject>(configurationFileFullPath);

                if (bakeAnimationConfiguration == null)
                {
                    BakeAnimationConfigureScriptableObject configurationFile = ScriptableObject.CreateInstance<BakeAnimationConfigureScriptableObject>();

                    UnityEditor.AssetDatabase.CreateAsset(configurationFile, configurationFileFullPath);

                    bakeAnimationConfiguration = AssetDatabase.LoadAssetAtPath<BakeAnimationConfigureScriptableObject>(configurationFileFullPath);
                }

            }

            return bakeAnimationConfiguration;
        }
    }

    void OnGUI()
    {
        if (this.editor != null)
        {
            this.editor.OnInspectorGUI();
            TransformTreeView m_TransformTreeView = BakeAnimationConfiguration.BakeTargetTransformTreeView;
            GUILayout.BeginHorizontal();
           
            if (m_TransformTreeView != null)
            {
                GUILayout.Label("Select The Root Bone:");
                Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
                m_TransformTreeView.OnGUI(rect);
            }
            
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Bake"))
            {
                if (BakeAnimationConfiguration.bakeTarget == null)
                {
                    ShowNotification(new GUIContent("No object selected for searching"));
                }
                else
                {
                    EditorApplication.isPlaying = true;
                    BakeAnimationConfiguration.bakingLock = false;
                }

            }

            GUILayout.Space(30);

            if (EditorApplication.isPlaying == true && BakeAnimationConfiguration.bakingLock == false)
            {
                BakeAnimationConfiguration.bakingLock = true;
                //start baking the animations of corresponding model
                Bake();
            }
        }

     
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }


    /// <summary>
    /// Bake the animations of corresponding model
    /// </summary>
    public void Bake()
    {
        GameObject bakeTarget = BakeAnimationConfiguration.bakeTarget;
        AnimationClip animationClip = BakeAnimationConfiguration.animClip;
        RuntimeAnimatorController runtimeAnimatorController = BakeAnimationConfiguration.RuntimeAnimatorController;
        bool rootMotionEnabled = BakeAnimationConfiguration.rootMotionEnabled;
        bool individualDifferenceEnabled = BakeAnimationConfiguration.individualDifferenceEnabled;
        Shader playAnimationShader = BakeAnimationConfiguration.AnimationShader;
        GPUAnimationShaderType shaderType = BakeAnimationConfiguration.shaderType;
        string rootBonePath = BakeAnimationConfiguration.RootBonePath;

        //if we not set the model,then the baking progress will be terminate
        if (bakeTarget == null)
        {
            ShowDialog("bakeTarget can not be null");
            EditorApplication.isPlaying = false;
            return;
        }

        //if we not set the animation clip,then the baking progress will be terminate
        if (animationClip == null)
        {
            ShowDialog("animationClip can not be null");
            EditorApplication.isPlaying = false;
            return;
        }

        SkinnedMeshRenderer skinnedMeshRenderer = bakeTarget.GetComponentInChildren<SkinnedMeshRenderer>();
        //if there is no skinnedMeshRenderer component on this model,then the baking progress will be terminate
        if (skinnedMeshRenderer == null)
        {
            ShowDialog("Cannot find SkinnedMeshRenderer.");
            EditorApplication.isPlaying = false;
            return;
        }

        if (skinnedMeshRenderer.sharedMesh == null)
        {
            ShowDialog("Cannot find SkinnedMeshRenderer.mesh.");
            EditorApplication.isPlaying = false;
            return;
        }

        //if we not set the root bone of this model,then the baking progress will be terminate
        if (rootBonePath == null || rootBonePath == "")
        {
            ShowDialog("please select the root bone");
            EditorApplication.isPlaying = false;
            return;
        }

        Animation m_Animation = bakeTarget.GetComponent<Animation>();
        Animator m_Animator = bakeTarget.GetComponent<Animator>();
        if (m_Animator == null && m_Animation == null)
        {
            ShowDialog("Cannot find Animator Or Animation Component");
            EditorApplication.isPlaying = false;
            return;
        }
        if (m_Animator != null && m_Animation != null)
        {
            ShowDialog("Animation is not coexisting with Animator");
            EditorApplication.isPlaying = false;
            return;
        }
        if (m_Animator != null && m_Animator.avatar == null)
        {
            ShowDialog("Animator's avatar field can not be null");
            EditorApplication.isPlaying = false;
            return;
        }

        //For sampling the animation, we instantiate a empty gameObject and add the GPUAnimationBakerPro_AnimationSampler sript to it
        GameObject recorder = Instantiate(bakeTarget);
        recorder.gameObject.SetActive(false);
        recorder.name = bakeTarget.name;
        recorder.transform.parent = null;
        recorder.transform.position = Vector3.zero;
        recorder.transform.eulerAngles = Vector3.zero;
      
        GPUAnimationBakerPro_AnimationSampler gpuAnimationBakerPro_AnimationSampler = recorder.AddComponent<GPUAnimationBakerPro_AnimationSampler>();
        gpuAnimationBakerPro_AnimationSampler.bakeAnimationTextureFolderPath = BakeAnimationTextureFolderPath;
        gpuAnimationBakerPro_AnimationSampler.animClip = animationClip;
        gpuAnimationBakerPro_AnimationSampler.runtimeAnimatorController = runtimeAnimatorController;
        gpuAnimationBakerPro_AnimationSampler.rootMotionEnabled = rootMotionEnabled;
        gpuAnimationBakerPro_AnimationSampler.individualDifferenceEnabled = individualDifferenceEnabled;

        Transform rootBone = null;

        if (rootBonePath == bakeTarget.name)
        {
            rootBone = recorder.transform;
        }
        else
        {
            string boneName = rootBonePath.Replace(string.Format("{0}/", bakeTarget.name), "");
            rootBone = bakeTarget.transform.Find(boneName);
        }

        //if we not set the root bone of this model,then the baking progress will be terminate
        if (rootBone == null)
        {
            ShowDialog("please select the root bone");
            Destroy(recorder);
            EditorApplication.isPlaying = false;
            return;
        }


        gpuAnimationBakerPro_AnimationSampler.rootBoneTransform = rootBone;
        gpuAnimationBakerPro_AnimationSampler.shaderType = shaderType;
        recorder.gameObject.SetActive(true);
    }


    public static void ShowDialog(string msg)
    {
        EditorUtility.DisplayDialog("GPU Animation Baking", msg, "OK");
    }
}
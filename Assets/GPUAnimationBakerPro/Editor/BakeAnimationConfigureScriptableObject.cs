using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

/// <summary>
/// The GPU Animation Baker Configuration class
/// </summary>
public class BakeAnimationConfigureScriptableObject : ScriptableObject
{
    //the model you which want to bake: 
    public GameObject bakeTarget;
    //the animation clip corresponding to the model which you want to bake:
    public AnimationClip animClip;
    //When baking animation is finished, the GPU Animaton Baker Pro will use the shader of your selected
    //to renderer the model and corresponding animation.
    public GPUAnimationShaderType shaderType = GPUAnimationShaderType.Simple;
    //When instantiate a crowd about this animation, enable this field, 
    //the movement of each individual in this crowd looks different.
    //Disable this field, the movement of each individual in this crowd will looks the same:
    public bool individualDifferenceEnabled;
    public bool rootMotionEnabled;

    [HideInInspector]
    public GameObject previousBakeTarget;

    //the root bone of the model you want to bake:
    [HideInInspector]
    public GameObject rootBoneGameObject;

    //the relative path of the Animation Controller which will be used for sampling animation
    private const string runtimeAnimatorControllerPath = "Assets/GPUAnimationBakerPro/ConfigFile/SampleAnimationController.controller";
    public RuntimeAnimatorController RuntimeAnimatorController
    {
        get
        {
            return AssetDatabase.LoadAssetAtPath(runtimeAnimatorControllerPath, typeof(UnityEngine.Object)) as RuntimeAnimatorController;
        }

    }

    //the root bone path of the model you want to bake: 
    public string RootBonePath
    {
        get
        {       
            if (rootBoneGameObject != null)
            {
                return GetPath(rootBoneGameObject);
            }
            else
            {
                return null;
            }
        }
    }

    [HideInInspector]
    public bool bakingLock = true;

    [HideInInspector]
    public TreeViewState treeViewState;
    [HideInInspector]
    public TransformTreeView transformTreeView;

    public TransformTreeView BakeTargetTransformTreeView
    {
        get
        {
            if (bakeTarget == null)
            {
                return null;
            }

            if (bakeTarget == previousBakeTarget)
            {
                if (transformTreeView == null)
                {
                    treeViewState = new TreeViewState();
                    transformTreeView = new TransformTreeView(treeViewState, bakeTarget, this);
                }

                return transformTreeView;
            }

            if (bakeTarget != previousBakeTarget)
            {
                rootBoneGameObject = null;
                treeViewState = new TreeViewState();
                transformTreeView = new TransformTreeView(treeViewState, bakeTarget, this);
                previousBakeTarget = bakeTarget;
            }
            return null;
        }
    }

    /// <summary>
    /// When baking animation is finished,
    /// the GPU Animaton Baker Pro will use the shader of your selected
    /// to renderer the model and corresponding animation
    /// </summary>
    [HideInInspector]
    public Shader AnimationShader
    {
        get
        {
            string shaderName;

            if (shaderType == GPUAnimationShaderType.StandardMetallic)
            {
                shaderName = "GPUAnimationBakerPro/StandardPBR(Metallic setup)";
            }
            else if (shaderType == GPUAnimationShaderType.StandardSpecular)
            {
                shaderName = "GPUAnimationBakerPro/StandardPBR(Specular setup)";
            }
            else
            {
                shaderName = "GPUAnimationBakerPro/Simple";
            }

            return Shader.Find(shaderName);
        }
    }

    /// <summary>
    /// get root bone path
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public string GetPath(GameObject go)
    {
        StringBuilder stringBuilder = new StringBuilder();

        List<Transform> transformList = new List<Transform>();
        Transform goTransform = go.transform;
        transformList.Add(goTransform);
        while (goTransform.parent != null)
        {
            transformList.Add(goTransform.parent);
            goTransform = goTransform.parent;
        }
        for (int i = transformList.Count - 1; i >= 0; i--)
        {
            if (i == 0)
            {
                stringBuilder.Append(string.Format("{0}", transformList[i].name));
            }
            else
            {
                stringBuilder.Append(string.Format("{0}/", transformList[i].name));
            }

        }

        return stringBuilder.ToString();
    }

}

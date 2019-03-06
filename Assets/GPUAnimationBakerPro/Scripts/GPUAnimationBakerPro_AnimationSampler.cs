using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
///This class is used for baking the gpu animation of corresponding model at runtime
/// </summary>
public class GPUAnimationBakerPro_AnimationSampler : MonoBehaviour
{
#if UNITY_EDITOR

    //the animation clip corresponding to the model which you want to bake:
    public AnimationClip animClip = null;
    //the Runtime Animation Controller which will be used for sampling animation
    public RuntimeAnimatorController runtimeAnimatorController = null;
    public bool rootMotionEnabled;
    //When instantiate a crowd about this animation, enable this field, 
    //the movement of each individual in this crowd looks different.
    //Disable this field, the movement of each individual in this crowd will looks the same:
    public bool individualDifferenceEnabled;
    //the root bone of the model
    public Transform rootBoneTransform = null;
    /// <summary>
    /// When baking animation is finished,
    /// the GPU Animaton Baker Pro will use this shader type
    /// to renderer the model and corresponding animation
    /// </summary>
    public GPUAnimationShaderType shaderType;
    //The baked animation texture located folder path
    public string bakeAnimationTextureFolderPath;

    //Whether is sampling or not now
    private bool m_IsSampling = false;

    //the Animtion component which will be used for sampling animation
    private Animation m_Animation = null;

    //the Animatior component which will be used for sampling animation
    private Animator m_Animator = null;

    //the SkinnedMeshRenderer component on the model that you want to baked
    private SkinnedMeshRenderer m_SkinnedMeshRenderer = null;
    //the gpu animation config file of the final baked gpu animation
    private GPUAnimationConfig m_GPUAnimationConfig = null;
    //the position of the root
    private Vector3 m_RootMotionPosition;
    //the rotation of the root
    private Quaternion m_RootMotionRotation;
    //the current frame index of the animation that is sampling
    private int m_SamplingFrameIndex = 0;

    private bool m_Flag = false;

    /// <summary>
    /// initialize and prepare for sampling animation
    /// </summary>
    public void StartSample()
    {
        if (m_IsSampling == true)
        {
            return;
        }

        if (animClip == null)
        {
            m_IsSampling = false;
            return;
        }

        //get the total number of frames abour the animation clip
        int numFrames = (int)(animClip.frameRate * animClip.length);
        if (numFrames == 0)
        {
            m_IsSampling = false;
            return;
        }

        m_SkinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        m_SamplingFrameIndex = 0;

        m_GPUAnimationConfig = ScriptableObject.CreateInstance<GPUAnimationConfig>();
        m_GPUAnimationConfig.animationName = animClip.name;

        List<GPUAnimationBone> gpuAnimationBoneList = new List<GPUAnimationBone>();
        CollectBones(gpuAnimationBoneList, m_SkinnedMeshRenderer.bones, m_SkinnedMeshRenderer.sharedMesh.bindposes, null, rootBoneTransform, 0);

        m_GPUAnimationConfig.bones = gpuAnimationBoneList.ToArray();
        m_GPUAnimationConfig.rootBoneIndex = 0;
        m_GPUAnimationConfig.animationName = animClip.name;
        m_GPUAnimationConfig.fps = (int)animClip.frameRate;
        m_GPUAnimationConfig.length = animClip.length;

        if (animClip.wrapMode == WrapMode.Loop || animClip.isLooping == true)
        {
            m_GPUAnimationConfig.isLoop = true;
        }
        else
        {
            m_GPUAnimationConfig.isLoop = false;
        }

        m_GPUAnimationConfig.frames = new GPUAnimationFrame[numFrames];
        m_GPUAnimationConfig.rootMotionEnabled = rootMotionEnabled;
        m_GPUAnimationConfig.individualDifferenceEnabled = individualDifferenceEnabled;

        SetCurrentAnimationClip();

        //Prepare Record Animator;
        if (m_Animator != null)
        {
            m_Animator.applyRootMotion = m_GPUAnimationConfig.rootMotionEnabled;
            m_Animator.Rebind();
            m_Animator.recorderStartTime = 0;
            m_Animator.StartRecording(numFrames);
            for (int i = 0; i < numFrames; i++)
            {
                m_Animator.Update(1.0f / m_GPUAnimationConfig.fps);
            }
            m_Animator.StopRecording();
            m_Animator.StartPlayback();
        }

        m_IsSampling = true;
    }

    /// <summary>
    /// Create AnimatorOverrideController and set to it with the animation clip which you want to sampling
    /// </summary>
    private void SetCurrentAnimationClip()
    {
        if (m_Animation == null)
        {
            AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController();
            AnimationClip[] clips = runtimeAnimatorController.animationClips;
            List<KeyValuePair<AnimationClip, AnimationClip>> animList = new List<KeyValuePair<AnimationClip, AnimationClip>>();

            for (int i = 0; i < clips.Length; i++)
            {
                AnimationClip originalClip = clips[i];
                AnimationClip overrideClip = animClip;
                KeyValuePair<AnimationClip, AnimationClip> animKV = new KeyValuePair<AnimationClip, AnimationClip>(originalClip, overrideClip);
                animList.Add(animKV);
            }
            animatorOverrideController.runtimeAnimatorController = runtimeAnimatorController;
            animatorOverrideController.ApplyOverrides(animList);
            m_Animator.runtimeAnimatorController = animatorOverrideController;
        }
    }

    /// <summary>
    ///Caculate the weight affected by bones for each vertex on mesh,and store these informaton to the UV List
    ///Then Generate the new mesh.
    /// </summary>
    private Mesh CreateNewMesh(Mesh mesh, string meshName)
    {
        Vector3[] normals = mesh.normals;
        Vector4[] tangents = mesh.tangents;
        Color[] colors = mesh.colors;
        Vector2[] uv = mesh.uv;

        Mesh newMesh = new Mesh();
        newMesh.name = meshName;
        newMesh.vertices = mesh.vertices;
        if (normals != null && normals.Length > 0)
        {
            newMesh.normals = normals;
        }
        if (tangents != null && tangents.Length > 0)
        {
            newMesh.tangents = tangents;
        }
        if (colors != null && colors.Length > 0)
        {
            newMesh.colors = colors;
        }
        if (uv != null && uv.Length > 0)
        {
            newMesh.uv = uv;
        }

        int numVertices = mesh.vertexCount;
        BoneWeight[] boneWeights = mesh.boneWeights;
        Vector4[] uv2 = new Vector4[numVertices];
        Vector4[] uv3 = new Vector4[numVertices];
        Transform[] smrBones = m_SkinnedMeshRenderer.bones;
        for (int i = 0; i < numVertices; i++)
        {
            BoneWeight boneWeight = boneWeights[i];

            BoneWeightSortData[] weights = new BoneWeightSortData[4];
            weights[0] = new BoneWeightSortData() { index = boneWeight.boneIndex0, weight = boneWeight.weight0 };
            weights[1] = new BoneWeightSortData() { index = boneWeight.boneIndex1, weight = boneWeight.weight1 };
            weights[2] = new BoneWeightSortData() { index = boneWeight.boneIndex2, weight = boneWeight.weight2 };
            weights[3] = new BoneWeightSortData() { index = boneWeight.boneIndex3, weight = boneWeight.weight3 };
            Array.Sort(weights);

            GPUAnimationBone bone0 = GetBoneByTransform(smrBones[weights[0].index]);
            GPUAnimationBone bone1 = GetBoneByTransform(smrBones[weights[1].index]);
            GPUAnimationBone bone2 = GetBoneByTransform(smrBones[weights[2].index]);
            GPUAnimationBone bone3 = GetBoneByTransform(smrBones[weights[3].index]);

            Vector4 skinData_01 = new Vector4();
            skinData_01.x = Array.IndexOf(m_GPUAnimationConfig.bones, bone0);
            skinData_01.y = weights[0].weight;
            skinData_01.z = Array.IndexOf(m_GPUAnimationConfig.bones, bone1);
            skinData_01.w = weights[1].weight;
            uv2[i] = skinData_01;

            Vector4 skinData_23 = new Vector4();
            skinData_23.x = Array.IndexOf(m_GPUAnimationConfig.bones, bone2);
            skinData_23.y = weights[2].weight;
            skinData_23.z = Array.IndexOf(m_GPUAnimationConfig.bones, bone3);
            skinData_23.w = weights[3].weight;
            uv3[i] = skinData_23;
        }
        newMesh.SetUVs(1, new List<Vector4>(uv2));
        newMesh.SetUVs(2, new List<Vector4>(uv3));

        newMesh.triangles = mesh.triangles;
        return newMesh;
    }

	private class BoneWeightSortData : System.IComparable<BoneWeightSortData>
    {
        public int index = 0;

        public float weight = 0;

        public int CompareTo(BoneWeightSortData b)
        {
            return weight > b.weight ? -1 : 1;
        }
    }

    /// <summary>
    /// collect bones and store them in a list
    /// </summary>
	private void CollectBones(List<GPUAnimationBone> gpuAnimationBoneList, Transform[] skinMeshRendererBones, Matrix4x4[] bindposes, GPUAnimationBone parentBone, Transform currentBoneTransform, int currentBoneIndex)
    {
        GPUAnimationBone currentBone = new GPUAnimationBone();
        gpuAnimationBoneList.Add(currentBone);

        currentBone.transform = currentBoneTransform;
        currentBone.name = currentBone.transform.gameObject.name;

        int currentBoneTransformIndex = Array.IndexOf(skinMeshRendererBones, currentBoneTransform);
        if (currentBoneTransformIndex == -1)
        {
            currentBone.bindpose = Matrix4x4.identity;
        }
        else
        {
            currentBone.bindpose = bindposes[currentBoneTransformIndex];
        }

        if (parentBone == null)
        {
            currentBone.parentBoneIndex = -1;
        }
        else
        {
            currentBone.parentBoneIndex = gpuAnimationBoneList.IndexOf(parentBone);
        }


        if (parentBone != null)
        {
            parentBone.childrenBonesIndices[currentBoneIndex] = gpuAnimationBoneList.IndexOf(currentBone);
        }

        int numChildren = currentBone.transform.childCount;
        if (numChildren > 0)
        {
            currentBone.childrenBonesIndices = new int[numChildren];
            for (int i = 0; i < numChildren; ++i)
            {
                CollectBones(gpuAnimationBoneList, skinMeshRendererBones, bindposes, currentBone, currentBone.transform.GetChild(i), i);
            }
        }
    }

    /// <summary>
    /// Caculate the width and height of gpu animation texture which will be generate
    /// </summary>
    private void SetSizeAboutTexture(GPUAnimationConfig gpuAnimationConfig)
    {
        int numPixels = 0;

        GPUAnimationFrame[] frames = gpuAnimationConfig.frames;
        int numFrames = frames.Length;
        numPixels += gpuAnimationConfig.bones.Length * 3 * numFrames;

        CalculateTextureSize(numPixels, out gpuAnimationConfig.textureWidth, out gpuAnimationConfig.textureHeight);
    }

    /// <summary>
    /// The  width and height of gpu animation texture must be the power of 2
    /// </summary>
    private void CalculateTextureSize(int numPixels, out int texWidth, out int texHeight)
    {
        texWidth = 1;
        texHeight = 1;
        while (true)
        {
            if (texWidth * texHeight >= numPixels) break;
            texWidth *= 2;
            if (texWidth * texHeight >= numPixels) break;
            texHeight *= 2;
        }
    }

    private void Awake()
    {
        m_Animation = GetComponent<Animation>();
        m_Animator = GetComponent<Animator>();

        if (m_Animator != null)
        {
            m_Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            //Init Transform
            transform.parent = null;
            transform.position = Vector3.zero;
            transform.eulerAngles = Vector3.zero;
            return;
        }

        if (m_Animation != null)
        {
            m_Animation.Stop();
            m_Animation.cullingType = AnimationCullingType.AlwaysAnimate;

            //Init Transform
            transform.parent = null;
            transform.position = Vector3.zero;
            transform.eulerAngles = Vector3.zero;
            return;
        }
    }

    private void Update()
    {
        if (m_Flag == false)
        {
            m_Flag = true;
            StartSample();
        }

        if (m_IsSampling == false)
        {
            return;
        }

        int totalFrams = (int)(m_GPUAnimationConfig.length * m_GPUAnimationConfig.fps);

        if (m_SamplingFrameIndex >= totalFrams)
        {
            if (m_Animator != null)
            {
                m_Animator.StopPlayback();
            }

            SetSizeAboutTexture(m_GPUAnimationConfig);
            EditorUtility.SetDirty(m_GPUAnimationConfig);
            //--Check Path
            string folderPath = Path.Combine(bakeAnimationTextureFolderPath, this.gameObject.name).Replace("\\", "/");

            if (Directory.Exists(folderPath) == false)
            {
                Directory.CreateDirectory(folderPath);
            }

            string animTexturesFolderPath = string.Format("{0}/{1}", folderPath, "AnimTextures").Replace("\\", "/");
            string animConfigsFolderPath = string.Format("{0}/{1}", folderPath, "AnimConfigs").Replace("\\", "/");
            string materialsFolderPath = string.Format("{0}/{1}", folderPath, "AnimMaterials").Replace("\\", "/");
            string meshFolderPath = string.Format("{0}/{1}", folderPath, "AnimMesh").Replace("\\", "/");
            string animPrefabFolderPath = string.Format("{0}/{1}", folderPath, "AnimPrefabs").Replace("\\", "/");
            if (Directory.Exists(animTexturesFolderPath) == false)
            {
                Directory.CreateDirectory(animTexturesFolderPath);
            }
            if (Directory.Exists(animConfigsFolderPath) == false)
            {
                Directory.CreateDirectory(animConfigsFolderPath);
            }
            if (Directory.Exists(materialsFolderPath) == false)
            {
                Directory.CreateDirectory(materialsFolderPath);
            }
            if (Directory.Exists(meshFolderPath) == false)
            {
                Directory.CreateDirectory(meshFolderPath);
            }
            if (Directory.Exists(animPrefabFolderPath) == false)
            {
                Directory.CreateDirectory(animPrefabFolderPath);
            }

            string textureName = string.Format("{0}_{1}_Texture", this.gameObject.name, m_GPUAnimationConfig.animationName);
            string animConfigName = string.Format("{0}_{1}_AnimConfig", this.gameObject.name, m_GPUAnimationConfig.animationName);
            string materialName = string.Empty;
            string prefabName = string.Empty;
            if (shaderType == GPUAnimationShaderType.StandardMetallic)
            {
                materialName = string.Format("{0}_{1}_StandardMetallic_Material", this.gameObject.name, m_GPUAnimationConfig.animationName);
                prefabName = string.Format("{0}_{1}_StandardMetallic_Prefab", this.gameObject.name, m_GPUAnimationConfig.animationName);
            }
            else if (shaderType == GPUAnimationShaderType.StandardSpecular)
            {
                materialName = string.Format("{0}_{1}_StandardSpecular_Material", this.gameObject.name, m_GPUAnimationConfig.animationName);
                prefabName = string.Format("{0}_{1}_StandardSpecular_Prefab", this.gameObject.name, m_GPUAnimationConfig.animationName);
            }
            else
            {
                materialName = string.Format("{0}_{1}_Simple_Material", this.gameObject.name, m_GPUAnimationConfig.animationName);
                prefabName = string.Format("{0}_{1}_Simple_Prefab", this.gameObject.name, m_GPUAnimationConfig.animationName);
            }
            string meshName = string.Format("{0}_Mesh", this.gameObject.name);

            string textureFullPath = string.Format("{0}/{1}.bytes", animTexturesFolderPath, textureName).Replace("\\", "/");
            string animationConfigFullPath = string.Format("{0}/{1}.asset", animConfigsFolderPath, animConfigName).Replace("\\", "/");
            string materialFullPath = string.Format("{0}/{1}.mat", materialsFolderPath, materialName).Replace("\\", "/");
            string meshFullPath = string.Format("{0}/{1}.asset", meshFolderPath, meshName).Replace("\\", "/");
            string prefabFullPath = string.Format("{0}/{1}.prefab", animPrefabFolderPath, prefabName).Replace("\\", "/");

            //generate new GPUAnimationConfig
            List<GPUAnimationConfig> existAnimationConfigList = new List<GPUAnimationConfig>();
            GPUAnimationConfig existAnimationConfig = AssetDatabase.LoadAssetAtPath<GPUAnimationConfig>(animationConfigFullPath);
            if (existAnimationConfig != null)
            {
                existAnimationConfigList.Add(existAnimationConfig);
            }
            GameObject[] referenceAnimationConfigGameObjects = null;
            if (existAnimationConfigList.Count > 0)
            {
                referenceAnimationConfigGameObjects = ReferenceFinder.ReturnProjectReferences<GameObject>(existAnimationConfigList.ToArray(), true);
            }
            AssetDatabase.CreateAsset(m_GPUAnimationConfig, animationConfigFullPath);
            if (referenceAnimationConfigGameObjects != null)
            {
                for (int i = 0; i < referenceAnimationConfigGameObjects.Length; i++)
                {
                    GameObject referenceGameObject = referenceAnimationConfigGameObjects[i];

                    if (referenceGameObject.GetComponent<GPUAnimationPlayer>() != null)
                    {
                        referenceGameObject.GetComponent<GPUAnimationPlayer>().gpuAnimationConfig = m_GPUAnimationConfig;
                    }

                }
            }

            //generate new texture
            Texture2D texture = new Texture2D(m_GPUAnimationConfig.textureWidth, m_GPUAnimationConfig.textureHeight, TextureFormat.RGBAHalf, false, true);
            Color[] pixels = texture.GetPixels();
            int pixelIndex = 0;

            GPUAnimationFrame[] frames = m_GPUAnimationConfig.frames;

            for (int frameIndex = 0; frameIndex < frames.Length; frameIndex++)
            {
                Matrix4x4[] matrices = frames[frameIndex].matrices;
                int numMatrices = matrices.Length;
                for (int matrixIndex = 0; matrixIndex < numMatrices; ++matrixIndex)
                {
                    Matrix4x4 matrix = matrices[matrixIndex];
                    pixels[pixelIndex] = new Color(matrix.m00, matrix.m01, matrix.m02, matrix.m03);
                    pixelIndex++;
                    pixels[pixelIndex] = new Color(matrix.m10, matrix.m11, matrix.m12, matrix.m13);
                    pixelIndex++;
                    pixels[pixelIndex] = new Color(matrix.m20, matrix.m21, matrix.m22, matrix.m23);
                    pixelIndex++;
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();

            using (FileStream fileStream = new FileStream(textureFullPath, FileMode.Create))
            {
                byte[] bytes = texture.GetRawTextureData();
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Flush();
                fileStream.Close();
                fileStream.Dispose();
            }

            //generate new mesh
            List<Mesh> existMeshList = new List<Mesh>();
            Mesh existMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshFullPath);
            if (existMesh != null)
            {
                existMeshList.Add(existMesh);
            }
            GameObject[] referenceMeshGameObjects = null;
            if (existMeshList.Count > 0)
            {
                referenceMeshGameObjects = ReferenceFinder.ReturnProjectReferences<GameObject>(existMeshList.ToArray());
            }

            Mesh newMesh = CreateNewMesh(m_SkinnedMeshRenderer.sharedMesh, meshName);
            AssetDatabase.CreateAsset(newMesh, meshFullPath);
            if (referenceMeshGameObjects != null)
            {
                for (int i = 0; i < referenceMeshGameObjects.Length; i++)
                {
                    GameObject referenceGameObject = referenceMeshGameObjects[i];

                    if (referenceGameObject.GetComponent<MeshFilter>() != null)
                    {
                        referenceGameObject.GetComponent<MeshFilter>().sharedMesh = newMesh;
                    }

                }
            }

            //new Material
            List<Material> existMaterialList = new List<Material>();
            Material existMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialFullPath);
            if (existMaterial != null)
            {
                existMaterialList.Add(existMaterial);
            }
            GameObject[] referenceMaterialGameObjects = null;
            if (existMaterialList.Count > 0)
            {
                referenceMaterialGameObjects = ReferenceFinder.ReturnProjectReferences<GameObject>(existMaterialList.ToArray());
            }

            Shader shader = null;
            string shaderName = string.Empty;

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


            shader = Shader.Find(shaderName);

            Material mtrl = new Material(shader);

            if (m_SkinnedMeshRenderer.sharedMaterial != null)
            {
                mtrl.CopyPropertiesFromMaterial(m_SkinnedMeshRenderer.sharedMaterial);
            }

            AssetDatabase.CreateAsset(mtrl, materialFullPath);
            if (referenceMaterialGameObjects != null)
            {
                for (int i = 0; i < referenceMaterialGameObjects.Length; i++)
                {
                    GameObject referenceGameObject = referenceMaterialGameObjects[i];

                    if (referenceGameObject.GetComponent<MeshRenderer>() != null)
                    {
                        referenceGameObject.GetComponent<MeshRenderer>().sharedMaterial = mtrl;
                    }

                }
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            string goName = string.Format("{0}_{1}_{2}", this.gameObject.name, m_GPUAnimationConfig.animationName, shaderType);
            GameObject go = new GameObject(goName);
            go.AddComponent<MeshFilter>().sharedMesh = newMesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mtrl;
            GPUAnimationPlayer gpuAnimationPlayer = go.AddComponent<GPUAnimationPlayer>();
            TextAsset textAsset = AssetDatabase.LoadAssetAtPath(textureFullPath, typeof(UnityEngine.Object)) as TextAsset;
            gpuAnimationPlayer.textureRawData = textAsset;
            gpuAnimationPlayer.gpuAnimationConfig = m_GPUAnimationConfig;
            gpuAnimationPlayer.enableInstancing = true;
            //create baked gpu animation prefab
            PrefabUtility.CreatePrefab(prefabFullPath, go);

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
            Destroy(this.gameObject);
            EditorApplication.isPlaying = false;

            m_IsSampling = false;
            return;
        }

        float time = m_GPUAnimationConfig.length * ((float)m_SamplingFrameIndex / totalFrams);
        GPUAnimationFrame frame = new GPUAnimationFrame();

        m_GPUAnimationConfig.frames[m_SamplingFrameIndex] = frame;
        frame.matrices = new Matrix4x4[m_GPUAnimationConfig.bones.Length];
        if (m_Animation == null)
        {
            m_Animator.playbackTime = time;
            m_Animator.Update(0);
        }
        else
        {
            m_Animation.Stop();
            AnimationState animState = m_Animation[animClip.name];

            if (animState != null)
            {
                animState.time = time;
                m_Animation.Sample();
                m_Animation.Play(animClip.name);
            }
            else
            {
                m_Animation.AddClip(animClip, animClip.name);
                AnimationState newAnimState = m_Animation[animClip.name];
                newAnimState.time = time;
                m_Animation.Sample();
                m_Animation.Play(animClip.name);
            }
        }
        m_IsSampling = false;
        string title = "Baking Animation To Texture";
        string info = string.Format("{0}/{1}", m_SamplingFrameIndex, totalFrams);
        EditorUtility.DisplayProgressBar(title, info, ((float)m_SamplingFrameIndex / (float)totalFrams));
        StartCoroutine(SamplingCoroutine(frame, totalFrams));
    }

    /// <summary>
    /// sampling data of current animation frame at the end of frame 
    /// </summary>
    private IEnumerator SamplingCoroutine(GPUAnimationFrame frame, int totalFrames)
    {
        yield return new WaitForEndOfFrame();

        GPUAnimationBone[] bones = m_GPUAnimationConfig.bones;

        for (int i = 0; i < bones.Length; i++)
        {
            Transform boneTransform = bones[i].transform;
            GPUAnimationBone currentBone = GetBoneByTransform(boneTransform);
            frame.matrices[i] = currentBone.bindpose;
            do
            {
                Matrix4x4 mat = Matrix4x4.TRS(currentBone.transform.localPosition, currentBone.transform.localRotation, currentBone.transform.localScale);
                frame.matrices[i] = mat * frame.matrices[i];
                if (currentBone.parentBoneIndex == -1)
                {
                    break;
                }
                else
                {
                    currentBone = bones[currentBone.parentBoneIndex];
                }
            }
            while (true);
        }

        if (m_SamplingFrameIndex == 0)
        {
            m_RootMotionPosition = bones[m_GPUAnimationConfig.rootBoneIndex].transform.localPosition;
            m_RootMotionRotation = bones[m_GPUAnimationConfig.rootBoneIndex].transform.localRotation;
        }
        else
        {
            Vector3 newPosition = bones[m_GPUAnimationConfig.rootBoneIndex].transform.localPosition;
            Quaternion newRotation = bones[m_GPUAnimationConfig.rootBoneIndex].transform.localRotation;
            Vector3 deltaPosition = newPosition - m_RootMotionPosition;
            frame.rootMotionDeltaPositionQ = Quaternion.Inverse(Quaternion.Euler(transform.forward.normalized)) * Quaternion.Euler(deltaPosition.normalized);
            frame.rootMotionDeltaPositionL = deltaPosition.magnitude;
            frame.rootMotionDeltaRotation = Quaternion.Inverse(m_RootMotionRotation) * newRotation;
            m_RootMotionPosition = newPosition;
            m_RootMotionRotation = newRotation;

            if (m_SamplingFrameIndex == 1)
            {
                m_GPUAnimationConfig.frames[0].rootMotionDeltaPositionQ = m_GPUAnimationConfig.frames[1].rootMotionDeltaPositionQ;
                m_GPUAnimationConfig.frames[0].rootMotionDeltaPositionL = m_GPUAnimationConfig.frames[1].rootMotionDeltaPositionL;
                m_GPUAnimationConfig.frames[0].rootMotionDeltaRotation = m_GPUAnimationConfig.frames[1].rootMotionDeltaRotation;
            }
        }

        m_SamplingFrameIndex++;
        m_IsSampling = true;
    }

    /// <summary>
    /// Get bone from gpu animation config file
    /// </summary>
    private GPUAnimationBone GetBoneByTransform(Transform transform)
    {
        GPUAnimationBone[] bones = m_GPUAnimationConfig.bones;
        for (int i = 0; i < bones.Length; ++i)
        {
            if (bones[i].transform == transform)
            {
                return bones[i];
            }
        }
        return null;
    }

#endif

}

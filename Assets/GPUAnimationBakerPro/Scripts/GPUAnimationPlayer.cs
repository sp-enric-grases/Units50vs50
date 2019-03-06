using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The gpu animation player class
/// </summary>
public class GPUAnimationPlayer:MonoBehaviour
{
    //the gpu animation configuration file 
    public GPUAnimationConfig gpuAnimationConfig;
    //the gpu animtion texture raw data
    public TextAsset textureRawData = null;

    //the gpu animation texture
    [HideInInspector]
    public Texture2D texture = null;

    //whether enable gpu instancing or not
    public bool enableInstancing;

    public GameObject deleteReference;

    //the shader name id of the gpu animation texture 
    private int m_ShaderPropID_GPUAnimation_TextureMatrix = -1;

    //the shader name id of the gpu animation texture size and the gpu animation pixels number per frame
    private int m_ShaderPropID_GPUAnimation_TextureSize_NumPixelsPerFrame = 0;

    //the shader name id of the gpu animation frame index 
    private int m_ShaderPorpID_GPUAnimation_FrameIndex = 0;

    //the shader name id of the gpu animation root motion matrix 
    private int m_ShaderPropID_GPUAnimtion_RootMotion = 0;

    private MeshRenderer m_MeshRenderer = null;

    private MaterialPropertyBlock m_MaterialPropertyBlock = null;

    //the timer of playing gpu animation
    private float m_Timer = 0;

    //offset the timer which be used for playing gpu animation
    private float m_TimeDiff = 0;

    //the last playing frame index of gpu animtion 
    private int m_LastPlayingFrameIndex = -1;

    //the root motion frame index of gpu animation
    private int m_RootMotionFrameIndex = -1;

    //the  cache of gpu animation texture
    public static Dictionary<TextAsset,Texture2D> m_TextureDic = new Dictionary<TextAsset, Texture2D>();
    public static Dictionary<TextAsset, int> m_TextureCounterDic = new Dictionary<TextAsset, int>();

    private void Awake()
    {
        if (deleteReference != null)
                DestroyImmediate(deleteReference);
    }

    /// <summary>
    /// Initalization
    /// </summary>
    public void Start()
    {
        m_ShaderPropID_GPUAnimation_TextureMatrix = Shader.PropertyToID("_GPUAnimation_TextureMatrix");
        m_ShaderPropID_GPUAnimation_TextureSize_NumPixelsPerFrame = Shader.PropertyToID("_GPUAnimation_TextureSize_NumPixelsPerFrame");
        m_ShaderPorpID_GPUAnimation_FrameIndex = Shader.PropertyToID("_GPUAnimation_FrameIndex");
        m_ShaderPropID_GPUAnimtion_RootMotion = Shader.PropertyToID("_GPUAnimation_RootMotion");

        //try load gpu animtion texture from cache,if there is no texture data in the cache,
        //then generate a new gpu animation texture and save it to the cache
        m_TextureDic.TryGetValue(textureRawData,out texture);
        if (texture == null)
        {
            texture = new Texture2D(gpuAnimationConfig.textureWidth, gpuAnimationConfig.textureHeight, TextureFormat.RGBAHalf, false, true);
            texture.filterMode = FilterMode.Point;
            texture.LoadRawTextureData(textureRawData.bytes);
            texture.Apply(false, true);
            m_TextureDic.Add(textureRawData, texture);
            m_TextureCounterDic[textureRawData] = 1;
        }
        else
        {
            m_TextureCounterDic[textureRawData] = m_TextureCounterDic[textureRawData] + 1;
        }

        m_MeshRenderer = this.gameObject.GetComponent<MeshRenderer>();

        m_MaterialPropertyBlock = new MaterialPropertyBlock();

        if (gpuAnimationConfig.rootMotionEnabled == true)
        {
            m_MeshRenderer.sharedMaterial.EnableKeyword("ROOTON");
        }
        else
        {
            m_MeshRenderer.sharedMaterial.DisableKeyword("ROOTON");
        }

        if (enableInstancing == true)
        {
            m_MeshRenderer.sharedMaterial.enableInstancing = true;
        }
        else
        {
            m_MeshRenderer.sharedMaterial.enableInstancing = false;
        }

        m_RootMotionFrameIndex = -1;
        m_TimeDiff = Random.Range(0, gpuAnimationConfig.length);

        m_MeshRenderer.sharedMaterial.SetTexture(m_ShaderPropID_GPUAnimation_TextureMatrix, texture);
        m_MeshRenderer.sharedMaterial.SetVector(m_ShaderPropID_GPUAnimation_TextureSize_NumPixelsPerFrame, new Vector4(gpuAnimationConfig.textureWidth, gpuAnimationConfig.textureHeight, gpuAnimationConfig.bones.Length * 3, 0));

    }

    public void Update()
    {
        if (gpuAnimationConfig == null)
        {
            return;
        }

        //Update gpu animation every frame 
        if (gpuAnimationConfig.isLoop == true)
        {           
            UpdateMaterial(Time.deltaTime, m_MeshRenderer.sharedMaterial);
            m_Timer = m_Timer + Time.deltaTime;
        }
        else
        {
            if (m_Timer >= gpuAnimationConfig.length)
            {
                m_Timer = gpuAnimationConfig.length;
                UpdateMaterial(Time.deltaTime, m_MeshRenderer.sharedMaterial);
            }
            else
            {
                UpdateMaterial(Time.deltaTime, m_MeshRenderer.sharedMaterial);
                m_Timer = m_Timer + Time.deltaTime;
                if (m_Timer > gpuAnimationConfig.length)
                {
                    m_Timer = gpuAnimationConfig.length;
                }
            }
        }

    }


    public void OnDestroy()
    {
        int referenceCounter = 0;
        m_TextureCounterDic.TryGetValue(textureRawData,out referenceCounter);
        if (referenceCounter > 0)
        {
            referenceCounter--;
        }

        if (referenceCounter == 0)
        {
            m_TextureDic.Remove(textureRawData);
            m_TextureCounterDic.Remove(textureRawData);
            Destroy(texture);
            texture = null;
        }
        else
        {
            m_TextureCounterDic[textureRawData] = referenceCounter;
        }
    }

    /// <summary>
    /// Update gpu animation
    /// </summary>
    private void UpdateMaterial(float deltaTime, Material currMaterial)
    {
        int frameIndex = GetFrameIndex();

        if(m_LastPlayingFrameIndex == frameIndex)
        {       
            return;
        }

        m_LastPlayingFrameIndex = frameIndex;
        
        GPUAnimationFrame frame = gpuAnimationConfig.frames[frameIndex];
        //set current frame index to shader
        m_MaterialPropertyBlock.SetFloat(m_ShaderPorpID_GPUAnimation_FrameIndex, frameIndex);

        //do root motion
        if (gpuAnimationConfig.rootMotionEnabled == true)
        {
            Matrix4x4 rootMotionInv = frame.RootMotionInv(gpuAnimationConfig.rootBoneIndex);
            m_MaterialPropertyBlock.SetMatrix(m_ShaderPropID_GPUAnimtion_RootMotion, rootMotionInv);
        }
        m_MeshRenderer.SetPropertyBlock(m_MaterialPropertyBlock);
 
        if (gpuAnimationConfig.rootMotionEnabled && frameIndex != m_RootMotionFrameIndex)
        {
            m_RootMotionFrameIndex = frameIndex;
            //do root motion
            Quaternion deltaRotation = frame.rootMotionDeltaPositionQ;
            Vector3 newForward = deltaRotation * transform.forward;
            Vector3 deltaPosition = newForward * frame.rootMotionDeltaPositionL;
            transform.Translate(deltaPosition, Space.World);
            transform.rotation = transform.rotation * frame.rootMotionDeltaRotation;
        }
    }

    /// <summary>
    /// Caculate current frame index of gpu animation
    /// </summary>
    private int GetFrameIndex()
    {
        int frameIndex = 0;

        float time = 0.0f;

        if (gpuAnimationConfig.individualDifferenceEnabled == true)
        {
            time = m_Timer + this.m_TimeDiff;
        }
        else
        {
            time = m_Timer;
        }

        frameIndex = (int)(time * gpuAnimationConfig.fps) % (int)(gpuAnimationConfig.length * gpuAnimationConfig.fps);

        return frameIndex;

    }

}

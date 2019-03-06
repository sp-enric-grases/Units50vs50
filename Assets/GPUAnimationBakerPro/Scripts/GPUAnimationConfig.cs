using UnityEngine;
using System.Collections;

/// <summary>
/// When baking animation is finished,
/// the GPU Animaton Baker Pro will use this shader type
/// to renderer the model and corresponding animation
/// </summary>
public enum GPUAnimationShaderType
{   
    StandardMetallic,
    StandardSpecular,
    Simple
}

/// <summary>
/// The gpu animation configuration file corresponding to it's gpu animtion texture 
/// </summary>
public class GPUAnimationConfig : ScriptableObject
{
    public string animationName = null;

    public GPUAnimationBone[] bones = null;

    //the index of root bone
    public int rootBoneIndex = 0;

    //the width of gpu animation texture
    public int textureWidth = 0;
    //the height of gpu animation texture
    public int textureHeight = 0;
    //the frame per second of this gpu animation 
    public int fps = 0;
    //the length(second) of this gpu animation
    public float length = 0.0f;

    public GPUAnimationFrame[] frames = null;

    //whether this gpu animation is loop or not
    public bool isLoop;
    public bool rootMotionEnabled = false;

    //When we instantiate a crowd about this animation, 
    //enable this Field,the movement of each individual in this crowd looks different.
    //Disable this field, the movement of each individual in this crowd will looks the same:
    public bool individualDifferenceEnabled = false;

}

/// <summary>
/// gpu animation frame class
/// </summary>
[System.Serializable]
public class GPUAnimationFrame
{
    //store all the bone matrices data
    public Matrix4x4[] matrices = null;

    public Quaternion rootMotionDeltaPositionQ;

    public float rootMotionDeltaPositionL;

    public Quaternion rootMotionDeltaRotation;

    [System.NonSerialized]
    private bool rootMotionInvInit = false;
    [System.NonSerialized]
    private Matrix4x4 rootMotionInv;

    //Get the root bone inverse matrix
    public Matrix4x4 RootMotionInv(int rootBoneIndex)
    {
        if (rootMotionInvInit == false)
        {
            rootMotionInv = matrices[rootBoneIndex].inverse;
            rootMotionInvInit = true;
        }
        return rootMotionInv;
    }
}

/// <summary>
/// gpu animation bone class
/// </summary>
[System.Serializable]
public class GPUAnimationBone
{
    [System.NonSerialized]
    public Transform transform = null;

    public Matrix4x4 bindpose;

    public int parentBoneIndex = -1;

    //the children indices of this bone
    public int[] childrenBonesIndices = null;

    [System.NonSerialized]
    public Matrix4x4 animationMatrix;

    public string name = null;

    [System.NonSerialized]
    private bool bindposeInvInit = false;
    [System.NonSerialized]
    private Matrix4x4 bindposeInv;
    //get the binepose inverse matrix
    public Matrix4x4 BindposeInv
    {
        get
        {
            if (bindposeInvInit == false)
            {
                bindposeInv = bindpose.inverse;
                bindposeInvInit = true;
            }
            return bindposeInv;
        }
    }
}

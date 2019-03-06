// Upgrade NOTE: upgraded instancing buffer 'GPUAnimationProperties0' to new syntax.
// Upgrade NOTE: upgraded instancing buffer 'GPUAnimationProperties1' to new syntax.

#ifndef GPU_ANIMATIN_BAKER_PRO_INCLUDE
#define GPU_ANIMATIN_BAKER_PRO_INCLUDE

uniform sampler2D _GPUAnimation_TextureMatrix;
uniform float3 _GPUAnimation_TextureSize_NumPixelsPerFrame;

UNITY_INSTANCING_BUFFER_START(GPUAnimationProperties0)
	UNITY_DEFINE_INSTANCED_PROP(float, _GPUAnimation_FrameIndex)
#define _GPUAnimation_FrameIndex_arr GPUAnimationProperties0
UNITY_INSTANCING_BUFFER_END(GPUAnimationProperties0)

#if defined(ROOTON)
UNITY_INSTANCING_BUFFER_START(GPUAnimationProperties1)
	UNITY_DEFINE_INSTANCED_PROP(float4x4, _GPUAnimation_RootMotion)
#define _GPUAnimation_RootMotion_arr GPUAnimationProperties1
UNITY_INSTANCING_BUFFER_END(GPUAnimationProperties1)
#endif


struct SkinVertexInput
{
    float4 vertex   : POSITION;
    half3 normal    : NORMAL;
    float2 uv0      : TEXCOORD0;
    float4 uv1      : TEXCOORD1;
	float4 uv3      : TEXCOORD2;

#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
    float2 uv2      : TEXCOORD3;
#endif

#ifdef _TANGENT_TO_WORLD
    half4 tangent   : TANGENT;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct SkinVertexMobileInput
{
	float4 vertex : POSITION;
	half3 normal    : NORMAL;
	float2 uv : TEXCOORD0;
	float4 uv2 : TEXCOORD1;
	float4 uv3 : TEXCOORD2;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

inline float4 indexToUV(float index)
{
	int row = (int)(index / _GPUAnimation_TextureSize_NumPixelsPerFrame.x);
	float col = index - row * _GPUAnimation_TextureSize_NumPixelsPerFrame.x;
	return float4(col / _GPUAnimation_TextureSize_NumPixelsPerFrame.x, row / _GPUAnimation_TextureSize_NumPixelsPerFrame.y, 0, 0);
}

inline float4x4 getMatrix(int frameStartIndex, float boneIndex)
{
	float matStartIndex = frameStartIndex + boneIndex * 3;
	float4 row0 = tex2Dlod(_GPUAnimation_TextureMatrix, indexToUV(matStartIndex));
	float4 row1 = tex2Dlod(_GPUAnimation_TextureMatrix, indexToUV(matStartIndex + 1));
	float4 row2 = tex2Dlod(_GPUAnimation_TextureMatrix, indexToUV(matStartIndex + 2));
	float4 row3 = float4(0, 0, 0, 1);
	float4x4 mat = float4x4(row0, row1, row2, row3);
	return mat;
}

inline float getFrameStartIndex()
{
	float frameIndex = UNITY_ACCESS_INSTANCED_PROP(_GPUAnimation_FrameIndex_arr, _GPUAnimation_FrameIndex);
	float frameStartIndex = frameIndex * _GPUAnimation_TextureSize_NumPixelsPerFrame.z;
	return frameStartIndex;
}


#define rootMotion UNITY_ACCESS_INSTANCED_PROP(_GPUAnimation_RootMotion_arr, _GPUAnimation_RootMotion)

inline void VertexMotionMobile1(out float4 vertex,SkinVertexMobileInput v)
{
	float frameStartIndex = getFrameStartIndex(); 
	float4x4 mat0 = getMatrix(frameStartIndex, v.uv2.x); 
	float4x4 mat1 = getMatrix(frameStartIndex, v.uv2.z); 
	float4x4 mat2 = getMatrix(frameStartIndex, v.uv3.x); 
	float4x4 mat3 = getMatrix(frameStartIndex,v. uv3.z);

#if ROOTON
	float4x4 root = rootMotion;
	vertex = mul(root, mul(mat0, v.vertex)) * v.uv2.y + \
	mul(root, mul(mat1, v.vertex)) * v.uv2.w + \
	mul(root, mul(mat2, v.vertex)) * v.uv3.y + \
	mul(root, mul(mat3, v.vertex)) * v.uv3.w;
#else
	vertex = mul(mat0, v.vertex) * v.uv2.y + \
	mul(mat1, v.vertex) * v.uv2.w + \
	mul(mat2, v.vertex) * v.uv3.y + \
	mul(mat3, v.vertex) * v.uv3.w;
#endif

}

inline void VertexMotionMobile2(out float4 vertex,out float3 normal,SkinVertexMobileInput v)
{
	float frameStartIndex = getFrameStartIndex(); 
	float4x4 mat0 = getMatrix(frameStartIndex, v.uv2.x); 
	float4x4 mat1 = getMatrix(frameStartIndex, v.uv2.z); 
	float4x4 mat2 = getMatrix(frameStartIndex, v.uv3.x); 
	float4x4 mat3 = getMatrix(frameStartIndex, v.uv3.z);

#if ROOTON
	float4x4 root = rootMotion;
	vertex = mul(root, mul(mat0, v.vertex)) * v.uv2.y + \
	mul(root, mul(mat1, v.vertex)) * v.uv2.w + \
	mul(root, mul(mat2, v.vertex)) * v.uv3.y + \
	mul(root, mul(mat3, v.vertex)) * v.uv3.w;

	float4 normalFloat4 = float4(v.normal, 0);
	normalFloat4 = mul(root, mul(mat0, normalFloat4)) * v.uv2.y + \
	mul(root, mul(mat1, normalFloat4)) * v.uv2.w + \
	mul(root, mul(mat2, normalFloat4)) * v.uv3.y + \
	mul(root, mul(mat3, normalFloat4)) * v.uv3.w;
	normal = normalFloat4.xyz;
#else
	vertex = mul(mat0, v.vertex) * v.uv2.y + \
	mul(mat1, v.vertex) * v.uv2.w + \
	mul(mat2, v.vertex) * v.uv3.y + \
	mul(mat3, v.vertex) * v.uv3.w;

	float4 normalFloat4 = float4(v.normal, 0);
	normalFloat4 = mul(mat0, normalFloat4) * v.uv2.y + \
	mul(mat1, normalFloat4) * v.uv2.w + \
	mul(mat2, normalFloat4) * v.uv3.y + \
	mul(mat3, normalFloat4) * v.uv3.w;
	normal = normalFloat4.xyz;
#endif

}

inline void VertexMotion1(out float4 vertex,SkinVertexInput v)
{
	float frameStartIndex = getFrameStartIndex(); 
	float4x4 mat0 = getMatrix(frameStartIndex, v.uv1.x); 
	float4x4 mat1 = getMatrix(frameStartIndex, v.uv1.z); 
	float4x4 mat2 = getMatrix(frameStartIndex, v.uv3.x); 
	float4x4 mat3 = getMatrix(frameStartIndex, v.uv3.z);

#if ROOTON
	float4x4 root = rootMotion;
	vertex =mul(root, mul(mat0, v.vertex)) * v.uv1.y + \
	mul(root, mul(mat1, v.vertex)) * v.uv1.w + \
	mul(root, mul(mat2, v.vertex)) * v.uv3.y + \
	mul(root, mul(mat3, v.vertex)) * v.uv3.w;
#else
	vertex = mul(mat0, v.vertex) * v.uv1.y + \
	mul(mat1, v.vertex) * v.uv1.w + \
	mul(mat2, v.vertex) * v.uv3.y + \
	mul(mat3, v.vertex) * v.uv3.w;
#endif
}

inline void VertexMotion2(out float4 vertex,out float3 normal,SkinVertexInput v)
{	
	float frameStartIndex = getFrameStartIndex(); 
	float4x4 mat0 = getMatrix(frameStartIndex, v.uv1.x); 
	float4x4 mat1 = getMatrix(frameStartIndex, v.uv1.z); 
	float4x4 mat2 = getMatrix(frameStartIndex, v.uv3.x); 
	float4x4 mat3 = getMatrix(frameStartIndex, v.uv3.z);

#if ROOTON

	float4x4 root = rootMotion;
	vertex = mul(root, mul(mat0, v.vertex)) * v.uv1.y + \
	mul(root, mul(mat1, v.vertex)) * v.uv1.w + \
	mul(root, mul(mat2, v.vertex)) * v.uv3.y + \
	mul(root, mul(mat3, v.vertex)) * v.uv3.w;

	float4 normalFloat4 = float4(v.normal, 0);
	normalFloat4 = mul(root, mul(mat0, normalFloat4)) * v.uv1.y + \
	mul(root, mul(mat1, normalFloat4)) * v.uv1.w + \
	mul(root, mul(mat2, normalFloat4)) * v.uv3.y + \
	mul(root, mul(mat3, normalFloat4)) * v.uv3.w;
	normal = normalFloat4.xyz;

#else

	vertex = mul(mat0, v.vertex) * v.uv1.y + \
	mul(mat1, v.vertex) * v.uv1.w + \
	mul(mat2, v.vertex) * v.uv3.y + \
	mul(mat3, v.vertex) * v.uv3.w;

	float4 normalFloat4 = float4(v.normal, 0);
	normalFloat4 = mul(mat0, normalFloat4) * v.uv1.y + \
	mul(mat1, normalFloat4) * v.uv1.w + \
	mul(mat2, normalFloat4) * v.uv3.y + \
	mul(mat3, normalFloat4) * v.uv3.w;
	normal = normalFloat4.xyz;

#endif

}

inline void VertexMotion3(out float4 vertex,out float3 normal,out float4 tangent,SkinVertexInput v)
{
	float frameStartIndex = getFrameStartIndex(); 
	float4x4 mat0 = getMatrix(frameStartIndex, v.uv1.x); 
	float4x4 mat1 = getMatrix(frameStartIndex, v.uv1.z); 
	float4x4 mat2 = getMatrix(frameStartIndex, v.uv3.x); 
	float4x4 mat3 = getMatrix(frameStartIndex, v.uv3.z);

#if ROOTON

	float4x4 root = rootMotion;
	vertex = mul(root, mul(mat0, v.vertex)) * v.uv1.y + \
	mul(root, mul(mat1, v.vertex)) * v.uv1.w + \
	mul(root, mul(mat2, v.vertex)) * v.uv3.y + \
	mul(root, mul(mat3, v.vertex)) * v.uv3.w;

	float4 normalFloat4 = float4(v.normal, 0);
	normalFloat4 = mul(root, mul(mat0, normalFloat4)) * v.uv1.y + \
	mul(root, mul(mat1, normalFloat4)) * v.uv1.w + \
	mul(root, mul(mat2, normalFloat4)) * v.uv3.y + \
	mul(root, mul(mat3, normalFloat4)) * v.uv3.w;
	normal = normalFloat4.xyz;

	#ifdef _TANGENT_TO_WORLD
	float4 tangentFloat4 = float4(v.tangent.xyz, 0);
	tangent = mul(root, mul(mat0, tangentFloat4)) * v.uv1.y + \
	mul(root, mul(mat1, tangentFloat4)) * v.uv1.w + \
	mul(root, mul(mat2, tangentFloat4)) * v.uv3.y + \
	mul(root, mul(mat3, tangentFloat4)) * v.uv3.w;
	#endif

#else

	vertex = mul(mat0, v.vertex) * v.uv1.y + \
	mul(mat1, v.vertex) * v.uv1.w + \
	mul(mat2, v.vertex) * v.uv3.y + \
	mul(mat3, v.vertex) * v.uv3.w;

	float4 normalFloat4 = float4(v.normal, 0);
	normalFloat4 = mul(mat0, normalFloat4) * v.uv1.y + \
	mul(mat1, normalFloat4) * v.uv1.w + \
	mul(mat2, normalFloat4) * v.uv3.y + \
	mul(mat3, normalFloat4) * v.uv3.w;
	normal = normalFloat4.xyz;

	#ifdef _TANGENT_TO_WORLD
	float4 tangentFloat4 = float4(v.tangent.xyz, 0);
	tangent = mul(mat0, tangentFloat4) * v.uv1.y + \
	mul(mat1, tangentFloat4) * v.uv1.w + \
	mul(mat2, tangentFloat4) * v.uv3.y + \
	mul(mat3, tangentFloat4) * v.uv3.w;
	#endif

#endif
	
}

#endif


#ifndef GPU_ANIMATIN_BAKER_PRO_STANDARD_CORE_FORWARD_INCLUDED
#define GPU_ANIMATIN_BAKER_PRO_STANDARD_CORE_FORWARD_INCLUDED

#if defined(UNITY_NO_FULL_STANDARD_SHADER)
#   define UNITY_STANDARD_SIMPLE 1
#endif


#include "UnityStandardConfig.cginc"

#if UNITY_STANDARD_SIMPLE
    #include "GPUAnimationBakerPro_StandardCoreForwardSimple.cginc"
    VertexOutputBaseSimple vertBase (SkinVertexInput v) { return vertForwardBaseSimple(v); }
    VertexOutputForwardAddSimple vertAdd (SkinVertexInput v) { return vertForwardAddSimple(v); }
    half4 fragBase (VertexOutputBaseSimple i) : SV_Target { return fragForwardBaseSimpleInternal(i); }
    half4 fragAdd (VertexOutputForwardAddSimple i) : SV_Target { return fragForwardAddSimpleInternal(i); }
#else
    #include "GPUAnimationBakerPro_StandardCore.cginc"
    VertexOutputForwardBase vertBase (SkinVertexInput v) { return vertForwardBase(v); }
    VertexOutputForwardAdd vertAdd (SkinVertexInput v) { return vertForwardAdd(v); }
    half4 fragBase (VertexOutputForwardBase i) : SV_Target { return fragForwardBaseInternal(i); }
    half4 fragAdd (VertexOutputForwardAdd i) : SV_Target { return fragForwardAddInternal(i); }
#endif

#endif // GPU_ANIMATIN_BAKER_PRO_STANDARD_CORE_FORWARD_INCLUDED

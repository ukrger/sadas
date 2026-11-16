Shader "TrainSummer/FoliageWind" {
 Properties {
  [MainColor] _BaseColor("Base Color",Color)=(.2,.38,.075,1)
  _Smoothness("Smoothness",Range(0,1))=.12
  _WindAmplitude("Wind amplitude",Range(0,.4))=.10
  _WindSpeed("Wind speed",Range(0,3))=.8
  _TreeHeight("Reference height",Float)=9
 }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
  Cull Off
  HLSLINCLUDE
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;half _Smoothness;float _WindAmplitude;float _WindSpeed;float _TreeHeight;
  CBUFFER_END
  float3 WindPositionWS(float3 positionOS,half windMask){
   float3 world=TransformObjectToWorld(positionOS);
   float3 origin=TransformObjectToWorld(float3(0,0,0));
   // World-space height avoids assuming FBX mesh axes or importer unit scaling.
   float weight=saturate((world.y-origin.y)/max(.1,_TreeHeight))*windMask;
   float wave=sin(_Time.y*_WindSpeed+origin.x*.37+origin.z*.19+world.y*.7);
   world.xz+=float2(1,.4)*(wave*_WindAmplitude*weight);return world;
  }
  ENDHLSL
  Pass {
   Name "ForwardLit" Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile_instancing
   #pragma multi_compile_fog
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
   #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   struct Attributes {float4 positionOS:POSITION;float3 normalOS:NORMAL;half4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct Varyings {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;half3 normalWS:TEXCOORD1;half4 color:COLOR;half fog:TEXCOORD2;half3 vertexLight:TEXCOORD3;UNITY_VERTEX_INPUT_INSTANCE_ID UNITY_VERTEX_OUTPUT_STEREO};
   Varyings Vert(Attributes input){
    Varyings output=(Varyings)0;UNITY_SETUP_INSTANCE_ID(input);UNITY_TRANSFER_INSTANCE_ID(input,output);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    output.positionWS=WindPositionWS(input.positionOS.xyz,input.color.a);output.positionCS=TransformWorldToHClip(output.positionWS);
    output.normalWS=TransformObjectToWorldNormal(input.normalOS);output.vertexLight=VertexLighting(output.positionWS,output.normalWS);output.color=input.color;output.fog=ComputeFogFactor(output.positionCS.z);return output;
   }
   half4 Frag(Varyings input,FRONT_FACE_TYPE face:FRONT_FACE_SEMANTIC):SV_Target {
    UNITY_SETUP_INSTANCE_ID(input);UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    half3 normal=NormalizeNormalPerPixel(input.normalWS)*IS_FRONT_VFACE(face,1.0,-1.0);
    InputData lighting=(InputData)0;lighting.positionWS=input.positionWS;lighting.normalWS=normal;lighting.viewDirectionWS=GetWorldSpaceNormalizeViewDir(input.positionWS);
    VertexPositionInputs positions=(VertexPositionInputs)0;positions.positionWS=input.positionWS;positions.positionCS=TransformWorldToHClip(input.positionWS);lighting.shadowCoord=GetShadowCoord(positions);lighting.fogCoord=input.fog;lighting.vertexLighting=input.vertexLight;lighting.bakedGI=SampleSH(normal);lighting.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(input.positionCS);lighting.shadowMask=half4(1,1,1,1);
    SurfaceData surface=(SurfaceData)0;surface.albedo=_BaseColor.rgb*input.color.rgb;surface.metallic=0;surface.specular=half3(.04,.04,.04);surface.smoothness=_Smoothness;surface.normalTS=half3(0,0,1);surface.occlusion=1;surface.alpha=1;
    half4 color=UniversalFragmentPBR(lighting,surface);color.rgb=MixFog(color.rgb,input.fog);return color;
   }
   ENDHLSL
  }
  Pass {
   Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"} ZWrite On ZTest LEqual ColorMask 0
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex ShadowVert
   #pragma fragment ShadowFrag
   #pragma multi_compile_instancing
   #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
   float3 _LightDirection;float3 _LightPosition;
   struct ShadowInput {float4 positionOS:POSITION;float3 normalOS:NORMAL;half4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct ShadowOutput {float4 positionCS:SV_POSITION;UNITY_VERTEX_OUTPUT_STEREO};
   ShadowOutput ShadowVert(ShadowInput input){
    ShadowOutput output=(ShadowOutput)0;UNITY_SETUP_INSTANCE_ID(input);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    float3 world=WindPositionWS(input.positionOS.xyz,input.color.a);float3 normal=TransformObjectToWorldNormal(input.normalOS);
    #if _CASTING_PUNCTUAL_LIGHT_SHADOW
     float3 lightDirection=normalize(_LightPosition-world);
    #else
     float3 lightDirection=_LightDirection;
    #endif
    output.positionCS=TransformWorldToHClip(ApplyShadowBias(world,normal,lightDirection));
    #if UNITY_REVERSED_Z
     output.positionCS.z=min(output.positionCS.z,output.positionCS.w*UNITY_NEAR_CLIP_VALUE);
    #else
     output.positionCS.z=max(output.positionCS.z,output.positionCS.w*UNITY_NEAR_CLIP_VALUE);
    #endif
    return output;
   }
   half4 ShadowFrag(ShadowOutput input):SV_Target{return 0;}
   ENDHLSL
  }
  Pass {
   Name "DepthOnly" Tags {"LightMode"="DepthOnly"} ZWrite On ColorMask 0
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex DepthVert
   #pragma fragment DepthFrag
   #pragma multi_compile_instancing
   struct DepthInput {float4 positionOS:POSITION;half4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct DepthOutput {float4 positionCS:SV_POSITION;UNITY_VERTEX_OUTPUT_STEREO};
   DepthOutput DepthVert(DepthInput input){DepthOutput output=(DepthOutput)0;UNITY_SETUP_INSTANCE_ID(input);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);output.positionCS=TransformWorldToHClip(WindPositionWS(input.positionOS.xyz,input.color.a));return output;}
   half4 DepthFrag(DepthOutput input):SV_Target{return 0;}
   ENDHLSL
  }
 }
 FallBack "Universal Render Pipeline/Lit"
}

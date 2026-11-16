Shader "Hidden/TrainSummer/SunShaft" {
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline"}
  ZTest Always ZWrite Off Cull Off
  HLSLINCLUDE
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
  float4 _SunScreen;half4 _ShaftColor;
  TEXTURE2D_X(_SummerShaftTexture);
  half4 Mask(Varyings input):SV_Target {
   UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
   float2 uv=input.texcoord;float depth=Linear01Depth(SampleSceneDepth(uv),_ZBufferParams);
   float value=step(.999,depth)*pow(saturate(1-distance(uv,_SunScreen.xy)*1.2),3);
   return half4(value,value,value,1);
  }
  half4 Blur(Varyings input):SV_Target {
   UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
   float2 uv=input.texcoord;float2 delta=(_SunScreen.xy-uv)*(.8/16.0);half value=0;
   [unroll]for(int sample=0;sample<16;sample++){value+=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv).r;uv+=delta;}
   return half4(value/16,value/16,value/16,1);
  }
  half4 Composite(Varyings input):SV_Target {
   UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
   half4 scene=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,input.texcoord);
   half shaft=SAMPLE_TEXTURE2D_X(_SummerShaftTexture,sampler_LinearClamp,input.texcoord).r;
   return half4(scene.rgb+shaft*_ShaftColor.rgb*_SunScreen.z,scene.a);
  }
  ENDHLSL
  Pass {
   Name "Sky mask"
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex Vert
   #pragma fragment Mask
   ENDHLSL
  }
  Pass {
   Name "Radial blur"
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex Vert
   #pragma fragment Blur
   ENDHLSL
  }
  Pass {
   Name "Composite"
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex Vert
   #pragma fragment Composite
   ENDHLSL
  }
 }
 Fallback Off
}

Shader "TrainSummer/SummerSky" {
 Properties {
  _Daylight("Daylight",Range(0,1))=1
  _SunDirection("Sun direction",Vector)=(0,1,0,0)
  _MoonDirection("Moon direction",Vector)=(0,-1,0,0)
 }
 SubShader {
  Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline"}
  Cull Off ZWrite Off
  Pass {
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
    float _Daylight;float4 _SunDirection;float4 _MoonDirection;
   CBUFFER_END
   struct Attributes {float4 positionOS:POSITION;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct Varyings {float4 positionCS:SV_POSITION;float3 direction:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
   Varyings vert(Attributes input){Varyings o;UNITY_SETUP_INSTANCE_ID(input);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.positionCS=TransformObjectToHClip(input.positionOS.xyz);o.direction=input.positionOS.xyz;return o;}
   float hash(float3 p){p=frac(p*.1031);p+=dot(p,p.yzx+33.33);return frac((p.x+p.y)*p.z);}
   float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(float3(i,0)),hash(float3(i+float2(1,0),0)),f.x),lerp(hash(float3(i+float2(0,1),0)),hash(float3(i+1,0)),f.x),f.y);}
   half4 frag(Varyings i):SV_Target {
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float3 d=normalize(i.direction);float elevation=saturate(d.y);float day=saturate(_Daylight);
    float3 horizon=lerp(float3(.018,.027,.055),float3(.58,.77,.95),day);
    float3 zenith=lerp(float3(.002,.006,.023),float3(.12,.38,.80),day);
    float3 color=lerp(horizon,zenith,pow(elevation,.5));
    float dusk=saturate(1-abs(_SunDirection.y)*7)*smoothstep(-.12,.03,_SunDirection.y);
    float towardSun=pow(saturate(dot(normalize(float3(d.x,.08,d.z)),normalize(float3(_SunDirection.x,.08,_SunDirection.z)))),5);
    color+=float3(.65,.22,.04)*dusk*pow(1-elevation,4)*towardSun;
    float sunDot=dot(d,normalize(_SunDirection.xyz));
    color+=float3(5,4.1,2.8)*smoothstep(.99995,.999975,sunDot)*smoothstep(-.02,.015,d.y);
    color+=float3(.26,.16,.065)*pow(saturate(sunDot),180)*day;
    float moonDot=dot(d,normalize(_MoonDirection.xyz));float moon=smoothstep(.99976,.99984,moonDot)*saturate(1-day*3);
    color+=float3(.65,.72,.88)*moon*(.7+.3*noise(d.xz*1700));
    float3 cell=floor(d*340);float random=hash(cell);float3 starPoint=float3(hash(cell+3),hash(cell+11),hash(cell+23));
    float stars=step(.994,random)*(1-smoothstep(.07,.18,length(frac(d*340)-starPoint)));
    color+=stars*float3(.8,.88,1)*saturate(1-day*5)*smoothstep(.02,.2,d.y)*(1-moon);
    // A few thin, stationary fair-weather clouds. No weather transitions.
    float2 cloudUV=d.xz/max(.18,d.y)*2.2;
    float cloudNoise=noise(cloudUV)*.58+noise(cloudUV*2.1)*.28+noise(cloudUV*4.1)*.14;
    float clouds=smoothstep(.66,.79,cloudNoise)*smoothstep(.07,.25,d.y)*.55;
    color=lerp(color,lerp(float3(.045,.055,.08),float3(.93,.91,.84),day),clouds);
    return half4(max(color,0),1);
   }
   ENDHLSL
  }
 }
 Fallback Off
}

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace TrainSummer {
// Optional URP 14 Forward renderer feature. Screen-space shafts, not volumetric fog.
public sealed class SunShaftFeature:ScriptableRendererFeature {
 public Shader shader;[Range(0,.5f)]public float intensity=.12f;
 ShaftPass pass;Material material;
 public override void Create(){pass?.Dispose();CoreUtils.Destroy(material);material=shader!=null?CoreUtils.CreateEngineMaterial(shader):null;pass=new ShaftPass(material){renderPassEvent=RenderPassEvent.BeforeRenderingPostProcessing};}
 bool Eligible(in RenderingData data)=>material!=null && intensity>0 && data.cameraData.cameraType==CameraType.Game && data.cameraData.renderType==CameraRenderType.Base && data.cameraData.postProcessEnabled && RenderSettings.sun!=null && RenderSettings.sun.intensity>.2f;
 public override void SetupRenderPasses(ScriptableRenderer renderer,in RenderingData data){if(Eligible(data))pass.Setup(renderer.cameraColorTargetHandle,intensity);}
 public override void AddRenderPasses(ScriptableRenderer renderer,ref RenderingData data){if(Eligible(data))renderer.EnqueuePass(pass);}
 protected override void Dispose(bool disposing){pass?.Dispose();CoreUtils.Destroy(material);}
 sealed class ShaftPass:ScriptableRenderPass {
  readonly Material material;readonly ProfilingSampler sampler=new ProfilingSampler("Summer sun shafts");
  RTHandle source,copy,mask,blur;float intensity;
  public ShaftPass(Material m){material=m;ConfigureInput(ScriptableRenderPassInput.Depth|ScriptableRenderPassInput.Color);}
  public void Setup(RTHandle target,float value){source=target;intensity=value;}
  public override void OnCameraSetup(CommandBuffer cmd,ref RenderingData data){
   var desc=data.cameraData.cameraTargetDescriptor;desc.depthBufferBits=0;desc.msaaSamples=1;desc.bindMS=false;desc.useMipMap=false;desc.autoGenerateMips=false;
   RenderingUtils.ReAllocateIfNeeded(ref copy,desc,FilterMode.Bilinear,TextureWrapMode.Clamp,name:"SummerShaftScene");
   desc.width=Mathf.Max(1,desc.width/2);desc.height=Mathf.Max(1,desc.height/2);
   RenderingUtils.ReAllocateIfNeeded(ref mask,desc,FilterMode.Bilinear,TextureWrapMode.Clamp,name:"SummerShaftMask");RenderingUtils.ReAllocateIfNeeded(ref blur,desc,FilterMode.Bilinear,TextureWrapMode.Clamp,name:"SummerShaftBlur");
  }
  public override void Execute(ScriptableRenderContext context,ref RenderingData data){
   var camera=data.cameraData.camera;var light=RenderSettings.sun;
   if(source==null || light==null || material==null)return;
   Vector3 point=camera.WorldToViewportPoint(camera.transform.position-light.transform.forward*1000);
   if(point.z<=0 || point.x<-.2f || point.x>1.2f || point.y<-.2f || point.y>1.2f)return;
   material.SetVector("_SunScreen",new Vector4(point.x,point.y,intensity,0));material.SetColor("_ShaftColor",light.color);
   var cmd=CommandBufferPool.Get();using(new ProfilingScope(cmd,sampler)){
    Blitter.BlitCameraTexture(cmd,source,copy);Blitter.BlitCameraTexture(cmd,copy,mask,material,0);Blitter.BlitCameraTexture(cmd,mask,blur,material,1);
    cmd.SetGlobalTexture("_SummerShaftTexture",blur.nameID);Blitter.BlitCameraTexture(cmd,copy,source,material,2);
   }
   context.ExecuteCommandBuffer(cmd);CommandBufferPool.Release(cmd);
  }
  public void Dispose(){copy?.Release();mask?.Release();blur?.Release();}
 }
}
}

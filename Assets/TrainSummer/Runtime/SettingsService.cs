using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace TrainSummer {
[DefaultExecutionOrder(-400)]public sealed class SettingsService:MonoBehaviour {
 public FirstPersonMotor player;public Camera view;public static GameSettings Current {get;private set;}=new GameSettings();
 int oldMipmap,oldVsync,oldFps;float oldLod;bool initialized;
 UniversalRenderPipelineAsset pipeline;RenderPipelineAsset oldGraphics,oldQuality;
 public string Error {get;private set;}="";
 string PathFor=>Path.Combine(Application.persistentDataPath,"settings_v1.json");
 void Awake(){
  Current=new GameSettings{width=Screen.width,height=Screen.height};
  try{if(File.Exists(PathFor)){var data=JsonUtility.FromJson<GameSettings>(File.ReadAllText(PathFor));if(data!=null && data.schema==1)Current=data;}}
  catch(Exception ex){Debug.LogWarning("Using default settings: "+ex.Message);}
  oldMipmap=QualitySettings.globalTextureMipmapLimit;oldVsync=QualitySettings.vSyncCount;oldFps=Application.targetFrameRate;oldLod=QualitySettings.lodBias;
  oldGraphics=GraphicsSettings.renderPipelineAsset;oldQuality=QualitySettings.renderPipeline;initialized=true;
  var source=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
  if(source!=null){pipeline=Instantiate(source);pipeline.name="Runtime URP Settings";GraphicsSettings.renderPipelineAsset=pipeline;QualitySettings.renderPipeline=pipeline;}
  Apply(!Application.isEditor);
 }
 public void Preset(int level){
  var s=Current;s.quality=Mathf.Clamp(level,0,2);s.textureLimit=level==0?1:0;s.shadowResolution=level==0?1024:(level==1?2048:4096);s.shadowDistance=level==0?45:(level==1?80:110);s.msaa=level==0?1:(level==1?2:4);s.renderScale=level==0?.85f:1;s.postProcessing=level>0;s.drawDistance=level==0?200:300;Apply(false);
 }
 public void Apply(bool screen){
  var s=Current;s.quality=Mathf.Clamp(s.quality,0,2);s.width=Mathf.Clamp(s.width,640,7680);s.height=Mathf.Clamp(s.height,480,4320);s.sensitivity=AtomicFileStore.Range(s.sensitivity,.03f,.5f)?s.sensitivity:.12f;s.renderScale=AtomicFileStore.Range(s.renderScale,.7f,1.2f)?s.renderScale:1;s.shadowDistance=AtomicFileStore.Range(s.shadowDistance,20,150)?s.shadowDistance:80;s.drawDistance=AtomicFileStore.Range(s.drawDistance,150,300)?s.drawDistance:300;
  s.textureLimit=Mathf.Clamp(s.textureLimit,0,2);s.shadowResolution=s.shadowResolution==1024 || s.shadowResolution==4096?s.shadowResolution:2048;s.msaa=s.msaa==1 || s.msaa==4?s.msaa:2;s.fpsLimit=s.fpsLimit==0?0:Mathf.Clamp(s.fpsLimit,30,240);s.screenMode=Mathf.Clamp(s.screenMode,0,2);
  Localizer.SetLanguage(s.english);QualitySettings.globalTextureMipmapLimit=s.textureLimit;QualitySettings.vSyncCount=s.vSync?1:0;Application.targetFrameRate=s.fpsLimit==0?-1:s.fpsLimit;
  QualitySettings.lodBias=s.quality==0?.7f:(s.quality==2?1.5f:1);
  if(pipeline!=null){pipeline.renderScale=s.renderScale;pipeline.shadowDistance=s.shadowDistance;pipeline.mainLightShadowmapResolution=s.shadowResolution;pipeline.msaaSampleCount=s.msaa;}
  if(player!=null)player.sensitivity=s.sensitivity;
  if(view!=null){view.farClipPlane=s.drawDistance;view.GetUniversalAdditionalCameraData().renderPostProcessing=s.postProcessing;}
  if(screen){s.width=Mathf.Clamp(s.width,640,7680);s.height=Mathf.Clamp(s.height,480,4320);Screen.SetResolution(s.width,s.height,s.screenMode==0?FullScreenMode.Windowed:(s.screenMode==1?FullScreenMode.FullScreenWindow:FullScreenMode.ExclusiveFullScreen));}
 }
 public bool Save(){try{AtomicFileStore.Write(PathFor,JsonUtility.ToJson(Current,true));Error="";return true;}catch(Exception ex){Error=ex.Message;return false;}}
 void OnDestroy(){if(!initialized)return;QualitySettings.globalTextureMipmapLimit=oldMipmap;QualitySettings.vSyncCount=oldVsync;QualitySettings.lodBias=oldLod;Application.targetFrameRate=oldFps;if(pipeline!=null){GraphicsSettings.renderPipelineAsset=oldGraphics;QualitySettings.renderPipeline=oldQuality;Destroy(pipeline);}}
}
}

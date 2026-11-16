using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainSummer {
[DefaultExecutionOrder(-300)]public sealed class GameMenu:MonoBehaviour {
 public SettingsService settings;public SessionSave saves;public static bool Paused {get;private set;}
 Vector2 scroll;string message="";Resolution[] resolutions;int resolutionIndex;
 void Awake(){Paused=false;Time.timeScale=1;var unique=new System.Collections.Generic.List<Resolution>();foreach(var r in Screen.resolutions)if(!unique.Exists(x=>x.width==r.width && x.height==r.height))unique.Add(r);resolutions=unique.ToArray();}
 void Update(){
  var k=Keyboard.current;if(k==null)return;
  if(k.escapeKey.wasPressedThisFrame)SetPaused(!Paused);
  if(!Paused && k.f5Key.wasPressedThisFrame){saves.Save();message=saves.LastMessage;}
  if(!Paused && k.f9Key.wasPressedThisFrame){saves.Load();message=saves.LastMessage;}
 }
 void SetPaused(bool paused){Paused=paused;Time.timeScale=paused?0:1;Cursor.lockState=paused?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=paused;AudioListener.pause=paused;}
 void OnDestroy(){Paused=false;Time.timeScale=1;AudioListener.pause=false;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
 string T(string key)=>Localizer.Text(key);
 void Row(string name,string value,System.Action click){GUILayout.BeginHorizontal();GUILayout.Label(T(name),GUILayout.Width(270));if(GUILayout.Button(value))click();GUILayout.EndHorizontal();}
 float Slider(string name,float value,float min,float max){GUILayout.Label(T(name)+": "+value.ToString("0.00"));return GUILayout.HorizontalSlider(value,min,max);}
 void OnGUI(){
  if(!Paused){if(!string.IsNullOrEmpty(message))GUI.Label(new Rect(18,Screen.height-55,600,25),message);return;}
  float width=Mathf.Min(720,Screen.width-24);var rect=new Rect((Screen.width-width)/2,12,width,Screen.height-24);
  GUI.Box(rect,T("title"));GUILayout.BeginArea(new Rect(rect.x+16,rect.y+30,rect.width-32,rect.height-42));scroll=GUILayout.BeginScrollView(scroll);
  GUILayout.BeginHorizontal();if(GUILayout.Button(T("resume")))SetPaused(false);
  if(GUILayout.Button(T("save"))){saves.Save();message=saves.LastMessage;}
  if(GUILayout.Button(T("load"))){saves.Load();message=saves.LastMessage;}
  if(GUILayout.Button(T("autoLoad"))){saves.Load(true);message=saves.LastMessage;}GUILayout.EndHorizontal();
  if(GUILayout.Button(T("newGame"))){if(saves.NewGame())SetPaused(false);}
  GUILayout.Label(message);var s=SettingsService.Current;
  Row("language",s.english?"English":"Русский",()=>{s.english=!s.english;settings.Apply(false);});
  Row("preset",new[]{T("low"),T("medium"),T("high")}[Mathf.Clamp(s.quality,0,2)],()=>settings.Preset((s.quality+1)%3));
  Row("textures",new[]{T("full"),"1/2","1/4"}[s.textureLimit],()=>s.textureLimit=(s.textureLimit+1)%3);
  Row("shadows",s.shadowResolution.ToString(),()=>s.shadowResolution=s.shadowResolution==1024?2048:(s.shadowResolution==2048?4096:1024));
  s.shadowDistance=Slider("shadowDistance",s.shadowDistance,20,150);s.drawDistance=Slider("drawDistance",s.drawDistance,150,300);s.renderScale=Slider("renderScale",s.renderScale,.7f,1.2f);
  Row("aa",s.msaa==1?T("off"):s.msaa+"x MSAA",()=>s.msaa=s.msaa==1?2:(s.msaa==2?4:1));
  s.postProcessing=GUILayout.Toggle(s.postProcessing,T("post"));s.showFps=GUILayout.Toggle(s.showFps,T("fps"));s.vSync=GUILayout.Toggle(s.vSync,"V-Sync");
  Row("fpsLimit",s.fpsLimit==0?T("unlimited"):s.fpsLimit.ToString(),()=>s.fpsLimit=s.fpsLimit==0?60:(s.fpsLimit==60?120:(s.fpsLimit==120?144:(s.fpsLimit==144?240:0))));
  GUILayout.Label(T("vsyncNote"));s.sensitivity=Slider("sensitivity",s.sensitivity,.03f,.5f);
  Row("screen",new[]{T("windowed"),T("borderless"),T("exclusive")}[s.screenMode],()=>s.screenMode=(s.screenMode+1)%3);
  Row("resolution",s.width+" × "+s.height,()=>{if(resolutions.Length>0){resolutionIndex=(resolutionIndex+1)%resolutions.Length;s.width=resolutions[resolutionIndex].width;s.height=resolutions[resolutionIndex].height;}});
  if(GUILayout.Button(T("apply"))){settings.Apply(true);message=settings.Save()?T("settingsSaved"):settings.Error;}
  GUILayout.Label(T("keys"));GUILayout.EndScrollView();GUILayout.EndArea();
 }
}
}

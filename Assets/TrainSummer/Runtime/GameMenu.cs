using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainSummer {
[DefaultExecutionOrder(-300)]public sealed class GameMenu:MonoBehaviour {
 enum ScreenPage {None,Start,Pause,Settings,Death,Victory}
 public PrototypeUI ui;public SettingsService settings;public SessionSave saves;public ExpeditionSession expedition;
 public bool showStartScreen=true;public static bool Paused {get;private set;}
 bool hasSession;ScreenPage page,returnPage;Vector2 scroll;string message="";Resolution[] resolutions;int resolutionIndex;float messageUntil;
 string T(string key)=>Localizer.Text(key);
 string L(string ru,string en)=>Localizer.English?en:ru;
 void Awake(){Paused=false;Time.timeScale=1;AudioListener.pause=false;var unique=new System.Collections.Generic.List<Resolution>();foreach(var r in Screen.resolutions)if(!unique.Exists(x=>x.width==r.width && x.height==r.height))unique.Add(r);resolutions=unique.ToArray();}
 void Start(){SetPage(showStartScreen?ScreenPage.Start:ScreenPage.None);}
 void Update(){
  if(page==ScreenPage.None){
   if(!ui.vitals.Alive){ui.CloseInventory();SetPage(ScreenPage.Death);return;}
   if(expedition!=null && expedition.Won){ui.CloseInventory();saves.Save(true);SetPage(ScreenPage.Victory);return;}
  }
  var k=Keyboard.current;if(k==null)return;
  if(k.escapeKey.wasPressedThisFrame){if(page==ScreenPage.None)SetPage(ScreenPage.Pause);else if(page==ScreenPage.Pause)SetPage(ScreenPage.None);else if(page==ScreenPage.Settings)SetPage(returnPage);}
  if(page==ScreenPage.None && k.f5Key.wasPressedThisFrame){saves.Save();ShowMessage(saves.LastMessage);}
  if(page==ScreenPage.None && k.f9Key.wasPressedThisFrame)Load(false);
 }
 void ShowMessage(string value){message=value;messageUntil=Time.unscaledTime+5;}
 void SetPage(ScreenPage value){
  page=value;Paused=value!=ScreenPage.None;Time.timeScale=Paused?0:1;if(Paused)ui.player.GetComponent<WeaponPresentation>()?.ResetView();
  bool cursor=Paused || ui.InventoryOpen;Cursor.lockState=cursor?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=cursor;AudioListener.pause=Paused;if(!Paused)ui.player.SuppressLook();
 }
 void Load(bool auto){if(saves.Load(auto)){hasSession=true;ShowMessage(saves.LastMessage);SetPage(ui.vitals.Alive?ScreenPage.None:ScreenPage.Death);}else ShowMessage(saves.LastMessage);}
 void Restart(){bool automatic=expedition.Model.Automatic;if(saves.NewGame()){hasSession=true;expedition.Model.Automatic=automatic;message="";SetPage(ScreenPage.None);}else ShowMessage(L("Не удалось начать маршрут.","Could not start the route."));}
 void OnDestroy(){Paused=false;Time.timeScale=1;AudioListener.pause=false;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
 void Row(string name,string value,System.Action click){GUILayout.BeginHorizontal();GUILayout.Label(T(name),GUILayout.Width(270));if(GUILayout.Button(value))click();GUILayout.EndHorizontal();}
 float Slider(string name,float value,float min,float max){GUILayout.Label(T(name)+": "+value.ToString("0.00"));return GUILayout.HorizontalSlider(value,min,max);}
 void OnGUI(){
  if(!Paused){if(Time.unscaledTime<messageUntil)GUI.Label(new Rect(18,Screen.height-90,700,25),message);return;}
  float width=Mathf.Min(720,Screen.width-24);var rect=new Rect((Screen.width-width)/2,12,width,Screen.height-24);
  string title=page==ScreenPage.Start?"TRAIN SUMMER":page==ScreenPage.Death?L("ВЫ НЕ ВЕРНУЛИСЬ","YOU DID NOT RETURN"):page==ScreenPage.Victory?L("СВЯЗЬ ВОССТАНОВЛЕНА","RADIO LINK RESTORED"):T("title");
  GUI.Box(rect,title);GUILayout.BeginArea(new Rect(rect.x+16,rect.y+35,rect.width-32,rect.height-48));scroll=GUILayout.BeginScrollView(scroll);
  if(page!=ScreenPage.Settings){
   GUILayout.Label(L("Ясное лето. Четыре станции. Последний поезд.","Clear summer skies. Four stations. The last train."));GUILayout.Space(12);
   if(page==ScreenPage.Victory)GUILayout.Label(L("Четыре радиомодуля доставлены. Маршрут завершён.","All four radio relays delivered. The route is complete."));
   else if(page==ScreenPage.Death)GUILayout.Label(L("Загрузи сохранение или начни маршрут заново.","Load your progress or start a new route."));
   else GUILayout.Label(L("Забери радиомодуль из рации в каждом станционном доме, пополняй припасы и доведи поезд до конца маршрута.","Recover a relay from each station house radio, replenish supplies and take the train to the route's end."));
   if((page==ScreenPage.Pause || (page==ScreenPage.Start && hasSession && ui.vitals.Alive && !expedition.Won)) && GUILayout.Button(T("resume"),GUILayout.Height(38)))SetPage(ScreenPage.None);
   if(page!=ScreenPage.Victory){
    if(GUILayout.Button(T("load"),GUILayout.Height(34)))Load(false);
    if(GUILayout.Button(T("autoLoad"),GUILayout.Height(34)))Load(true);
   }
   if(page==ScreenPage.Pause && GUILayout.Button(T("save"))){saves.Save();ShowMessage(saves.LastMessage);}
   if(GUILayout.Button(page==ScreenPage.Start?L("Начать маршрут","Start route"):T("newGame"),GUILayout.Height(38)))Restart();
   if(GUILayout.Button(L("Настройки","Settings"))){returnPage=page;SetPage(ScreenPage.Settings);}
   if(page!=ScreenPage.Start && GUILayout.Button(L("В главное меню","Main menu"))){ui.CloseInventory();SetPage(ScreenPage.Start);}
   if(GUILayout.Button(L("Выйти из игры","Quit game"))){if(Application.isEditor)ShowMessage(L("В редакторе останови Play.","Stop Play in the Unity editor."));else Application.Quit();}
   GUILayout.Space(12);GUILayout.Label(message);
   GUILayout.Label(L("E — взаимодействие · Tab — рюкзак · 1/2/3/4 — оружие · Esc — пауза", "E interact · Tab backpack · 1/2/3/4 weapons · Esc pause"));
  }else{
   if(GUILayout.Button(L("Назад","Back")))SetPage(returnPage);
   var s=SettingsService.Current;
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

   if(expedition!=null)expedition.Model.Automatic=GUILayout.Toggle(expedition.Model.Automatic,L("Автоотправление после ожидания (только с игроком на борту)","Automatic departure after waiting (only with the player aboard)"));
   GUILayout.Label(T("keys"));GUILayout.Label(message);
  }
  GUILayout.EndScrollView();GUILayout.EndArea();
 }
}
}

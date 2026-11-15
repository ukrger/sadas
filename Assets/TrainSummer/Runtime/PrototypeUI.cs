using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainSummer {
public sealed class PrototypeUI:MonoBehaviour {
 public FirstPersonMotor player; public TrainMotor train;
 public Transform cab;
 FuelPickup target; bool english,showFps=true; float smooth;
 string T(string ru,string en)=>english?en:ru;
 void Update(){
  smooth=Mathf.Lerp(smooth,Time.unscaledDeltaTime,0.08f);
  var k=Keyboard.current;if(k==null)return;
  if(k.f1Key.wasPressedThisFrame)english=!english;
  if(k.f2Key.wasPressedThisFrame)showFps=!showFps;
  target=null;
  if(!player.Locked)return;
  if(Physics.Raycast(player.view.transform.position,player.view.transform.forward,out var hit,3f,~(1<<player.gameObject.layer),QueryTriggerInteraction.Ignore))target=hit.collider.GetComponentInParent<FuelPickup>();
  if(k.eKey.wasPressedThisFrame){
   if(target!=null)target.Collect(train);
   else if(train.PlayerAboard && Vector3.Distance(player.transform.position,cab.position)<3)train.ToggleBrake();
  }
 }
 void OnGUI(){
  GUI.Box(new Rect(16,16,460,185),T("ЛЕТНИЙ МАРШРУТ — технический прототип","SUMMER ROUTE — technical prototype"));
  GUI.Label(new Rect(28,44,400,25),T("Скорость / Топливо: ","Speed / Fuel: ")+ $"{train.Speed:0.0} m/s / {train.Fuel:0.0}");
  GUI.Label(new Rect(28,70,400,25),T("WASD — движение; мышь — обзор; Space — прыжок","WASD move; mouse look; Space jump"));
  GUI.Label(new Rect(28,95,400,25),T("E — топливо / управление у пульта; Esc — курсор","E collect / cab control; Esc cursor"));
  GUI.Label(new Rect(28,120,430,25),"F1 RU/EN · F2 FPS");
  GUI.Label(new Rect(28,145,430,25),T("До остановки: ","Next stop: ")+Mathf.Max(0,train.NextStopDistance-train.Distance).ToString("0")+" m");
  if(showFps)GUI.Label(new Rect(Screen.width-100,16,100,25),$"{1f/Mathf.Max(smooth,0.001f):0} FPS");
  GUI.Label(new Rect(Screen.width/2-5,Screen.height/2-12,30,30),"+");
  string prompt=target!=null?T("E — взять топливо","E — collect fuel"):
   train.Finished?T("Конец тестового маршрута.","End of the test route."):
   train.Parked?T("Топливо в доме за платформой. Вернись к пульту.","Fuel is in the house beyond the platform. Return to the cab."):"";
  GUI.Label(new Rect(Screen.width/2-240,Screen.height-70,600,35),prompt);
 }
}
}

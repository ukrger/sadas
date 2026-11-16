using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainSummer {
[DefaultExecutionOrder(200)]
public sealed class PrototypeUI:MonoBehaviour {
 public WeaponPresentation presentation;public ExpeditionSession expedition;public RouteChunks world;public VegetationRenderer vegetation;
 public PlayerVitals vitals;public InventoryMatView matView;
 public bool InventoryOpen=>inventoryOpen;
 public void OpenInventory(){if(vitals!=null && !vitals.Alive)return;presentation?.ResetView();inventoryOpen=true;if(matView!=null)matView.SetOpen(true);}public void CloseInventory(){inventoryOpen=false;if(matView!=null)matView.SetOpen(false);}
 public FirstPersonMotor player;public TrainMotor train;public Transform cab;public ResourceInventory inventory;
 readonly List<MonoBehaviour> candidates=new List<MonoBehaviour>(8);
 IInteractable target;bool inventoryOpen,showHelp,showWorldStats;
 bool english=>Localizer.English;bool showFps=>SettingsService.Current.showFps;float smooth,statusUntil;string status="";
 string T(string ru,string en)=>english?en:ru;
 void Update(){
  if(GameMenu.Paused)return;
  smooth=Mathf.Lerp(smooth,Time.unscaledDeltaTime,.08f);var k=Keyboard.current;if(k==null)return;
  if(k.f1Key.wasPressedThisFrame){SettingsService.Current.english=!english;Localizer.SetLanguage(SettingsService.Current.english);}
  if(k.f2Key.wasPressedThisFrame)SettingsService.Current.showFps=!showFps;
  if(k.f3Key.wasPressedThisFrame)showHelp=!showHelp;
  if(k.f4Key.wasPressedThisFrame)showWorldStats=!showWorldStats;
  if(k.tabKey.wasPressedThisFrame){if(inventoryOpen)CloseInventory();else OpenInventory();}
  target=null;if(!player.Locked || inventoryOpen || (vitals!=null && !vitals.Alive))return;
  if(k.hKey.wasPressedThisFrame){status=inventory!=null && inventory.TryHeal(vitals)?T("Здоровье восстановлено.","Health restored."):T("Лечение недоступно: нет аптечки или здоровье полное.","Cannot heal: no medkit or already healthy.");statusUntil=Time.unscaledTime+4;}
  if(Physics.Raycast(player.view.transform.position,player.view.transform.forward,out var hit,3f,~(1<<player.gameObject.layer),QueryTriggerInteraction.Ignore)){
   candidates.Clear();hit.collider.GetComponentsInParent<MonoBehaviour>(false,candidates);
   foreach(var candidate in candidates)if(candidate is IInteractable interactable){target=interactable;break;}
  }
  if(k.eKey.wasPressedThisFrame){
   if(target!=null){status=target.Interact(train,inventory,english);statusUntil=Time.unscaledTime+4;}
   else if(train.PlayerAboard && Vector3.Distance(player.transform.position,cab.position)<3){train.ToggleBrake();}
  }
 }
 void OnGUI(){
  if(GameMenu.Paused || (inventoryOpen && matView!=null))return;
  GUI.Label(new Rect(18,16,360,25),T("Здоровье: ","Health: ")+(vitals!=null?vitals.Health.ToString("0"):"—"));
  if(train.PlayerAboard)GUI.Label(new Rect(18,42,430,25),$"{train.Speed:0.0} m/s · {train.Fuel:0.0} "+T("топлива · ","fuel · ")+Mathf.Max(0,train.NextStopDistance-train.Distance).ToString("0")+" m");
  if(inventory.SelectedWeapon==WeaponSlot.Pistol)GUI.Label(new Rect(18,68,360,25),T("Патроны: ","Ammo: ")+inventory.Magazine+" / "+inventory.Ammo);
  if(inventory.SelectedWeapon==WeaponSlot.Shotgun)GUI.Label(new Rect(18,68,360,25),T("Дробь: ","Shells: ")+inventory.Tube+" / "+inventory.Shells);
  if(inventory.SelectedWeapon==WeaponSlot.Rifle)GUI.Label(new Rect(18,68,360,25),T("Автомат: ","Rifle: ")+inventory.RifleMagazine+" / "+inventory.RifleAmmo);
  var needs=player.GetComponent<PlayerNeeds>();if(needs!=null && (needs.Hunger<30 || needs.Thirst<30 || showHelp))GUI.Label(new Rect(18,94,360,25),T("Сытость / вода: ","Food / water: ")+needs.Hunger.ToString("0")+" / "+needs.Thirst.ToString("0"));
  if(expedition!=null){
   GUI.Label(new Rect(Screen.width/2-290,18,580,25),T("Радиомодули: ","Radio relays: ")+expedition.Model.Recovered+" / 4");
   if(train.Parked || showHelp)GUI.Label(new Rect(Screen.width/2-290,44,580,25),expedition.Objective(english));
   if(train.Parked)GUI.Label(new Rect(Screen.width/2-290,70,580,25),expedition.Schedule(english));
  }
  if(showFps)GUI.Label(new Rect(Screen.width-100,16,100,25),$"{1f/Mathf.Max(smooth,.001f):0} FPS");
  if(showWorldStats && world!=null && vegetation!=null){
   GUI.Box(new Rect(Screen.width-330,50,310,105),T("ОКРУЖЕНИЕ — F4","ENVIRONMENT — F4"));
   GUI.Label(new Rect(Screen.width-316,78,290,25),T("Участки: ","Tiles: ")+world.ActiveCount+" / "+world.CreatedCount);
   GUI.Label(new Rect(Screen.width-316,103,290,25),T("Видимые растения: ","Visible plants: ")+vegetation.VisibleInstances);
   GUI.Label(new Rect(Screen.width-316,128,290,25),T("Подачи мешей: ","Mesh submissions: ")+vegetation.SubmittedDrawCalls);
  }
  if(!inventoryOpen && player.Locked && (presentation==null || presentation.AimBlend<.5f))GUI.Label(new Rect(Screen.width/2-5,Screen.height/2-12,30,30),"+");
  string prompt="";
  if(vitals!=null && !vitals.Alive)prompt=T("Ты погиб. Загрузи сохранение или начни заново.","You died. Load progress or restart.");
  else if(inventoryOpen)prompt=T("Tab — закрыть список ресурсов","Tab — close resources");
  else if(!player.Locked)prompt=T("Esc — вернуться к управлению","Esc — resume controls");
  else if(target!=null)prompt=target.Prompt(english);
  else if(train.Finished)prompt=T("Конец маршрута.","End of the route.");
  else if(train.maintenance!=null && train.maintenance.Broken)prompt=T("Поломка. Остановись и используй верстак.","Breakdown. Stop and use the workbench.");
  else if(train.PlayerAboard && train.Parked && expedition!=null && !expedition.CanLeave)prompt=expedition.Objective(english);
  else if(train.PlayerAboard && Vector3.Distance(player.transform.position,cab.position)<3)prompt=T("E — тормоз / отправление","E — brake / depart");
  else if(train.Parked)prompt=T("Топливо и ящик припасов — в доме за платформой.","Fuel and a supply crate are in the station house.");
  GUI.Label(new Rect(Screen.width/2-340,Screen.height-65,720,30),prompt);
  if(Time.unscaledTime<statusUntil)GUI.Label(new Rect(Screen.width/2-340,Screen.height-100,720,30),status);
  GUI.Label(new Rect(18,Screen.height-30,450,25),T("Tab — рюкзак · F3 — управление","Tab — backpack · F3 — controls"));
  if(inventoryOpen && inventory!=null){
   float x=Screen.width/2-220,y=Screen.height/2-115;
   GUI.Box(new Rect(x,y,440,300),T("СОБРАННЫЕ РЕСУРСЫ","COLLECTED RESOURCES"));
   GUI.Label(new Rect(x+20,y+45,400,30),T("Металлические детали: ","Metal parts: ")+inventory.MetalParts);
   GUI.Label(new Rect(x+20,y+80,400,30),T("Ткань: ","Cloth: ")+inventory.Cloth);
   GUI.Label(new Rect(x+20,y+115,400,30),T("Аптечки: ","Medkits: ")+inventory.Medkits+T(" · H — лечение"," · H — heal"));
   GUI.Label(new Rect(x+20,y+150,400,30),T("Топор: ","Axe: ")+(inventory.HasAxe?T("есть · 1 — экипировка","owned · 1 — equip"):T("нет","none")));
   var m=train.maintenance;
   if(m!=null)GUI.Label(new Rect(x+20,y+190,400,55),english?
    $"Repair: {m.partsPerRepair} parts + {m.clothPerRepair} cloth → up to +{m.repairAmount:0}%.\nWorkbench: rear left of the railcar.":
    $"Ремонт: {m.partsPerRepair} детали + {m.clothPerRepair} ткань → до +{m.repairAmount:0}%.\nВерстак находится сзади слева в вагоне.");
   GUI.Label(new Rect(x+20,y+265,400,25),T("Мир продолжает двигаться.","The world keeps running."));
  }
  if(showHelp){
   GUI.Box(new Rect(16,220,500,230),T("УПРАВЛЕНИЕ","CONTROLS"));
   GUI.Label(new Rect(28,250,475,200),T("WASD — ходьба, мышь — обзор, Space — прыжок\nE — взаимодействие, Esc — пауза\nTab — ресурсы, F1 — RU/EN, F2 — FPS\nЛКМ — атака, ПКМ — прицел, 1/2/3/4 — оружие, H — лечение\nR — перезарядка, F — фонарик, F5/F9 — сохранить/загрузить\nРемонт доступен при остановленном поезде.\nЯщики можно обыскать один раз за сессию.","WASD move, mouse look, Space jump\nE interact, Esc pause\nTab resources, F1 RU/EN, F2 FPS\nLMB attack, RMB aim, 1/2/3/4 weapons, H heal\nR reload, F flashlight, F5/F9 save/load\nStop the train to repair it.\nCrates can be looted once per session."));
  }
 }
}
}

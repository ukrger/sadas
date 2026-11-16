using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
namespace TrainSummer {
[DefaultExecutionOrder(210)]public sealed class InventoryMatView:MonoBehaviour {
 public FirstPersonMotor player;public PrototypeUI ui;public ResourceInventory inventory;public PlayerNeeds needs;public TrainMotor train;public Transform matRoot;
 Camera overlay;InventoryMatSlot[] slots;Vector3 oldPosition;Quaternion oldRotation;int originalMask;bool opened,stacked,initialized;float opening;
 InventoryMatSlot hovered;BagSlotKind selected;string message="";
 bool English=>Localizer.English;string T(string ru,string en)=>English?en:ru;
 void Awake(){
  foreach(var group in matRoot.GetComponentsInChildren<LODGroup>(true))group.ForceLOD(0);
  slots=matRoot.GetComponentsInChildren<InventoryMatSlot>(true);originalMask=player.view.cullingMask;
  var obj=new GameObject("Inventory Overlay Camera");obj.transform.SetParent(player.view.transform,false);overlay=obj.AddComponent<Camera>();overlay.cullingMask=1<<9;overlay.nearClipPlane=.03f;overlay.farClipPlane=10;overlay.fieldOfView=player.view.fieldOfView;
  var data=obj.AddComponent<UniversalAdditionalCameraData>();data.renderType=CameraRenderType.Overlay; // New URP 14 overlay cameras clear depth by default (read-only API).
  data.renderPostProcessing=false;
  var cameraData=player.view.GetUniversalAdditionalCameraData();var stack=cameraData.cameraStack;stacked=stack!=null;
  if(stacked){stack.Add(overlay);player.view.cullingMask=originalMask&~(1<<9);}else{player.view.cullingMask=originalMask|(1<<9);Debug.LogWarning("Inventory fallback: use a URP Forward renderer for camera stacking.");}
  overlay.enabled=false;matRoot.gameObject.SetActive(false);initialized=true;
 }
 public void SetOpen(bool value){
  if(!initialized || opened==value)return;opened=value;hovered=null;
  if(value){oldPosition=player.view.transform.localPosition;oldRotation=player.view.transform.localRotation;opening=0;Refresh();}
  else{player.SuppressLook();player.view.transform.localPosition=oldPosition;player.view.transform.localRotation=oldRotation;}
  matRoot.gameObject.SetActive(value);overlay.enabled=value && stacked;
  if(!GameMenu.Paused){Cursor.lockState=value?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=value;}
 }
 int Count(BagSlotKind kind){switch(kind){
  case BagSlotKind.Rifle:return inventory.HasRifle?1:0;case BagSlotKind.RifleAmmo:return inventory.RifleAmmo;case BagSlotKind.Shotgun:return inventory.HasShotgun?1:0;case BagSlotKind.Shells:return inventory.Shells;case BagSlotKind.Parts:return inventory.MetalParts;case BagSlotKind.Cloth:return inventory.Cloth;case BagSlotKind.Water:return inventory.Water;case BagSlotKind.Food:return inventory.Food;
  case BagSlotKind.Medkit:return inventory.Medkits;case BagSlotKind.RepairKit:return inventory.RepairKits;case BagSlotKind.Ammo:return inventory.Ammo;case BagSlotKind.Axe:return inventory.HasAxe?1:0;case BagSlotKind.Pistol:return inventory.HasPistol?1:0;default:return 0;}}
 string Name(BagSlotKind kind){switch(kind){
  case BagSlotKind.Rifle:return T("Автомат","Rifle");case BagSlotKind.RifleAmmo:return T("Патроны автомата","Rifle ammo");case BagSlotKind.Shotgun:return T("Дробовик","Shotgun");case BagSlotKind.Shells:return T("Дробь","Shells");case BagSlotKind.Parts:return T("Детали","Parts");case BagSlotKind.Cloth:return T("Ткань","Cloth");case BagSlotKind.Water:return T("Вода","Water");case BagSlotKind.Food:return T("Консервы","Food");
  case BagSlotKind.Medkit:return T("Аптечка","Medkit");case BagSlotKind.RepairKit:return T("Ремонтный набор","Repair kit");case BagSlotKind.Ammo:return T("Патроны","Ammo");case BagSlotKind.Axe:return T("Топор","Axe");default:return T("Пистолет","Pistol");}}
 void Refresh(){foreach(var slot in slots)slot.gameObject.SetActive(Count(slot.kind)>0);}
 Rect Panel=>new Rect(Mathf.Max(8,Screen.width/2-360),Screen.height-178,Mathf.Min(720,Screen.width-16),165);
 void Update(){
  if(!opened || GameMenu.Paused)return;if(!player.vitals.Alive){ui.CloseInventory();return;}
  Refresh();var mouse=Mouse.current;hovered=null;if(mouse==null)return;
  Vector2 pointer=mouse.position.ReadValue();Vector2 guiPoint=new Vector2(pointer.x,Screen.height-pointer.y);
  if(Panel.Contains(guiPoint))return;
  Camera picker=stacked?overlay:player.view;
  Physics.SyncTransforms();
  if(Physics.Raycast(picker.ScreenPointToRay(pointer),out var hit,10,1<<9,QueryTriggerInteraction.Collide))hovered=hit.collider.GetComponent<InventoryMatSlot>();
  if(hovered!=null && mouse.leftButton.wasPressedThisFrame){selected=hovered.kind;UseSelected();}
 }
 void LateUpdate(){
  if(!opened || GameMenu.Paused)return;opening=Mathf.Min(1,opening+Time.unscaledDeltaTime*5);
  player.view.transform.localPosition=Vector3.Lerp(oldPosition,new Vector3(0,1.45f,0),opening);
  player.view.transform.localRotation=Quaternion.Slerp(oldRotation,Quaternion.Euler(58,0,0),opening);overlay.fieldOfView=player.view.fieldOfView;
 }
 void UseSelected(){
  bool success=false;
  switch(selected){
   case BagSlotKind.Water:success=inventory.TryDrink(needs);break;
   case BagSlotKind.Food:success=inventory.TryEat(needs);break;
   case BagSlotKind.Medkit:success=inventory.TryHeal(player.vitals);break;
   case BagSlotKind.Axe:if(inventory.SelectedWeapon!=WeaponSlot.Axe)inventory.Equip(WeaponSlot.Axe);message=T("Топор выбран. Закрой рюкзак.","Axe selected. Close the backpack.");return;
   case BagSlotKind.Rifle:if(inventory.SelectedWeapon!=WeaponSlot.Rifle)inventory.Equip(WeaponSlot.Rifle);message=T("Автомат выбран. Закрой рюкзак.","Rifle selected. Close the backpack.");return;
   case BagSlotKind.Shotgun:if(inventory.SelectedWeapon!=WeaponSlot.Shotgun)inventory.Equip(WeaponSlot.Shotgun);message=T("Дробовик выбран. Закрой рюкзак.","Shotgun selected. Close the backpack.");return;
   case BagSlotKind.Pistol:if(inventory.SelectedWeapon!=WeaponSlot.Pistol)inventory.Equip(WeaponSlot.Pistol);message=T("Пистолет выбран. Закрой рюкзак.","Pistol selected. Close the backpack.");return;
   case BagSlotKind.RepairKit:
    if(!train.PlayerAboard){message=T("Для ремонта вернись в поезд.","Return to the train to repair it.");return;}
    success=train.maintenance.TryUseKit(inventory)==RepairResult.Repaired;break;
   default:message=Name(selected)+" × "+Count(selected);return;
  }
  message=success?T("Предмет использован.","Item used."):T("Сейчас использовать нельзя.","Cannot use this item now.");Refresh();
 }
 void Craft(CraftRecipe recipe){message=inventory.TryCraft(recipe)?T("Предмет создан.","Item crafted."):T("Не хватает материалов или места.","Not enough materials or inventory space.");Refresh();}
 void OnGUI(){
  if(!opened || GameMenu.Paused)return;
  GUI.Label(new Rect(Screen.width/2-280,18,700,25),T("РЮКЗАК · мышь — выбрать и использовать · Tab — закрыть","BACKPACK · click an item to use/select · Tab — close"));
  GUI.Label(new Rect(Screen.width/2-260,45,700,25),T("Здоровье / сытость / вода: ","Health / food / water: ")+player.vitals.Health.ToString("0")+" / "+needs.Hunger.ToString("0")+" / "+needs.Thirst.ToString("0"));
  if(SettingsService.Current.showFps)GUI.Label(new Rect(Screen.width-100,16,100,25),(1f/Mathf.Max(Time.unscaledDeltaTime,.001f)).ToString("0")+" FPS");
  Camera camera=stacked?overlay:player.view;
  foreach(var slot in slots)if(slot.gameObject.activeSelf){Vector3 pos=camera.WorldToScreenPoint(slot.transform.position);if(pos.z>0)GUI.Label(new Rect(pos.x-16,Screen.height-pos.y-22,80,25),"× "+Count(slot.kind));}
  var panel=Panel;GUI.Box(panel,T("КРАФТ И ПРЕДМЕТЫ","CRAFTING AND ITEMS"));
  GUI.Label(new Rect(panel.x+14,panel.y+28,panel.width-28,25),hovered!=null?Name(hovered.kind):message);
  float buttonWidth=(panel.width-42)/2;
  if(GUI.Button(new Rect(panel.x+14,panel.y+65,buttonWidth,42),T("Ремнабор: 3 детали + 1 ткань","Repair kit: 3 parts + 1 cloth")))Craft(CraftRecipe.RepairKit);
  if(GUI.Button(new Rect(panel.x+28+buttonWidth,panel.y+65,buttonWidth,42),T("Аптечка: 1 деталь + 2 ткани","Medkit: 1 part + 2 cloth")))Craft(CraftRecipe.Medkit);
  GUI.Label(new Rect(panel.x+14,panel.y+120,panel.width-28,35),T("Мир не на паузе. Питьё +45, еда +35, аптечка до +35 здоровья.","The world keeps running. Water +45, food +35, medkit up to +35 health."));
 }
 void OnDestroy(){if(!initialized)return;var stack=player!=null && player.view!=null?player.view.GetUniversalAdditionalCameraData().cameraStack:null;if(stack!=null && overlay!=null)stack.Remove(overlay);if(player!=null && player.view!=null)player.view.cullingMask=originalMask;if(overlay!=null)Destroy(overlay.gameObject);}
}
}

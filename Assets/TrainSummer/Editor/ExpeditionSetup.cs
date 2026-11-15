using UnityEngine;
using UnityEngine.Rendering;
namespace TrainSummer.Editor {
public static class ExpeditionSetup {
 static GameObject Prop(string name,string asset,Transform parent,Vector3 position,Vector3 center,Vector3 size,Material fallback){
  var obj=new GameObject(name);obj.transform.SetParent(parent,false);obj.transform.localPosition=position;
  bool detailed=ModelSetup.Attach("Assets/Models/"+asset+".fbx",obj.transform)!=null;
  var collider=obj.AddComponent<BoxCollider>();collider.center=center;collider.size=size;
  if(!detailed){var cube=GameObject.CreatePrimitive(PrimitiveType.Cube);cube.transform.SetParent(obj.transform,false);cube.transform.localPosition=center;cube.transform.localScale=size;cube.GetComponent<Renderer>().sharedMaterial=fallback;cube.GetComponent<Collider>().enabled=false;Object.DestroyImmediate(cube.GetComponent<Collider>());}
  return obj;
 }
 public static void ConfigurePlayer(TrainMotor train,FirstPersonMotor player,ResourceInventory inventory,PrototypeUI ui,Material fallback){
  var vitals=player.gameObject.AddComponent<PlayerVitals>();player.vitals=vitals;ui.vitals=vitals;
  Prop("Field Backpack","FieldBackpack",train.transform,new Vector3(-.85f,1.15f,-2.9f),new Vector3(0,.36f,0),new Vector3(.62f,.72f,.42f),fallback).AddComponent<BackpackInteractable>();
  var bench=train.transform.Find("Repair Workbench");
  Prop("Axe Pickup","SurvivalAxe",bench,new Vector3(.65f,1.02f,-.15f),new Vector3(-.10f,.4f,0),new Vector3(.42f,.8f,.14f),fallback).AddComponent<AxePickup>();
  var pivot=new GameObject("Equipped Axe").transform;pivot.SetParent(player.view.transform,false);pivot.localPosition=new Vector3(.30f,-.62f,.64f);pivot.localRotation=Quaternion.Euler(0,0,-15);
  var visual=ModelSetup.Attach("Assets/Models/SurvivalAxe.fbx",pivot);
  if(visual!=null)foreach(var renderer in visual.GetComponentsInChildren<Renderer>())renderer.shadowCastingMode=ShadowCastingMode.Off;
  else {var cube=GameObject.CreatePrimitive(PrimitiveType.Cube);cube.transform.SetParent(pivot,false);cube.transform.localPosition=Vector3.up*.35f;cube.transform.localScale=new Vector3(.045f,.7f,.04f);cube.GetComponent<Renderer>().sharedMaterial=fallback;cube.GetComponent<Collider>().enabled=false;Object.DestroyImmediate(cube.GetComponent<Collider>());}
  pivot.gameObject.SetActive(false);
  var melee=player.gameObject.AddComponent<PlayerMelee>();melee.player=player;melee.ui=ui;melee.inventory=inventory;melee.vitals=vitals;melee.weaponPivot=pivot;
 }
 public static void AddStationProps(Transform house,Material wood){
  Prop("Medkit","FieldMedkit",house,new Vector3(-2.4f,1.02f,1.5f),new Vector3(0,.2f,0),new Vector3(.36f,.4f,.22f),wood).AddComponent<MedkitPickup>();
  var barrier=new GameObject("Breakable Door Boards");barrier.transform.SetParent(house,false);barrier.transform.localPosition=new Vector3(0,0,-2.66f);
  var collider=barrier.AddComponent<BoxCollider>();collider.center=new Vector3(0,1,0);collider.size=new Vector3(1.35f,1.45f,.12f);barrier.AddComponent<BreakableWood>();
  foreach(float y in new[]{.55f,1f,1.45f}){
   var board=GameObject.CreatePrimitive(PrimitiveType.Cube);board.name="Door Board";board.transform.SetParent(barrier.transform,false);board.transform.localPosition=new Vector3(0,y,0);board.transform.localScale=new Vector3(1.35f,.17f,.1f);board.GetComponent<Renderer>().sharedMaterial=wood;board.GetComponent<Collider>().enabled=false;Object.DestroyImmediate(board.GetComponent<Collider>());
  }
 }
}
}

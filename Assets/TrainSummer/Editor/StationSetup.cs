using UnityEngine;
namespace TrainSummer.Editor {
public static class StationSetup {
 public static void Build(RoutePath route,float distance,int number,Material concrete,Material wood,Material fuel,float fuelAmount=20){
  var pose=route.Evaluate(distance);var station=new GameObject("Station "+number).transform;station.SetPositionAndRotation(pose.position,pose.rotation);
  Box(station,"Platform",new Vector3(6,.35f,0),new Vector3(7,.7f,26),concrete,false);
  Box(station,"Shelter roof",new Vector3(7,3.7f,4),new Vector3(5,.2f,8),wood,false);
  foreach(float z in new[]{1f,7f})Box(station,"Pillar",new Vector3(8,2,z),new Vector3(.25f,3.4f,.25f),wood,false);
  for(int i=0;i<3;i++)Box(station,"Platform exit step",new Vector3(9.65f+i*.3f,.3f-i*.1f,0),new Vector3(.4f,.6f-i*.2f,2),concrete,false);
  var house=new GameObject("Station House").transform;house.SetParent(station,false);house.localPosition=new Vector3(14,0,0);house.localRotation=Quaternion.Euler(0,90,0);
  bool detailed=ModelSetup.Attach("Assets/Models/StationHouse.fbx",house)!=null;
  Box(house,"Foundation",new Vector3(0,.1f,0),new Vector3(6,.2f,5),concrete,detailed);
  Box(house,"Floor",new Vector3(0,.24f,0),new Vector3(5.8f,.08f,4.8f),wood,detailed);
  Box(house,"Porch",new Vector3(0,.14f,-3),new Vector3(6,.16f,1.2f),wood,detailed);
  foreach(float side in new[]{-1f,1f}){
   Box(house,"Side lower",new Vector3(side*3,.7f,0),new Vector3(.16f,1,5),wood,detailed);
   Box(house,"Side upper",new Vector3(side*3,2.72f,0),new Vector3(.16f,.56f,5),wood,detailed);
   foreach(float z in new[]{-2.4f,-1.4f,0,1.4f,2.4f})Box(house,"Side post",new Vector3(side*3,1.82f,z),new Vector3(.17f,1.25f,.15f),wood,detailed);
   Box(house,"Front lower",new Vector3(side*1.85f,.7f,-2.5f),new Vector3(2.3f,1,.16f),wood,detailed);
   Box(house,"Front upper",new Vector3(side*1.85f,2.72f,-2.5f),new Vector3(2.3f,.56f,.16f),wood,detailed);
   foreach(float x in new[]{side*.7f,side*3})Box(house,"Door/window post",new Vector3(x,1.82f,-2.5f),new Vector3(.16f,1.25f,.18f),wood,detailed);
  }
  Box(house,"Lintel",new Vector3(0,2.72f,-2.5f),new Vector3(1.4f,.56f,.18f),wood,detailed);
  Box(house,"Rear wall",new Vector3(0,1.6f,2.5f),new Vector3(6,2.8f,.16f),wood,detailed);
  float roofSlope=Mathf.Atan2(1.1f,2.75f)*Mathf.Rad2Deg;
  foreach(float side in new[]{-1f,1f}){
   var roof=Box(house,"Roof",new Vector3(0,3.55f,side*1.375f),new Vector3(6.6f,.14f,Mathf.Sqrt(2.75f*2.75f+1.1f*1.1f)),wood,detailed);
   roof.transform.localRotation=Quaternion.Euler(side*roofSlope,0,0);
  }
  Box(house,"Canopy",new Vector3(0,2.95f,-3.1f),new Vector3(6.4f,.14f,1.7f),wood,detailed);
  Box(house,"Table",new Vector3(1.6f,1.05f,.8f),new Vector3(1.6f,.12f,.8f),wood,detailed);
  Box(house,"Fuel can",new Vector3(1.6f,1.46f,.8f),new Vector3(.5f,.7f,.35f),fuel,false).AddComponent<FuelPickup>().amount=fuelAmount;
 }
 static GameObject Box(Transform parent,string name,Vector3 pos,Vector3 size,Material mat,bool invisible){
  var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;obj.transform.SetParent(parent,false);obj.transform.localPosition=pos;obj.transform.localScale=size;var r=obj.GetComponent<MeshRenderer>();r.sharedMaterial=mat;r.enabled=!invisible;return obj;
 }
}
}

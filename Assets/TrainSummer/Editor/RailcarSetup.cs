using UnityEditor;
using UnityEngine;
namespace TrainSummer.Editor {
public static class RailcarSetup {
 public static Transform Build(Transform parent,Material fallback){
  bool detailed=ModelSetup.Attach("Assets/Models/SummerRailcar.fbx",parent)!=null;
  var collision=new GameObject("Railcar Colliders").transform;collision.SetParent(parent,false);
  Box(collision,"Floor",new Vector3(0,1,0),new Vector3(3,.3f,12),fallback,detailed);
  Box(collision,"Roof",new Vector3(0,4,0),new Vector3(3.2f,.2f,12),fallback,detailed);
  for(int side=-1;side<=1;side+=2){
   float start=side<0?-6:-3.4f;float span=6-start;
   Box(collision,"Lower wall",new Vector3(side*1.5f,1.65f,(start+6)/2),new Vector3(.12f,1,span),fallback,detailed);
   Box(collision,"Upper wall",new Vector3(side*1.5f,3.65f,(start+6)/2),new Vector3(.12f,.55f,span),fallback,detailed);
   if(side>0)Box(collision,"Rear side panel",new Vector3(1.5f,2.5f,-5.8f),new Vector3(.12f,2.8f,.4f),fallback,detailed);
   for(float z=start;z<=6;z+=1.8f)Box(collision,"Pillar",new Vector3(side*1.5f,2.75f,z),new Vector3(.15f,1.2f,.1f),fallback,detailed);
   Box(collision,"End pillar",new Vector3(side*1.5f,2.75f,6),new Vector3(.15f,1.2f,.1f),fallback,detailed);
  }
  foreach(float z in new[]{-6f,6f}){
   Box(collision,"End lower wall",new Vector3(0,1.7f,z),new Vector3(3,1.1f,.12f),fallback,detailed);
   Box(collision,"End upper wall",new Vector3(0,3.65f,z),new Vector3(3,.6f,.12f),fallback,detailed);
   foreach(float x in new[]{-1.43f,0,1.43f})Box(collision,"End pillar",new Vector3(x,2.8f,z),new Vector3(.12f,1.1f,.14f),fallback,detailed);
  }
  for(int i=0;i<4;i++)Box(collision,"Step",new Vector3(2.7f-i*.3f,.15f+i*.25f,-4.5f),new Vector3(.4f,.3f,2),fallback,detailed);
  foreach(float z in new[]{-1.7f,.1f,1.9f})foreach(float side in new[]{-1f,1f}){
   Box(collision,"Bench",new Vector3(side*1.04f,1.65f,z),new Vector3(.66f,.15f,1.4f),fallback,detailed);
   Box(collision,"Bench back",new Vector3(side*1.34f,2,z),new Vector3(.12f,.7f,1.4f),fallback,detailed);
  }
  return Box(collision,"Cab control",new Vector3(0,1.6f,4.5f),new Vector3(1.8f,.8f,.6f),fallback,detailed).transform;
 }
 static GameObject Box(Transform parent,string name,Vector3 position,Vector3 size,Material material,bool invisible){
  var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;obj.transform.SetParent(parent,false);obj.transform.localPosition=position;obj.transform.localScale=size;
  obj.GetComponent<MeshRenderer>().sharedMaterial=material;obj.GetComponent<MeshRenderer>().enabled=!invisible;return obj;
 }
}
}

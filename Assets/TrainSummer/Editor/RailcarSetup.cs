using UnityEditor;
using UnityEngine;
namespace TrainSummer.Editor {
public static class RailcarSetup {
 static Transform Find(Transform root,string name){
  foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name==name)return t;
  return null;
 }
 public static Transform Build(Transform parent,Material fallback){
  var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/SummerRailcar.fbx");
  bool detailed=asset!=null;
  if(detailed){
   var visual=(GameObject)PrefabUtility.InstantiatePrefab(asset,parent);
   visual.name="Railcar Visual";visual.transform.localPosition=Vector3.zero;
   var right=Find(visual.transform,"AxisRight");var forward=Find(visual.transform,"AxisForward");var up=Find(visual.transform,"AxisUp");
   if(right!=null && forward!=null && up!=null){
    Vector3 f=parent.InverseTransformVector(forward.position-visual.transform.position);
    Vector3 u=parent.InverseTransformVector(up.position-visual.transform.position);
    float unit=parent.InverseTransformVector(right.position-visual.transform.position).magnitude;
    visual.transform.localRotation=Quaternion.Inverse(Quaternion.LookRotation(f,u))*visual.transform.localRotation;
    if(unit>.0001f)visual.transform.localScale/=unit;
   }
   string folder="Assets/TrainSummer/Generated";
   foreach(var renderer in visual.GetComponentsInChildren<MeshRenderer>(true)){
    renderer.enabled=true;
    var shared=renderer.sharedMaterials;
    for(int i=0;i<shared.Length;i++){
     string name=renderer.name;Color color=new Color(.14f,.22f,.18f);float metallic=.5f;
     if(name.Contains("Steel")){color=new Color(.12f,.14f,.15f);metallic=.8f;}
     else if(name.Contains("Wood")){color=new Color(.30f,.18f,.085f);metallic=0;}
     else if(name.Contains("Seat")){color=new Color(.18f,.105f,.055f);metallic=0;}
     else if(name.Contains("Trim")){color=new Color(.72f,.63f,.40f);metallic=.45f;}
     else if(name.Contains("Gauge")){color=new Color(.025f,.04f,.035f);metallic=.2f;}
     string key=name.Substring(name.IndexOf('_')+1);string path=folder+"/"+key+".mat";
     var material=AssetDatabase.LoadAssetAtPath<Material>(path);
     if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
     material.color=color;material.SetFloat("_Metallic",metallic);material.SetFloat("_Smoothness",.35f);material.enableInstancing=true;
     shared[i]=material;
    }
    renderer.sharedMaterials=shared;
   }
   var group=visual.AddComponent<LODGroup>();var lods=new LOD[3];float[] heights={.20f,.075f,.015f};
   for(int i=0;i<3;i++){var root=Find(visual.transform,"LOD"+i);if(root!=null){root.gameObject.SetActive(true);lods[i]=new LOD(heights[i],root.GetComponentsInChildren<Renderer>(true));}}
   group.SetLODs(lods);group.RecalculateBounds();
  }
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

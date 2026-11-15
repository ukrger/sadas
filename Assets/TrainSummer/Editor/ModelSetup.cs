using UnityEditor;
using UnityEngine;
namespace TrainSummer.Editor {
// Shared import adapter for our Blender assets: semantic axes, shared URP materials, LODs.
public static class ModelSetup {
 static Transform Find(Transform root,string name){foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name==name)return t;return null;}
 public static GameObject Attach(string path,Transform parent){
  var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(asset==null)return null;
  var visual=(GameObject)PrefabUtility.InstantiatePrefab(asset,parent);visual.transform.localPosition=Vector3.zero;
  var right=Find(visual.transform,"AxisRight");var forward=Find(visual.transform,"AxisForward");var up=Find(visual.transform,"AxisUp");
  if(right!=null && forward!=null && up!=null){
   Vector3 f=parent.InverseTransformVector(forward.position-visual.transform.position),u=parent.InverseTransformVector(up.position-visual.transform.position);
   float unit=parent.InverseTransformVector(right.position-visual.transform.position).magnitude;
   visual.transform.localRotation=Quaternion.Inverse(Quaternion.LookRotation(f,u))*visual.transform.localRotation;
   if(unit>.0001f)visual.transform.localScale/=unit;
  }
  foreach(var renderer in visual.GetComponentsInChildren<MeshRenderer>(true)){
   renderer.enabled=true;var materials=renderer.sharedMaterials;
   string key=renderer.name.Substring(renderer.name.IndexOf('_')+1);
   for(int i=0;i<materials.Length;i++)materials[i]=MaterialFor(key);
   renderer.sharedMaterials=materials;
  }
  var group=visual.AddComponent<LODGroup>();var lods=new LOD[3];float[] heights={.20f,.075f,.015f};
  for(int i=0;i<3;i++){
   var lod=Find(visual.transform,"LOD"+i);
   if(lod==null){Debug.LogError("Missing LOD"+i+" in "+path);return visual;}
   lod.gameObject.SetActive(true);lods[i]=new LOD(heights[i],lod.GetComponentsInChildren<Renderer>(true));
  }
  group.SetLODs(lods);group.RecalculateBounds();return visual;
 }
 static Material MaterialFor(string key){
  string path="Assets/TrainSummer/Generated/"+key+".mat";
  var material=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
  Color color=new Color(.14f,.22f,.18f);float metallic=.5f;
  switch(key){
   case "PropWood":color=new Color(.30f,.19f,.09f);metallic=0;break;
   case "PropSteel":color=new Color(.13f,.15f,.14f);metallic=.75f;break;
   case "PropPaint":color=new Color(.26f,.32f,.16f);metallic=.2f;break;
   case "PropMark":color=new Color(.65f,.57f,.34f);metallic=0;break;
   case "TrainSteel":color=new Color(.12f,.14f,.15f);metallic=.8f;break;
   case "TrainWood":color=new Color(.30f,.18f,.085f);metallic=0;break;
   case "TrainSeat":color=new Color(.18f,.105f,.055f);metallic=0;break;
   case "TrainTrim":color=new Color(.72f,.63f,.40f);metallic=.45f;break;
   case "TrainGauge":color=new Color(.025f,.04f,.035f);metallic=.2f;break;
   case "HouseWood":color=new Color(.24f,.17f,.10f);metallic=0;break;
   case "HousePaint":color=new Color(.24f,.34f,.29f);metallic=0;break;
   case "HouseRoof":color=new Color(.21f,.16f,.12f);metallic=.6f;break;
   case "HouseStone":color=new Color(.34f,.33f,.29f);metallic=0;break;
   case "HouseTrim":color=new Color(.43f,.37f,.24f);metallic=0;break;
  }
  material.color=color;material.SetFloat("_Metallic",metallic);material.SetFloat("_Smoothness",key.StartsWith("House")?.18f:.35f);material.enableInstancing=true;return material;
 }
}
}

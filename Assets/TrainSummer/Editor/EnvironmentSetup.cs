using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace TrainSummer.Editor {
public static class EnvironmentSetup {
 const string Folder="Assets/TrainSummer/Generated";
 public static void Configure(RouteChunks world,FirstPersonMotor player){
  var renderer=world.gameObject.AddComponent<VegetationRenderer>();renderer.viewer=player.view;
  renderer.definitions=new[]{Tree("SummerBirch"),Tree("SummerPine"),Grass()};world.vegetation=renderer;world.player=player;world.ruinPrefab=Ruin();
 }
 static VegetationDefinition Tree(string model){
  var root=new GameObject("Temporary Tree Definition");var visual=ModelSetup.Attach("Assets/Models/"+model+".fbx",root.transform);
  if(visual==null){Object.DestroyImmediate(root);throw new System.InvalidOperationException("Missing environment model: "+model);}
  var definition=LoadDefinition(model);definition.lods=new VegetationLod[3];definition.maxDistance=300;bool bounded=false;
  for(int lod=0;lod<3;lod++){
   var parts=new List<VegetationPart>();
   foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true)){
    if(!filter.name.StartsWith("LOD"+lod+"_"))continue;var renderer=filter.GetComponent<MeshRenderer>();Matrix4x4 local=root.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
    for(int submesh=0;submesh<filter.sharedMesh.subMeshCount;submesh++){
     var material=renderer.sharedMaterials[Mathf.Min(submesh,renderer.sharedMaterials.Length-1)];material.enableInstancing=true;
     parts.Add(new VegetationPart{mesh=filter.sharedMesh,submesh=submesh,material=material,localToTree=local});
    }
    if(lod==0){var bounds=VegetationRenderer.TransformBounds(filter.sharedMesh.bounds,local);if(!bounded){definition.bounds=bounds;bounded=true;}else definition.bounds.Encapsulate(bounds);}
   }
   definition.lods[lod]=new VegetationLod{distance=lod==0?45:(lod==1?110:300),parts=parts.ToArray()};
  }
  definition.bounds.Expand(.5f);EditorUtility.SetDirty(definition);Object.DestroyImmediate(root);return definition;
 }
 static VegetationDefinition LoadDefinition(string name){
  string path=Folder+"/"+name+"_Vegetation.asset";var definition=AssetDatabase.LoadAssetAtPath<VegetationDefinition>(path);if(definition==null){definition=ScriptableObject.CreateInstance<VegetationDefinition>();AssetDatabase.CreateAsset(definition,path);}return definition;
 }
 static VegetationDefinition Grass(){
  var definition=LoadDefinition("SummerGrass");definition.lods=new VegetationLod[3];definition.maxDistance=55;definition.bounds=new Bounds(Vector3.up*.3f,new Vector3(1.5f,.6f,1.5f));
  var material=ModelSetup.MaterialFor("EnvGrass");
  for(int lod=0;lod<3;lod++){
   int blades=lod==0?12:(lod==1?5:2);var vertices=new Vector3[blades*4];var normals=new Vector3[vertices.Length];var colors=new Color[vertices.Length];var uv=new Vector2[vertices.Length];var triangles=new int[blades*6];
   for(int i=0;i<blades;i++){
    float angle=i*2.39996f;Vector3 center=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*(.12f+(i%4)*.12f);Vector3 right=new Vector3(Mathf.Cos(angle+.5f),0,Mathf.Sin(angle+.5f))*.04f;float height=.3f+(i%4)*.08f;int v=i*4,t=i*6;
    vertices[v]=center-right;vertices[v+1]=center+right;vertices[v+2]=center+right*.2f+Vector3.up*height;vertices[v+3]=center-right*.2f+Vector3.up*height;
    for(int j=0;j<4;j++){normals[v+j]=Vector3.up;colors[v+j]=new Color(.8f,1,.8f,j>=2?1:0);}
    uv[v]=Vector2.zero;uv[v+1]=Vector2.right;uv[v+2]=Vector2.one;uv[v+3]=Vector2.up;triangles[t]=v;triangles[t+1]=v+1;triangles[t+2]=v+2;triangles[t+3]=v;triangles[t+4]=v+2;triangles[t+5]=v+3;
   }
   string path=Folder+"/Grass_LOD"+lod+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh==null){mesh=new Mesh{name="Summer Grass LOD"+lod};AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();mesh.vertices=vertices;mesh.normals=normals;mesh.colors=colors;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
   definition.lods[lod]=new VegetationLod{distance=lod==0?20:(lod==1?40:55),parts=new[]{new VegetationPart{mesh=mesh,material=material,submesh=0,localToTree=Matrix4x4.identity}}};
  }
  EditorUtility.SetDirty(definition);return definition;
 }
 static GameObject Ruin(){
  var root=new GameObject("Overgrown Ruin");bool detailed=ModelSetup.Attach("Assets/Models/OvergrownHouse.fbx",root.transform)!=null;
  if(!detailed){Object.DestroyImmediate(root);return null;}
  Collider(root.transform,new Vector3(0,.1f,0),new Vector3(6.6f,.2f,5.6f));Collider(root.transform,new Vector3(0,.23f,0),new Vector3(6.1f,.06f,5.1f));
  foreach(float side in new[]{-1f,1f}){
   Collider(root.transform,new Vector3(side*3.15f,.73f,0),new Vector3(.22f,1,5.4f));Collider(root.transform,new Vector3(side*3.15f,2.83f,0),new Vector3(.22f,.45f,5.4f));
   foreach(float z in new[]{-2.65f,-1.8f,0,1.8f,2.65f})Collider(root.transform,new Vector3(side*3.15f,1.915f,z),new Vector3(.23f,1.39f,.22f));
   Collider(root.transform,new Vector3(side*2,.73f,-2.7f),new Vector3(2.3f,1,.22f));Collider(root.transform,new Vector3(side*2,2.83f,-2.7f),new Vector3(2.3f,.45f,.22f));
   foreach(float x in new[]{side*.9f,side*3.1f})Collider(root.transform,new Vector3(x,1.915f,-2.7f),new Vector3(.23f,1.39f,.24f));
  }
  Collider(root.transform,new Vector3(0,2.83f,-2.7f),new Vector3(1.7f,.45f,.24f));Collider(root.transform,new Vector3(0,1.63f,2.7f),new Vector3(6.4f,2.8f,.22f));
  float slope=Mathf.Atan2(1,2.85f)*Mathf.Rad2Deg;foreach(float side in new[]{-1f,1f})foreach(float x in new[]{-1.6f,1.6f}){if(side<0 && x>0)continue;var roof=Collider(root.transform,new Vector3(x,3.57f,side*1.425f),new Vector3(3.18f,.12f,3.02f));roof.transform.localRotation=Quaternion.Euler(side*slope,0,0);}
  var prefab=PrefabUtility.SaveAsPrefabAsset(root,Folder+"/OvergrownRuin.prefab");Object.DestroyImmediate(root);return prefab;
 }
 static BoxCollider Collider(Transform root,Vector3 position,Vector3 size){var obj=new GameObject("Static Ruin Collider");obj.transform.SetParent(root,false);obj.transform.localPosition=position;var collider=obj.AddComponent<BoxCollider>();collider.size=size;return collider;}
}
}

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace TrainSummer.Editor {
public static class InventorySetup {
 public static void Configure(FirstPersonMotor player,PrototypeUI ui,ResourceInventory inventory,Material metal,Material cloth){
  var needs=player.gameObject.AddComponent<PlayerNeeds>();needs.vitals=player.vitals;
  const string fabricPath="Assets/TrainSummer/Generated/InventoryFabric.mat";var fabric=AssetDatabase.LoadAssetAtPath<Material>(fabricPath);
  if(fabric==null){fabric=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(fabric,fabricPath);}
  fabric.color=new Color(.19f,.14f,.09f);fabric.SetFloat("_Metallic",0);fabric.SetFloat("_Smoothness",.02f);
  var root=new GameObject("Inventory Cloth").transform;root.SetParent(player.transform,false);root.localPosition=new Vector3(0,.06f,.95f);
  root.gameObject.layer=9;root.gameObject.AddComponent<MeshFilter>().sharedMesh=ClothMesh();
  var renderer=root.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=fabric;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
  Slot(root,BagSlotKind.Water,"WaterBottle",new Vector3(-1.0f,.01f,.60f),Vector3.zero,new Vector3(.24f,.32f,.24f),metal);
  Slot(root,BagSlotKind.Food,"CannedFood",new Vector3(-.65f,.01f,.60f),Vector3.zero,new Vector3(.22f,.18f,.22f),metal);
  Slot(root,BagSlotKind.Medkit,"FieldMedkit",new Vector3(-.30f,.01f,.60f),Vector3.zero,new Vector3(.34f,.40f,.24f),cloth);
  Slot(root,BagSlotKind.RepairKit,"RepairKit",new Vector3(.05f,.01f,.60f),Vector3.zero,new Vector3(.32f,.22f,.23f),cloth);
  Slot(root,BagSlotKind.Ammo,"AmmoBox",new Vector3(.40f,.01f,.60f),Vector3.zero,new Vector3(.22f,.18f,.22f),metal);
  Slot(root,BagSlotKind.Axe,"SurvivalAxe",new Vector3(.95f,.055f,-.70f),new Vector3(90,0,0),new Vector3(.40f,.12f,.85f),metal);
  Slot(root,BagSlotKind.Pistol,"ServicePistol",new Vector3(.65f,.04f,-.25f),new Vector3(0,0,90),new Vector3(.30f,.14f,.30f),metal);
  Slot(root,BagSlotKind.Rifle,"AutomaticRifle",new Vector3(-1.0f,.07f,.08f),new Vector3(0,90,0),new Vector3(1.2f,.18f,.20f),metal);
  Slot(root,BagSlotKind.RifleAmmo,"AmmoBox",new Vector3(1.05f,.01f,.60f),Vector3.zero,new Vector3(.20f,.14f,.20f),metal);
  Slot(root,BagSlotKind.Shotgun,"PumpShotgun",new Vector3(-1.0f,.06f,-.32f),new Vector3(0,90,0),new Vector3(1.2f,.18f,.20f),metal);
  Slot(root,BagSlotKind.Shells,"AmmoBox",new Vector3(.75f,.01f,.60f),Vector3.zero,new Vector3(.20f,.14f,.20f),metal);
  Slot(root,BagSlotKind.Parts,null,new Vector3(.3f,.02f,.18f),Vector3.zero,new Vector3(.25f,.12f,.24f),metal);
  Slot(root,BagSlotKind.Cloth,null,new Vector3(.65f,.02f,.18f),Vector3.zero,new Vector3(.26f,.12f,.27f),fabric);
  var backpack=new GameObject("Open Pack Decoration").transform;backpack.SetParent(root,false);backpack.localPosition=new Vector3(1.15f,.03f,.90f);backpack.localScale=Vector3.one*.45f;ModelSetup.Attach("Assets/Models/FieldBackpack.fbx",backpack);
  foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=9;
  foreach(var r in root.GetComponentsInChildren<Renderer>(true)){r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
  foreach(var group in root.GetComponentsInChildren<LODGroup>(true))group.ForceLOD(0);
  root.gameObject.SetActive(false);
  var view=player.gameObject.AddComponent<InventoryMatView>();view.player=player;view.ui=ui;view.inventory=inventory;view.needs=needs;view.train=player.train;view.matRoot=root;ui.matView=view;
  Supplies(player.train.transform,new Vector3(.65f,1.15f,-2.9f),metal);
 }
 public static void Station(Transform house,Material fallback){Supplies(house,new Vector3(.45f,.28f,1.7f),fallback);}
 static void Supplies(Transform parent,Vector3 origin,Material fallback){
  string[] assets={"WaterBottle","CannedFood","RepairKit"};
  for(int i=0;i<3;i++){
   var root=new GameObject("Supply "+assets[i]);root.transform.SetParent(parent,false);root.transform.localPosition=origin+Vector3.right*(i*.28f);
   bool detailed=ModelSetup.Attach("Assets/Models/"+assets[i]+".fbx",root.transform)!=null;
   var collider=root.AddComponent<BoxCollider>();float height=i==0?.28f:(i==1?.13f:.19f);collider.center=Vector3.up*(height*.5f);collider.size=i==2?new Vector3(.32f,height,.20f):new Vector3(.13f,height,.13f);
   root.AddComponent<SurvivalPickup>().kind=(SurvivalItem)i;
   if(!detailed)Proxy(root.transform,fallback,new Vector3(.12f,.24f,.12f));
  }
 }
 static void Slot(Transform parent,BagSlotKind kind,string asset,Vector3 position,Vector3 euler,Vector3 hitSize,Material fallback){
  var root=new GameObject("Bag Slot "+kind);root.transform.SetParent(parent,false);root.transform.localPosition=position;
  var visual=new GameObject("Visual").transform;visual.SetParent(root.transform,false);visual.localRotation=Quaternion.Euler(euler);
  bool loaded=asset!=null && ModelSetup.Attach("Assets/Models/"+asset+".fbx",visual)!=null;
  if(!loaded)Proxy(visual,fallback,kind==BagSlotKind.Cloth?new Vector3(.22f,.045f,.20f):new Vector3(.16f,.05f,.10f));
  root.AddComponent<InventoryMatSlot>().kind=kind;
  var box=root.AddComponent<BoxCollider>();box.isTrigger=true;box.size=hitSize;
  if(kind==BagSlotKind.Shotgun || kind==BagSlotKind.Rifle)box.center=new Vector3(.28f,.08f,0);
  else if(kind==BagSlotKind.Axe)box.center=new Vector3(-.1f,.03f,.38f);
  else if(kind==BagSlotKind.Pistol)box.center=new Vector3(-.10f,.04f,.05f);
  else box.center=Vector3.up*(hitSize.y*.5f);
 }
 static void Proxy(Transform parent,Material mat,Vector3 size){
  var cube=GameObject.CreatePrimitive(PrimitiveType.Cube);cube.transform.SetParent(parent,false);cube.transform.localPosition=Vector3.up*(size.y*.5f);cube.transform.localScale=size;cube.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(cube.GetComponent<Collider>());
 }
 static Mesh ClothMesh(){
  const int width=17,depth=13;var vertices=new Vector3[width*depth];var uv=new Vector2[vertices.Length];var triangles=new int[(width-1)*(depth-1)*6];
  for(int z=0;z<depth;z++)for(int x=0;x<width;x++){
   int i=z*width+x;float u=x/(float)(width-1),v=z/(float)(depth-1);vertices[i]=new Vector3((u-.5f)*2.7f,Mathf.Sin(x*1.9f+z*.8f)*.004f,(v-.5f)*1.8f);uv[i]=new Vector2(u,v);
  }
  int t=0;for(int z=0;z<depth-1;z++)for(int x=0;x<width-1;x++){
   int i=z*width+x;triangles[t++]=i;triangles[t++]=i+width;triangles[t++]=i+1;triangles[t++]=i+1;triangles[t++]=i+width;triangles[t++]=i+width+1;
  }
  const string path="Assets/TrainSummer/Generated/InventoryCloth.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
  if(mesh==null){mesh=new Mesh{name="Inventory Cloth"};AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
  mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);return mesh;
 }
}
}

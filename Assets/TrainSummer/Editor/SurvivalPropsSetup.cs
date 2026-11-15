using UnityEngine;
namespace TrainSummer.Editor {
public static class SurvivalPropsSetup {
 public static void AddCrate(Transform house,Material fallback){
  var root=new GameObject("Supply Crate");root.transform.SetParent(house,false);root.transform.localPosition=new Vector3(-1.8f,.28f,-.5f);
  bool detailed=ModelSetup.Attach("Assets/Models/SupplyCrate.fbx",root.transform)!=null;
  var collider=root.AddComponent<BoxCollider>();collider.center=new Vector3(0,.38f,0);collider.size=new Vector3(1.08f,.76f,.76f);
  root.AddComponent<SupplyCrate>();
  if(!detailed)Fallback(root.transform,new Vector3(0,.38f,0),new Vector3(.98f,.76f,.7f),fallback);
 }
 public static void AddBench(TrainMotor train,Material fallback){
  var root=new GameObject("Repair Workbench");root.transform.SetParent(train.transform,false);root.transform.localPosition=new Vector3(-1.05f,1.15f,-4.5f);root.transform.localRotation=Quaternion.Euler(0,-90,0);
  bool detailed=ModelSetup.Attach("Assets/Models/RepairBench.fbx",root.transform)!=null;
  var body=root.AddComponent<BoxCollider>();body.center=new Vector3(0,.5f,0);body.size=new Vector3(1.8f,1,.7f);
  var board=root.AddComponent<BoxCollider>();board.center=new Vector3(0,1.35f,.3f);board.size=new Vector3(1.8f,.8f,.10f);
  var vise=root.AddComponent<BoxCollider>();vise.center=new Vector3(-.56f,1.13f,-.22f);vise.size=new Vector3(.35f,.3f,.6f);
  root.AddComponent<RepairBench>().maintenance=train.maintenance;
  if(!detailed){Fallback(root.transform,body.center,body.size,fallback);Fallback(root.transform,board.center,board.size,fallback);}
 }
 static void Fallback(Transform root,Vector3 pos,Vector3 size,Material material){
  var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.transform.SetParent(root,false);obj.transform.localPosition=pos;obj.transform.localScale=size;obj.GetComponent<Renderer>().sharedMaterial=material;
  // Root colliders provide interaction and collision; fallback mesh collider is unnecessary.
  var collider=obj.GetComponent<Collider>();collider.enabled=false;Object.DestroyImmediate(collider);
 }
}
}

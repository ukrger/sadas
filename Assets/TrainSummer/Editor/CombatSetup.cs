using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;
namespace TrainSummer.Editor {
public static class CombatSetup {
 static GameObject Prop(string name,string asset,Transform parent,Vector3 position,Vector3 center,Vector3 size,Material fallback){
  var obj=new GameObject(name);obj.transform.SetParent(parent,false);obj.transform.localPosition=position;
  bool detailed=ModelSetup.Attach("Assets/Models/"+asset+".fbx",obj.transform)!=null;
  var collider=obj.AddComponent<BoxCollider>();collider.center=center;collider.size=size;
  if(!detailed){var mesh=GameObject.CreatePrimitive(PrimitiveType.Cube);mesh.transform.SetParent(obj.transform,false);mesh.transform.localPosition=center;mesh.transform.localScale=size;mesh.GetComponent<Renderer>().sharedMaterial=fallback;Object.DestroyImmediate(mesh.GetComponent<Collider>());}
  return obj;
 }
 public static void Player(FirstPersonMotor player,PrototypeUI ui,ResourceInventory inventory,Material metal){
  var loadout=player.gameObject.AddComponent<PlayerLoadout>();loadout.player=player;loadout.ui=ui;loadout.inventory=inventory;
  var bench=player.train.transform.Find("Repair Workbench");
  Prop("Pistol Pickup","ServicePistol",bench,new Vector3(.15f,1.02f,-.05f),new Vector3(0,.12f,.04f),new Vector3(.08f,.24f,.25f),metal).AddComponent<PistolPickup>();
  var pivot=new GameObject("Equipped Pistol").transform;pivot.SetParent(player.view.transform,false);pivot.localPosition=new Vector3(.23f,-.28f,.53f);
  var gunVisual=ModelSetup.Attach("Assets/Models/ServicePistol.fbx",pivot);if(gunVisual!=null)foreach(var renderer in gunVisual.GetComponentsInChildren<Renderer>())renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;pivot.gameObject.SetActive(false);
  var bullets=new GameObject("Ballistic Projectiles").AddComponent<BallisticProjectiles>();
  string path="Assets/TrainSummer/Generated/Tracer.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Unlit"));mat.color=new Color(1,.7f,.25f);AssetDatabase.CreateAsset(mat,path);}bullets.tracerMaterial=mat;
  var pistol=player.gameObject.AddComponent<PlayerPistol>();pistol.player=player;pistol.ui=ui;pistol.inventory=inventory;pistol.pivot=pivot;pistol.projectiles=bullets;pistol.audioEffects=player.gameObject.AddComponent<ProceduralAudio>();
  var lamp=new GameObject("Flashlight").AddComponent<Light>();lamp.transform.SetParent(player.view.transform,false);lamp.type=LightType.Spot;lamp.range=22;lamp.spotAngle=48;lamp.intensity=3;lamp.shadows=LightShadows.None;lamp.enabled=false;
  var flashlight=player.gameObject.AddComponent<PlayerFlashlight>();flashlight.player=player;flashlight.ui=ui;flashlight.lamp=lamp;
  Prop("Ammo on Train","AmmoBox",bench,new Vector3(-.1f,1.02f,-.15f),new Vector3(0,.06f,0),new Vector3(.15f,.12f,.15f),metal).AddComponent<AmmoPickup>();
 }
 public static void Stations(FirstPersonMotor player,Material fallback){
  AnimatorController controller=WalkController();
  foreach(var station in Object.FindObjectsOfType<Transform>()){
   if(!station.name.StartsWith("Station ") || station.name=="Station House")continue;
   var house=station.Find("Station House");if(house==null)continue;
   Prop("Station Ammo","AmmoBox",house,new Vector3(1.7f,1.12f,.85f),new Vector3(0,.06f,0),new Vector3(.15f,.12f,.15f),fallback).AddComponent<AmmoPickup>();
   BuildNavigation(station);
   for(int i=0;i<2;i++){
    var enemy=new GameObject("Mutant "+i);enemy.layer=8;enemy.transform.position=station.TransformPoint(i==0?new Vector3(-8,0,7):new Vector3(18,0,8));
    enemy.transform.rotation=station.rotation;enemy.transform.SetParent(station,true);
    var agent=enemy.AddComponent<NavMeshAgent>();agent.radius=.3f;agent.height=1.8f;agent.speed=2.2f;agent.angularSpeed=180;agent.acceleration=7;agent.stoppingDistance=1.2f;
    var collider=enemy.AddComponent<CapsuleCollider>();collider.center=Vector3.up*.9f;collider.height=1.8f;collider.radius=.28f;
    var visual=ModelSetup.Attach("Assets/Models/ForestMutant.fbx",enemy.transform);
    if(visual==null){var body=GameObject.CreatePrimitive(PrimitiveType.Capsule);body.transform.SetParent(enemy.transform,false);body.transform.localPosition=Vector3.up*.9f;body.GetComponent<Renderer>().sharedMaterial=fallback;Object.DestroyImmediate(body.GetComponent<Collider>());}
    foreach(var t in enemy.GetComponentsInChildren<Transform>(true))t.gameObject.layer=8;
    var ai=enemy.AddComponent<MutantAI>();ai.home=enemy.transform.position;ai.target=player.vitals;ai.player=player;ai.flashlight=player.GetComponent<PlayerFlashlight>();
    if(visual!=null){var animator=visual.GetComponentInChildren<Animator>();if(animator==null)animator=visual.AddComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;ai.animator=animator;}
   }
  }
 }
 static AnimatorController WalkController(){
  const string model="Assets/Models/ForestMutant.fbx";var importer=AssetImporter.GetAtPath(model) as ModelImporter;
  if(importer==null)return null;
  if(importer.animationType!=ModelImporterAnimationType.Generic || importer.clipAnimations.Length!=importer.defaultClipAnimations.Length){importer.animationType=ModelImporterAnimationType.Generic;var clips=importer.defaultClipAnimations;foreach(var clip in clips)clip.loopTime=true;importer.clipAnimations=clips;importer.SaveAndReimport();}
  const string path="Assets/TrainSummer/Generated/Mutant.controller";var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
  if(controller==null)controller=AnimatorController.CreateAnimatorControllerAtPath(path);
  var machine=controller.layers[0].stateMachine;foreach(var state in machine.states)machine.RemoveState(state.state);
  AnimationClip walk=null,idle=null;foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(model))if(asset is AnimationClip clip && !clip.name.StartsWith("__preview__")){if(clip.name.Contains("Walk"))walk=clip;if(clip.name.Contains("Idle"))idle=clip;}
  controller.parameters=new[]{new AnimatorControllerParameter{name="Speed",type=AnimatorControllerParameterType.Float}};
  if(walk!=null){var state=machine.AddState("Locomotion");machine.defaultState=state;
   if(idle!=null){var blend=new BlendTree{name="Locomotion",blendType=BlendTreeType.Simple1D,blendParameter="Speed",useAutomaticThresholds=false};AssetDatabase.AddObjectToAsset(blend,controller);blend.AddChild(idle,0);blend.AddChild(walk,1.4f);state.motion=blend;}else state.motion=walk;
  }
  return controller;
 }
 static void BuildNavigation(Transform station){
  var sources=new List<NavMeshBuildSource>();
  sources.Add(new NavMeshBuildSource{shape=NavMeshBuildSourceShape.Box,size=new Vector3(90,.2f,90),transform=station.localToWorldMatrix*Matrix4x4.Translate(Vector3.down*.1f),area=0});
  foreach(var box in station.GetComponentsInChildren<BoxCollider>()){
   if(box.GetComponentInParent<BreakableWood>()!=null){
    var obstacle=box.gameObject.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Box;obstacle.center=box.center;obstacle.size=box.size;obstacle.carving=true;obstacle.carveOnlyStationary=false;continue;
   }
   sources.Add(new NavMeshBuildSource{shape=NavMeshBuildSourceShape.Box,size=box.size,transform=box.transform.localToWorldMatrix*Matrix4x4.Translate(box.center),area=0});
  }
  var settings=NavMesh.GetSettingsByID(0);settings.agentRadius=.3f;settings.agentHeight=1.8f;settings.agentClimb=.35f;settings.overrideVoxelSize=true;settings.voxelSize=.1f;
  var data=NavMeshBuilder.BuildNavMeshData(settings,sources,new Bounds(station.position+Vector3.up*3,new Vector3(120,20,120)),Vector3.zero,Quaternion.identity);
  if(data==null){Debug.LogError("Could not build station navigation: "+station.name);return;}
  string path=AssetDatabase.GenerateUniqueAssetPath("Assets/TrainSummer/Generated/Nav_"+station.name.Replace(' ','_')+".asset");AssetDatabase.CreateAsset(data,path);station.gameObject.AddComponent<StationNavigation>().data=data;
 }
}
}

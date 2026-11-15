using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace TrainSummer.Editor {
public static class PrototypeBuilder {
 const string Root="Assets/TrainSummer/Generated";
 static Material Mat(string name,Color color,float metallic=0){
  string path=Root+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
  m.color=color;m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",.25f);m.enableInstancing=true;return m;
 }
 static GameObject Box(Transform parent,string name,Vector3 position,Vector3 size,Material mat){
  var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;return g;
 }
 [MenuItem("Train Summer/Create Prototype Scene")]
 public static void Build(){Build(false);}
 [MenuItem("Train Summer/Create Long Route Scene")]
 public static void BuildLong(){Build(true);}
 static void Build(bool longRoute){
  if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
  System.IO.Directory.CreateDirectory(Root);AssetDatabase.Refresh();
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var grass=Mat("Grass",new Color(.27f,.38f,.12f));var wood=Mat("Wood",new Color(.25f,.17f,.10f));var metal=Mat("Steel",new Color(.22f,.26f,.23f),.7f);var foliage=Mat("Leaves",new Color(.20f,.34f,.09f));var concrete=Mat("Concrete",new Color(.45f,.44f,.37f));var fuel=Mat("Fuel",new Color(.55f,.24f,.08f),.4f);
  var route=new GameObject("Route Path").AddComponent<RoutePath>();if(longRoute)route.forwardExtent=15600;route.Rebuild();
  var trainObject=new GameObject("Train");var train=trainObject.AddComponent<TrainMotor>();train.route=route;
  if(longRoute){train.stops=new float[]{2800,6000,9200,12400};train.startingFuel=75;}
  var maintenance=trainObject.AddComponent<TrainMaintenance>();maintenance.train=train;train.maintenance=maintenance;
  var cab=RailcarSetup.Build(trainObject.transform,metal);
  SurvivalPropsSetup.AddBench(train,metal);
  for(int i=0;i<train.stops.Length;i++)StationSetup.Build(route,train.stops[i],i+1,concrete,wood,fuel,longRoute?80:20);
  var playerObject=new GameObject("Player");playerObject.layer=2;playerObject.transform.position=new Vector3(0,1.3f,-2);
  var cc=playerObject.AddComponent<CharacterController>();cc.height=1.8f;cc.radius=.28f;cc.center=new Vector3(0,.9f,0);cc.stepOffset=.3f;
  var inventory=playerObject.AddComponent<ResourceInventory>();
  var player=playerObject.AddComponent<FirstPersonMotor>();player.train=train;
  var cameraObject=new GameObject("Player Camera");cameraObject.tag="MainCamera";cameraObject.transform.SetParent(playerObject.transform,false);cameraObject.transform.localPosition=Vector3.up*1.65f;
  var cam=cameraObject.AddComponent<Camera>();cam.nearClipPlane=.05f;cam.farClipPlane=350;cam.fieldOfView=75;cameraObject.AddComponent<AudioListener>();
  cam.gameObject.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing=true;player.view=cam;
  var ui=playerObject.AddComponent<PrototypeUI>();ui.player=player;ui.train=train;ui.cab=cab;ui.inventory=inventory;
  ExpeditionSetup.ConfigurePlayer(train,player,inventory,ui,wood);
  var world=new GameObject("Route Streamer").AddComponent<RouteChunks>();world.train=train;world.grass=grass;world.wood=wood;world.leaves=foliage;world.steel=metal;
  var sun=new GameObject("Sun").AddComponent<Light>();sun.type=LightType.Directional;sun.shadows=LightShadows.Soft;RenderSettings.sun=sun;
  var cycle=sun.gameObject.AddComponent<SummerLighting>();cycle.sun=sun;
  RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.003f;
  var sky=new Material(Shader.Find("Skybox/Procedural"));sky.name="SummerSky";AssetDatabase.CreateAsset(sky,AssetDatabase.GenerateUniqueAssetPath(Root+"/Sky.mat"));RenderSettings.skybox=sky;
  var volume=new GameObject("Summer Volume").AddComponent<Volume>();volume.isGlobal=true;
  var profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,AssetDatabase.GenerateUniqueAssetPath(Root+"/SummerVolume.asset"));volume.sharedProfile=profile;
  var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);
  var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(.15f);bloom.threshold.Override(1.1f);
  EditorSceneManager.SaveScene(scene,Root+(longRoute?"/Prototype_Long.unity":"/Prototype.unity"));AssetDatabase.SaveAssets();
  Selection.activeGameObject=playerObject;
  Debug.Log("Prototype ready. Press Play. Requires URP 14 and Input System with Active Input Handling set to Input System or Both.");
 }
}
}

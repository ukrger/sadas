using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace TrainSummer.Editor {
public static class BuildTools {
 [MenuItem("Train Summer/Validate Current Scene")]
 public static void ValidateCurrentScene(){
  if(!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset))throw new InvalidOperationException("A URP 14 pipeline must be assigned.");
  var train=UnityEngine.Object.FindObjectOfType<TrainMotor>();var player=UnityEngine.Object.FindObjectOfType<FirstPersonMotor>();
  var mission=UnityEngine.Object.FindObjectOfType<ExpeditionSession>();var saves=UnityEngine.Object.FindObjectOfType<SessionSave>();var menu=UnityEngine.Object.FindObjectOfType<GameMenu>();
  Require(train!=null && train.route!=null && train.maintenance!=null,"Missing train/route/maintenance.");
  Require(player!=null && player.view!=null && player.vitals!=null,"Missing player/camera/vitals.");
  Require(mission!=null && mission.player==player && mission.train==train && train.expedition==mission,"Missing mission references.");
  Require(saves!=null && saves.player==player && saves.train==train && saves.expedition==mission && saves.clock!=null,"Missing save references.");
  Require(menu!=null && menu.saves==saves && menu.ui!=null && menu.expedition==mission,"Missing menu references.");
  Require(train.stops.Length==4,"Mission requires exactly four stops.");
  var relays=UnityEngine.Object.FindObjectsOfType<MissionRelay>(true);Require(relays.Length==4,"Expected four mission radios.");
  var relayIndices=new HashSet<int>();foreach(var relay in relays)Require(relay.mission==mission && relay.stationIndex>=0 && relay.stationIndex<4 && relayIndices.Add(relay.stationIndex),"Invalid/duplicate mission radio.");
  var ids=new HashSet<string>();foreach(var e in UnityEngine.Object.FindObjectsOfType<PersistentEntity>(true))Require(!string.IsNullOrEmpty(e.id) && ids.Add(e.id),"Invalid/duplicate persistent ID.");
  var gun=player.GetComponent<PlayerShotgun>();Require(gun!=null && gun.pivot!=null && gun.projectiles!=null && gun.inventory!=null,"Missing shotgun references.");
  var aiming=player.GetComponent<WeaponPresentation>();Require(aiming!=null && aiming.pistolSight!=null && aiming.shotgunSight!=null && aiming.rifleSight!=null,"Missing aim references.");
  var rifle=player.GetComponent<PlayerRifle>();Require(rifle!=null && rifle.pivot!=null && rifle.projectiles==gun.projectiles,"Missing rifle references.");
  var chunks=UnityEngine.Object.FindObjectOfType<RouteChunks>();Require(chunks!=null && chunks.vegetation!=null && chunks.ruinPrefab!=null,"Missing environment references.");
  foreach(string name in new[]{"TrainSummer/FoliageWind","TrainSummer/SummerSky","Hidden/TrainSummer/SunShaft"}){var shader=Shader.Find(name);Require(shader!=null && !ShaderUtil.ShaderHasError(shader),"Shader import failed: "+name);}
  foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0,"Missing script: "+t.name);
  Debug.Log("Train Summer scene validation passed. This checks references/import errors; run PlayMode tests and profile the built player separately.");
 }
 static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
 [MenuItem("Train Summer/Generate And Validate Long Route")]
 public static void GenerateAndValidate(){Generate();}
 static bool Generate(){if(!PrototypeBuilder.GenerateLongRoute())return false;ValidateCurrentScene();return true;}
 [MenuItem("Train Summer/Build Windows Development Player")]
 public static void BuildWindows(){
  const BuildTarget target=BuildTarget.StandaloneWindows64;
  if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone,target))throw new InvalidOperationException("Install Windows Build Support for this Unity editor.");
  if(!Generate())return;
  string folder=Environment.GetEnvironmentVariable("TRAIN_SUMMER_BUILD_DIR");if(string.IsNullOrEmpty(folder))folder="Builds/Windows";Directory.CreateDirectory(folder);
  var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/TrainSummer/Generated/Prototype_Long.unity"},locationPathName=Path.Combine(folder,"TrainSummer.exe"),target=target,options=BuildOptions.Development|BuildOptions.AllowDebugging});
  if(result.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Build failed: "+result.summary.result);
  Debug.Log("Windows development player built: "+result.summary.outputPath);
 }
 [MenuItem("Train Summer/Bake Station Occlusion (current scene)")]
 public static void BakeOcclusion(){
  ValidateCurrentScene();
  foreach(var nav in UnityEngine.Object.FindObjectsOfType<StationNavigation>(true)){
   foreach(var renderer in nav.GetComponentsInChildren<MeshRenderer>(true)){
    if(renderer.GetComponentInParent<PersistentEntity>()!=null)continue;
    // LODGroup handles visibility at runtime; lower meshes must not become independent occluders.
    if(renderer.name.StartsWith("LOD1_") || renderer.name.StartsWith("LOD2_"))continue;
    GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,StaticEditorFlags.OccluderStatic|StaticEditorFlags.OccludeeStatic);
   }
  }
  if(!StaticOcclusionCulling.Compute())throw new InvalidOperationException("Occlusion bake failed.");
  EditorSceneManager.SaveOpenScenes();Debug.Log("Occlusion baked for fixed station geometry. Dynamic train and explicit vegetation batches are excluded.");
 }
}
}

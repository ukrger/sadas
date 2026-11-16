using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace TrainSummer.Editor {
public static class SettingsSetup {
 public static void Configure(SettingsService service){
  var source=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
  if(source==null){Debug.LogWarning("Create the project with the Unity 2022.3 URP template before building the prototype.");return;}
  var sizes=new[]{1024,2048,4096};service.shadowPresets=new UniversalRenderPipelineAsset[sizes.Length];
  for(int i=0;i<sizes.Length;i++){
   string path="Assets/TrainSummer/Generated/URP_Shadow"+sizes[i]+".asset";
   var preset=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
   if(preset==null){preset=Object.Instantiate(source);preset.name="URP Shadow "+sizes[i];AssetDatabase.CreateAsset(preset,path);}else EditorUtility.CopySerialized(source,preset);
   preset.name="URP Shadow "+sizes[i];
   // URP 14 exposes this runtime property with an internal setter. Configure the asset in Editor instead.
   var serialized=new SerializedObject(preset);var resolution=serialized.FindProperty("m_MainLightShadowmapResolution");
   if(resolution==null)throw new System.InvalidOperationException("Unsupported URP version: expected URP 14 shadow settings.");
   resolution.intValue=sizes[i];serialized.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(preset);service.shadowPresets[i]=preset;
  }
 }
}
}

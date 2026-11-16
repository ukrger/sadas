using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace TrainSummer.Editor {
public static class SettingsSetup {
 public static void Configure(SettingsService service){
  var source=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
  if(source==null){Debug.LogWarning("Create the project with the Unity 2022.3 URP template before building the prototype.");return;}
  var sourceSettings=new SerializedObject(source);var renderers=sourceSettings.FindProperty("m_RendererDataList");var defaultIndex=sourceSettings.FindProperty("m_DefaultRendererIndex");
  if(renderers==null || defaultIndex==null || defaultIndex.intValue<0 || renderers.arraySize<=defaultIndex.intValue)throw new System.InvalidOperationException("Unsupported URP renderer settings.");
  var rendererSource=renderers.GetArrayElementAtIndex(defaultIndex.intValue).objectReferenceValue as UniversalRendererData;
  if(rendererSource==null)throw new System.InvalidOperationException("Train Summer requires a Universal Forward renderer.");
  var sizes=new[]{1024,2048,4096};service.shadowPresets=new UniversalRenderPipelineAsset[sizes.Length];
  for(int i=0;i<sizes.Length;i++){
   // Each preset owns its shaft feature so disposing an old pipeline cannot destroy the new pass.
   var renderer=Renderer(rendererSource,sizes[i]);
   string path="Assets/TrainSummer/Generated/URP_Shadow"+sizes[i]+".asset";var preset=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
   if(preset==null){preset=Object.Instantiate(source);AssetDatabase.CreateAsset(preset,path);}else EditorUtility.CopySerialized(source,preset);
   preset.name="URP Shadow "+sizes[i];
   var serialized=new SerializedObject(preset);var resolution=serialized.FindProperty("m_MainLightShadowmapResolution");
   if(resolution==null)throw new System.InvalidOperationException("Unsupported URP version: expected URP 14 shadow settings.");
   resolution.intValue=sizes[i];var list=serialized.FindProperty("m_RendererDataList");list.GetArrayElementAtIndex(defaultIndex.intValue).objectReferenceValue=renderer;serialized.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(preset);service.shadowPresets[i]=preset;
  }
 }
 static UniversalRendererData Renderer(UniversalRendererData source,int size){
  string path="Assets/TrainSummer/Generated/JourneyRenderer_"+size+".asset";var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
  if(renderer==null){renderer=Object.Instantiate(source);AssetDatabase.CreateAsset(renderer,path);}else EditorUtility.CopySerialized(source,renderer);
  renderer.name="Train Summer Forward "+size;
  var serialized=new SerializedObject(renderer);var mode=serialized.FindProperty("m_RenderingMode");if(mode!=null){mode.intValue=0;serialized.ApplyModifiedPropertiesWithoutUndo();}
  SunShaftFeature shafts=null;foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(path))if(asset is SunShaftFeature feature)shafts=feature;
  if(shafts==null){shafts=ScriptableObject.CreateInstance<SunShaftFeature>();shafts.name="Summer Sun Shafts";AssetDatabase.AddObjectToAsset(shafts,renderer);}
  shafts.shader=Shader.Find("Hidden/TrainSummer/SunShaft");shafts.intensity=.12f;
  renderer.rendererFeatures.RemoveAll(feature=>feature is SunShaftFeature);renderer.rendererFeatures.Add(shafts);renderer.SetDirty();
  var featureSettings=new SerializedObject(renderer);var map=featureSettings.FindProperty("m_RendererFeatureMap");
  if(map!=null){map.arraySize=renderer.rendererFeatures.Count;for(int f=0;f<map.arraySize;f++)if(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(renderer.rendererFeatures[f],out string guid,out long id))map.GetArrayElementAtIndex(f).longValue=id;featureSettings.ApplyModifiedPropertiesWithoutUndo();}
  EditorUtility.SetDirty(renderer);EditorUtility.SetDirty(shafts);return renderer;
 }
}
}

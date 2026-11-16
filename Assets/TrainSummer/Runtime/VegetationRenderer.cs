using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace TrainSummer {
[DefaultExecutionOrder(500)]public sealed class VegetationRenderer:MonoBehaviour {
 public Camera viewer;public VegetationDefinition[] definitions;
 sealed class Batch {public VegetationPart part;public int lod,count;public readonly Matrix4x4[] matrices=new Matrix4x4[1023];}
 readonly List<VegetationTile> tiles=new List<VegetationTile>();readonly Plane[] planes=new Plane[6];Batch[][][] batches;
 public int VisibleInstances {get;private set;}public int SubmittedDrawCalls {get;private set;}
 public VegetationTile Register(int count){var tile=new VegetationTile{instances=new VegetationInstance[count]};tiles.Add(tile);return tile;}
 void Awake(){
  if(definitions==null)definitions=new VegetationDefinition[0];batches=new Batch[definitions.Length][][];
  for(int species=0;species<definitions.Length;species++){
   var def=definitions[species];if(def==null){batches[species]=new Batch[0][];continue;}
   batches[species]=new Batch[def.lods.Length][];
   for(int lod=0;lod<def.lods.Length;lod++){var parts=def.lods[lod].parts;batches[species][lod]=new Batch[parts.Length];for(int part=0;part<parts.Length;part++)batches[species][lod][part]=new Batch{part=parts[part],lod=lod};}
  }
 }
 public VegetationInstance Place(int species,Vector3 position,Quaternion rotation,float scale){
  var matrix=Matrix4x4.TRS(position,rotation,Vector3.one*scale);Bounds bounds=species<definitions.Length && definitions[species]!=null?TransformBounds(definitions[species].bounds,matrix):new Bounds(position+Vector3.up*4,Vector3.one*8);
  return new VegetationInstance{species=species,matrix=matrix,bounds=bounds,position=position,scale=scale};
 }
 public static Bounds TransformBounds(Bounds local,Matrix4x4 matrix){
  Vector3 x=matrix.MultiplyVector(new Vector3(local.extents.x,0,0)),y=matrix.MultiplyVector(new Vector3(0,local.extents.y,0)),z=matrix.MultiplyVector(new Vector3(0,0,local.extents.z));
  var extents=new Vector3(Mathf.Abs(x.x)+Mathf.Abs(y.x)+Mathf.Abs(z.x),Mathf.Abs(x.y)+Mathf.Abs(y.y)+Mathf.Abs(z.y),Mathf.Abs(x.z)+Mathf.Abs(y.z)+Mathf.Abs(z.z));return new Bounds(matrix.MultiplyPoint3x4(local.center),extents*2);
 }
 void LateUpdate(){
  if(viewer==null || batches==null)return;VisibleInstances=0;SubmittedDrawCalls=0;GeometryUtility.CalculateFrustumPlanes(viewer,planes);
  foreach(var tile in tiles){if(!tile.active)continue;foreach(var instance in tile.instances){
   if(instance.species<0 || instance.species>=definitions.Length)continue;var def=definitions[instance.species];if(def==null)continue;
   float distance=Vector3.Distance(viewer.transform.position,instance.position);float bias=Mathf.Max(.1f,QualitySettings.lodBias);float limit=Mathf.Min(viewer.farClipPlane,def.maxDistance*bias);
   if(distance>limit || !GeometryUtility.TestPlanesAABB(planes,instance.bounds))continue;
   int lod=0;float normalized=distance/(bias*Mathf.Max(.1f,instance.scale));while(lod<def.lods.Length-1 && normalized>def.lods[lod].distance)lod++;
   VisibleInstances++;foreach(var batch in batches[instance.species][lod]){
    batch.matrices[batch.count++]=instance.matrix*batch.part.localToTree;if(batch.count==batch.matrices.Length)Draw(batch);
   }
  }}
  foreach(var species in batches)foreach(var lod in species)foreach(var batch in lod)if(batch.count>0)Draw(batch);
 }
 void Draw(Batch batch){
  if(batch.part.mesh!=null && batch.part.material!=null){
   var shadows=batch.lod<2?ShadowCastingMode.On:ShadowCastingMode.Off;
   if(SystemInfo.supportsInstancing){Graphics.DrawMeshInstanced(batch.part.mesh,batch.part.submesh,batch.part.material,batch.matrices,batch.count,null,shadows,true,0,viewer,LightProbeUsage.Off);SubmittedDrawCalls++;}
   else{for(int i=0;i<batch.count;i++){Graphics.DrawMesh(batch.part.mesh,batch.matrices[i],batch.part.material,0,viewer,batch.part.submesh,null,shadows,true,null,LightProbeUsage.Off);SubmittedDrawCalls++;}}
  }
  batch.count=0;
 }
}
}

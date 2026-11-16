using System.Collections.Generic;
using UnityEngine;
namespace TrainSummer {
// Ten prewarmed tiles: seven around the train plus up to three around a stranded player.
public sealed class RouteChunks:MonoBehaviour {
 public TrainMotor train;public FirstPersonMotor player;public VegetationRenderer vegetation;public GameObject ruinPrefab;
 public Material grass,wood,steel;
 const int Size=EnvironmentLayout.TileSize,Segments=40,Trees=EnvironmentLayout.TreesPerTile;
 sealed class Tile {
  public GameObject root;public Transform ground,ruin;public Mesh rails,sleepers,collision;
  public MeshCollider trackCollider;public VegetationTile plants;public Transform[] trunks=new Transform[Trees];
  public readonly CombineInstance[] combine=new CombineInstance[2];
 }
 readonly Dictionary<int,Tile> active=new Dictionary<int,Tile>();readonly Stack<Tile> pool=new Stack<Tile>();readonly List<int> expired=new List<int>(),wanted=new List<int>(10);
 readonly List<Tile> all=new List<Tile>();readonly TrackMeshBuilder railBuilder=new TrackMeshBuilder(80),sleeperBuilder=new TrackMeshBuilder(40);
 int center=int.MinValue,footCenter=int.MinValue;
 public int ActiveCount=>active.Count;public int CreatedCount {get;private set;}
 void Start(){for(int i=0;i<EnvironmentLayout.PoolCapacity;i++)pool.Push(Build());Refresh();}
 void Update(){Refresh();}
 void Refresh(){
  if(train==null || train.route==null)return;
  int trainIndex=Mathf.FloorToInt(train.Distance/Size),playerIndex=player!=null?Mathf.FloorToInt(train.route.ProjectDistance(player.transform.position)/Size):trainIndex;
  if(trainIndex==center && playerIndex==footCenter)return;center=trainIndex;footCenter=playerIndex;EnvironmentLayout.Window(center,footCenter,wanted);
  expired.Clear();foreach(var entry in active)if(!wanted.Contains(entry.Key))expired.Add(entry.Key);
  foreach(int index in expired){var tile=active[index];tile.root.SetActive(false);tile.plants.active=false;pool.Push(tile);active.Remove(index);}
  foreach(int index in wanted)if(!active.ContainsKey(index)){var tile=pool.Pop();Configure(tile,index);tile.root.SetActive(true);tile.plants.active=true;active.Add(index,tile);}
 }
 Transform Box(Transform parent,string name,Vector3 size,Material material){
  var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;obj.transform.SetParent(parent,false);obj.transform.localScale=size;obj.GetComponent<Renderer>().sharedMaterial=material;return obj.transform;
 }
 Mesh RenderMesh(Transform parent,string name,Material material){
  var obj=new GameObject(name);obj.transform.SetParent(parent,false);var mesh=new Mesh{name=name};mesh.MarkDynamic();obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=material;return mesh;
 }
 Tile Build(){
  CreatedCount++;var tile=new Tile();all.Add(tile);tile.root=new GameObject("Route Tile");tile.root.transform.SetParent(transform,false);
  tile.ground=Box(tile.root.transform,"Ground",new Vector3(160,.6f,80),grass);
  tile.rails=RenderMesh(tile.root.transform,"Combined Rails",steel);tile.sleepers=RenderMesh(tile.root.transform,"Combined Sleepers",wood);
  tile.collision=new Mesh{name="Track Collision"};tile.trackCollider=new GameObject("Track Collider").AddComponent<MeshCollider>();tile.trackCollider.transform.SetParent(tile.root.transform,false);
  tile.combine[0]=new CombineInstance{mesh=tile.rails,transform=Matrix4x4.identity};tile.combine[1]=new CombineInstance{mesh=tile.sleepers,transform=Matrix4x4.identity};
  tile.plants=vegetation.Register(Trees+EnvironmentLayout.GrassPerTile);
  for(int i=0;i<Trees;i++){
   var trunk=new GameObject("Tree Trunk Collider").transform;trunk.SetParent(tile.root.transform,false);var capsule=trunk.gameObject.AddComponent<CapsuleCollider>();capsule.center=Vector3.up*2.3f;capsule.height=4.6f;capsule.radius=.25f;tile.trunks[i]=trunk;
  }
  if(ruinPrefab!=null)tile.ruin=Instantiate(ruinPrefab,tile.root.transform).transform;
  tile.root.SetActive(false);return tile;
 }
 void Configure(Tile tile,int index){
  var route=train.route;float start=index*Size;var a=route.Evaluate(start);tile.root.name="Route_"+index;tile.root.transform.SetPositionAndRotation(a.position,Quaternion.identity);
  var bounds=new Bounds(a.position,Vector3.zero);
  for(int i=0;i<=8;i++){var pose=route.Evaluate(start+Size*i/8f);Vector3 edge=pose.rotation*Vector3.right*80;bounds.Encapsulate(pose.position+edge);bounds.Encapsulate(pose.position-edge);}
  tile.ground.position=bounds.center-Vector3.up*.3f;tile.ground.localScale=new Vector3(bounds.size.x+.2f,.6f,bounds.size.z+.2f);
  railBuilder.Clear();sleeperBuilder.Clear();
  for(int i=0;i<Segments;i++){
   var p=route.Evaluate(start+i*2);var q=route.Evaluate(start+(i+1)*2);
   sleeperBuilder.Box(p.position-a.position+Vector3.up*.08f,p.rotation,new Vector3(2.8f,.16f,.25f));
   for(int side=0;side<2;side++){
    float offset=side==0?-.8f:.8f;Vector3 from=p.position+p.rotation*Vector3.right*offset,to=q.position+q.rotation*Vector3.right*offset;
    railBuilder.Box((from+to)*.5f-a.position+Vector3.up*.22f,Quaternion.LookRotation(to-from),new Vector3(.12f,.16f,Vector3.Distance(from,to)+.035f));
   }
  }
  railBuilder.Apply(tile.rails);sleeperBuilder.Apply(tile.sleepers);tile.trackCollider.sharedMesh=null;tile.collision.Clear();tile.collision.CombineMeshes(tile.combine,true,false);tile.trackCollider.sharedMesh=tile.collision;
  for(int i=0;i<Trees;i++){
   var item=EnvironmentLayout.Tree(index,i,train.stops);var pose=route.Evaluate(item.distance);Vector3 position=pose.position+pose.rotation*Vector3.right*item.offset;Quaternion rotation=pose.rotation*Quaternion.Euler(0,item.yaw,0);
   tile.plants.instances[i]=vegetation.Place(item.species,position,rotation,item.scale);tile.trunks[i].SetPositionAndRotation(position,rotation);tile.trunks[i].localScale=Vector3.one*item.scale;
  }
  for(int i=0;i<EnvironmentLayout.GrassPerTile;i++){
   var item=EnvironmentLayout.Grass(index,i,train.stops);var pose=route.Evaluate(item.distance);Vector3 position=pose.position+pose.rotation*Vector3.right*item.offset;
   tile.plants.instances[Trees+i]=vegetation.Place(2,position,pose.rotation*Quaternion.Euler(0,item.yaw,0),item.scale);
  }
  if(tile.ruin!=null){var item=EnvironmentLayout.Ruin(index,train.stops);tile.ruin.gameObject.SetActive(item.active);if(item.active){var pose=route.Evaluate(item.distance);tile.ruin.SetPositionAndRotation(pose.position+pose.rotation*Vector3.right*item.offset,pose.rotation*Quaternion.Euler(0,item.yaw,0));}}
 }
 void OnDestroy(){foreach(var tile in all){if(tile.rails!=null)Destroy(tile.rails);if(tile.sleepers!=null)Destroy(tile.sleepers);if(tile.collision!=null)Destroy(tile.collision);}}
}
}

using System.Collections.Generic;
using UnityEngine;
namespace TrainSummer {
// Fixed pool of seven tiles. Repositioning never instantiates/destroys scene objects.
public sealed class RouteChunks:MonoBehaviour {
 public TrainMotor train; public Material grass,wood,leaves,steel;
 const int Size=80,Segments=40,Trees=28;
 sealed class Tile {
  public GameObject root;public Transform ground;
  public readonly Transform[] sleepers=new Transform[Segments],rails=new Transform[Segments*2],trees=new Transform[Trees];
 }
 readonly Dictionary<int,Tile> active=new Dictionary<int,Tile>();
 readonly Stack<Tile> pool=new Stack<Tile>();readonly List<int> expired=new List<int>();int center=int.MinValue;
 public int ActiveCount=>active.Count;
 public int CreatedCount {get;private set;}
 void Start(){for(int i=0;i<7;i++)pool.Push(Build());Refresh();}
 void Update(){Refresh();}
 void Refresh(){
  if(train==null || train.route==null)return;
  int next=Mathf.FloorToInt(train.Distance/Size);if(next==center)return;center=next;
  expired.Clear();foreach(var pair in active)if(pair.Key<center-2||pair.Key>center+4)expired.Add(pair.Key);
  foreach(int i in expired){active[i].root.SetActive(false);pool.Push(active[i]);active.Remove(i);}
  for(int i=center-2;i<=center+4;i++)if(!active.ContainsKey(i)){
   // Seven active indices always fit seven prewarmed tiles, including teleports.
   Tile tile=pool.Pop();Configure(tile,i);tile.root.SetActive(true);active.Add(i,tile);
  }
 }
 Transform Box(Transform parent,string name,Vector3 size,Material material){
  var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;obj.transform.SetParent(parent,false);obj.transform.localScale=size;obj.GetComponent<Renderer>().sharedMaterial=material;return obj.transform;
 }
 Tile Build(){
  CreatedCount++;var tile=new Tile();tile.root=new GameObject("Route Tile");tile.root.transform.SetParent(transform,false);
  tile.ground=Box(tile.root.transform,"Ground",new Vector3(160,.6f,80),grass);
  for(int i=0;i<Segments;i++){
   tile.sleepers[i]=Box(tile.root.transform,"Sleeper",new Vector3(2.8f,.16f,.25f),wood);
   for(int side=0;side<2;side++)tile.rails[i*2+side]=Box(tile.root.transform,"Rail",new Vector3(.12f,.16f,2.04f),steel);
  }
  for(int i=0;i<Trees;i++){
   var tree=new GameObject("Tree").transform;tree.SetParent(tile.root.transform,false);tile.trees[i]=tree;
   Box(tree,"Trunk",new Vector3(.45f,5,.45f),wood).localPosition=Vector3.up*2.5f;
   var crown=GameObject.CreatePrimitive(PrimitiveType.Sphere);crown.transform.SetParent(tree,false);crown.transform.localPosition=Vector3.up*6;crown.transform.localScale=new Vector3(4,5,4);crown.GetComponent<Renderer>().sharedMaterial=leaves;
   // Disable immediately; destruction occurs only during pool warmup.
   crown.GetComponent<Collider>().enabled=false;Destroy(crown.GetComponent<Collider>());
  }
  tile.root.SetActive(false);return tile;
 }
 void Configure(Tile tile,int index){
  var route=train.route;float start=index*Size;var a=route.Evaluate(start);
  tile.root.name="Route_"+index;tile.root.transform.position=a.position;tile.root.transform.rotation=Quaternion.identity;
  var bounds=new Bounds(a.position,Vector3.zero);
  for(int i=0;i<=8;i++){
   var pose=route.Evaluate(start+Size*i/8f);Vector3 edge=pose.rotation*Vector3.right*80;
   bounds.Encapsulate(pose.position+edge);bounds.Encapsulate(pose.position-edge);
  }
  tile.ground.position=bounds.center-Vector3.up*.3f;
  tile.ground.localScale=new Vector3(bounds.size.x+.2f,.6f,bounds.size.z+.2f);
  for(int i=0;i<Segments;i++){
   var p=route.Evaluate(start+i*2);var q=route.Evaluate(start+(i+1)*2);
   tile.sleepers[i].SetPositionAndRotation(p.position+Vector3.up*.08f,p.rotation);
   for(int side=0;side<2;side++){
    float x=side==0?-.8f:.8f;Vector3 from=p.position+p.rotation*Vector3.right*x,to=q.position+q.rotation*Vector3.right*x;
    var rail=tile.rails[i*2+side];rail.SetPositionAndRotation((from+to)*.5f+Vector3.up*.22f,Quaternion.LookRotation(to-from));rail.localScale=new Vector3(.12f,.16f,Vector3.Distance(from,to)+.035f);
   }
  }
  var random=new System.Random(unchecked(index*73856093+725));
  for(int i=0;i<Trees;i++){
   float side=i%2==0?-1:1;float offset=side*(14+(float)random.NextDouble()*52);float s=start+(float)random.NextDouble()*Size;
   foreach(float stop in train.stops)if(side>0 && Mathf.Abs(s-stop)<26 && offset<25)offset+=25;
   var pose=route.Evaluate(s);tile.trees[i].position=pose.position+pose.rotation*Vector3.right*offset;
  }
 }
}
}

using System.Collections.Generic;
using UnityEngine;
namespace TrainSummer {
// Small deterministic greybox streamer: not additive scene loading yet.
public sealed class RouteChunks:MonoBehaviour {
 public TrainMotor train; public Material grass,wood,leaves,steel;
 const int Size=80; readonly Dictionary<int,GameObject> active=new Dictionary<int,GameObject>();
 readonly List<int> expired=new List<int>();int center=int.MinValue;
 void Update(){
  int next=Mathf.FloorToInt(train.transform.position.z/Size);if(next==center)return;center=next;
  for(int i=center-2;i<=center+4;i++)if(!active.ContainsKey(i))active.Add(i,Build(i));
  expired.Clear();foreach(var pair in active)if(pair.Key<center-2||pair.Key>center+4)expired.Add(pair.Key);
  foreach(int i in expired){Destroy(active[i]);active.Remove(i);}
 }
 GameObject Box(Transform root,string name,Vector3 pos,Vector3 scale,Material mat){
  var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(root,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;return g;
 }
 GameObject Build(int index){
  var root=new GameObject("Route_"+index);root.transform.SetParent(transform,false);root.transform.position=new Vector3(0,0,index*Size);
  Box(root.transform,"Ground",new Vector3(0,-.3f,40),new Vector3(160,.6f,80),grass);
  for(int i=0;i<40;i++)Box(root.transform,"Sleeper",new Vector3(0,.08f,i*2),new Vector3(2.8f,.16f,.25f),wood);
  foreach(float x in new[]{-.8f,.8f})Box(root.transform,"Rail",new Vector3(x,.22f,40),new Vector3(.12f,.16f,80),steel);
  var random=new System.Random(unchecked(index*73856093+725));
  for(int i=0;i<28;i++){
   float x=(i%2==0?-1:1)*(12+(float)random.NextDouble()*55),z=(float)random.NextDouble()*80;
   var tree=new GameObject("Tree");tree.transform.SetParent(root.transform,false);tree.transform.localPosition=new Vector3(x,0,z);
   Box(tree.transform,"Trunk",new Vector3(0,2.5f,0),new Vector3(.45f,5,.45f),wood);
   var crown=GameObject.CreatePrimitive(PrimitiveType.Sphere);crown.transform.SetParent(tree.transform,false);crown.transform.localPosition=Vector3.up*6;crown.transform.localScale=new Vector3(4,5,4);crown.GetComponent<Renderer>().sharedMaterial=leaves;Destroy(crown.GetComponent<Collider>());
  }
  return root;
 }
}
}

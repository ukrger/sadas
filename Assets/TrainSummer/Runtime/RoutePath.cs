using UnityEngine;
namespace TrainSummer {
// Arc-length table keeps speed independent of curvature. Heights remain flat in 0.3.
[DefaultExecutionOrder(-200)]
public sealed class RoutePath:MonoBehaviour {
 public float forwardExtent=2200, amplitude=24, wavelength=150;
 float[] lengths; Vector3[] points;
 public float Length {get{EnsureBuilt();return lengths[lengths.Length-1];}}
 public struct Pose {public Vector3 position;public Quaternion rotation;public Pose(Vector3 p,Quaternion q){position=p;rotation=q;}}
 void Awake(){Rebuild();}
 public void Rebuild(){
  int count=Mathf.CeilToInt(Mathf.Max(100,forwardExtent)/2)+1;
  lengths=new float[count];points=new Vector3[count];
  for(int i=0;i<count;i++){
   float z=Mathf.Max(100,forwardExtent)*i/(count-1);
   points[i]=new Vector3(amplitude*(1-Mathf.Cos(z/Mathf.Max(20,wavelength))),0,z);
   if(i>0)lengths[i]=lengths[i-1]+Vector3.Distance(points[i-1],points[i]);
  }
 }
 void EnsureBuilt(){if(lengths==null)Rebuild();}
 public Pose Evaluate(float distance){
  EnsureBuilt();int last=points.Length-1;
  if(distance<0)return Make(points[0]+(points[1]-points[0]).normalized*distance,points[1]-points[0]);
  if(distance>=lengths[last])return Make(points[last]+(points[last]-points[last-1]).normalized*(distance-lengths[last]),points[last]-points[last-1]);
  int low=0,high=last;while(high-low>1){int mid=(low+high)/2;if(lengths[mid]<=distance)low=mid;else high=mid;}
  float t=(distance-lengths[low])/(lengths[high]-lengths[low]);
  Vector3 pos=Vector3.Lerp(points[low],points[high],t);
  float slope=amplitude/Mathf.Max(20,wavelength)*Mathf.Sin(pos.z/Mathf.Max(20,wavelength));
  return Make(pos,new Vector3(slope,0,1));
 }
 Pose Make(Vector3 position,Vector3 tangent){return new Pose(transform.TransformPoint(position),transform.rotation*Quaternion.LookRotation(tangent,Vector3.up));}
}
}

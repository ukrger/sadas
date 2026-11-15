using UnityEngine;
namespace TrainSummer {
[DefaultExecutionOrder(-100)]
public sealed class TrainMotor : MonoBehaviour {
 public float cruiseSpeed=8f, acceleration=1.5f, stationDistance=160f;
 public float Speed {get;private set;} public float Fuel {get;private set;}=30f;
 public bool Parked {get;private set;} public bool Arrived {get;private set;}
 public float Distance {get;private set;}
 public bool PlayerAboard {get;set;}
 bool brake;
 public void AddFuel(float value){Fuel+=Mathf.Max(0,value);}
 public bool Depart(){if(!PlayerAboard || Fuel<=0)return false; Parked=false; brake=false; return true;}
 public void ToggleBrake(){if(Parked)Depart();else brake=!brake;}
 void Update(){
  float remaining=stationDistance-Distance;
  float target=(brake || Fuel<=0 || Parked)?0:cruiseSpeed;
  if(!Arrived) target=Mathf.Min(target,Mathf.Sqrt(2*acceleration*Mathf.Max(0,remaining)));
  Speed=Mathf.MoveTowards(Speed,target,acceleration*Time.deltaTime);
  float step=Speed*Time.deltaTime;
  if(!Arrived && step>=remaining){step=Mathf.Max(0,remaining);Arrived=true;Parked=true;Speed=0;}
  Distance+=step;transform.position+=Vector3.forward*step;
  Fuel=Mathf.Max(0,Fuel-step*0.015f);
 }
}
}

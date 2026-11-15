using UnityEngine;
namespace TrainSummer {
[DefaultExecutionOrder(-100)]
public sealed class TrainMotor:MonoBehaviour {
 public RoutePath route;public TrainMaintenance maintenance;
 public float cruiseSpeed=8,acceleration=1.5f,startingFuel=30;
 public float[] stops={160,640,1200,1800};
 public float Speed {get;private set;}
 public float Fuel {get;private set;}=30;
 public float Distance {get;private set;}
 public bool Parked {get;private set;}
 public bool PlayerAboard {get;set;}
 public int StopIndex {get;private set;}
 public bool Finished=>route!=null && Distance>=route.Length-.01f;
 public float NextStopDistance=>route==null?0:(StopIndex<stops.Length?Mathf.Min(stops[StopIndex],route.Length):route.Length);
 bool brake;
 void Awake(){Fuel=Mathf.Max(0,startingFuel);}
 void Start(){if(route!=null)ApplyPose();}
 void OnValidate(){
  cruiseSpeed=Mathf.Max(.1f,cruiseSpeed);acceleration=Mathf.Max(.1f,acceleration);
  if(stops==null)stops=new float[0];
  for(int i=0;i<stops.Length;i++)stops[i]=Mathf.Max(i==0?10:stops[i-1]+30,stops[i]);
 }
 public void AddFuel(float value){Fuel+=Mathf.Max(0,value);}
 public bool Depart(){
  if(!PlayerAboard || Fuel<=0 || Finished || (maintenance!=null && maintenance.Broken))return false;
  if(Parked)StopIndex++;Parked=false;brake=false;return true;
 }
 public void ToggleBrake(){if(Parked)Depart();else brake=!brake;}
 void Update(){Tick(Time.deltaTime);}
 public void Tick(float dt){
  if(route==null || dt<=0)return;
  // A breakdown or empty tank latches the brake; service must not restart an unattended train.
  if(Fuel<=0 || (maintenance!=null && maintenance.Broken))brake=true;
  float remaining=Mathf.Max(0,NextStopDistance-Distance);
  float target=(brake || Fuel<=0 || Parked || Finished || (maintenance!=null && maintenance.Broken))?0:Mathf.Min(cruiseSpeed,Mathf.Sqrt(2*acceleration*remaining));
  Speed=Mathf.MoveTowards(Speed,target,acceleration*dt);
  float step=Speed*dt;
  if(!Parked && !Finished && step>=remaining){step=remaining;Parked=true;Speed=0;}
  Distance+=step;Fuel=Mathf.Max(0,Fuel-step*.015f);if(maintenance!=null)maintenance.ApplyWear(step);ApplyPose();
 }
 void ApplyPose(){var pose=route.Evaluate(Distance);transform.SetPositionAndRotation(pose.position,pose.rotation);}
}
}

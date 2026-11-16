using System;
namespace TrainSummer {
// Engine-independent transition and accuracy rules used by all three firearms.
public sealed class AimModel {
 public float Blend {get;private set;}
 public void Reset(){Blend=0;}
 public void Advance(float deltaTime,bool held,bool allowed,float response=18){
  if(float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime<0 || float.IsNaN(response) || float.IsInfinity(response) || response<=0)return;
  float target=held && allowed?1:0;Blend+=(target-Blend)*(1-(float)Math.Exp(-response*deltaTime));
  if(Math.Abs(target-Blend)<.0001f)Blend=target;
 }
 public float Spread(float hipDegrees,float aimedDegrees)=>hipDegrees+(aimedDegrees-hipDegrees)*Blend;
 public float Sensitivity=>1-.5f*Blend;
 public float Movement=>1-.3f*Blend;
}
}

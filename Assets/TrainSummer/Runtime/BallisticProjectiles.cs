using System.Collections.Generic;
using UnityEngine;
namespace TrainSummer {
public sealed class BallisticProjectiles:MonoBehaviour {
 public Material tracerMaterial;const int Capacity=64;
 sealed class Bullet {public bool active;public Vector3 position,velocity;public float age,damage;public int mask;public LineRenderer tracer;}
 readonly Bullet[] bullets=new Bullet[Capacity];readonly List<MonoBehaviour> candidates=new List<MonoBehaviour>(8);
 void Awake(){for(int i=0;i<Capacity;i++){
  var obj=new GameObject("Pooled Tracer");obj.transform.SetParent(transform,false);var line=obj.AddComponent<LineRenderer>();line.positionCount=2;line.useWorldSpace=true;line.startWidth=.009f;line.endWidth=.003f;line.sharedMaterial=tracerMaterial;line.enabled=false;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
  bullets[i]=new Bullet{tracer=line};
 }}
 public bool Fire(Vector3 origin,Vector3 direction,float speed,float damage,int mask){
  foreach(var b in bullets)if(!b.active){b.active=true;b.position=origin;b.velocity=direction.normalized*speed;b.age=0;b.damage=damage;b.mask=mask;b.tracer.enabled=true;b.tracer.SetPosition(0,origin);b.tracer.SetPosition(1,origin);return true;}return false;
 }
 public bool FirePellets(Vector3 origin,Quaternion aim,int count,float spread,float speed,float damage,int mask){
  if(count<=0 || count>Capacity)return false;int free=0;foreach(var bullet in bullets)if(bullet!=null && !bullet.active)free++;
  if(free<count)return false; // Reserve the complete volley before spending a shell.
  for(int i=0;i<count;i++){
   Vector2 disk=Random.insideUnitCircle*Mathf.Tan(Mathf.Clamp(spread,0,15)*Mathf.Deg2Rad);
   Fire(origin,aim*new Vector3(disk.x,disk.y,1).normalized,speed,damage,mask);
  }
  return true;
 }
 public void Clear(){foreach(var b in bullets)if(b!=null){b.active=false;b.tracer.enabled=false;}}
 void Update(){float dt=Time.deltaTime;if(dt<=0)return;
  foreach(var b in bullets){if(!b.active)continue;Vector3 previous=b.position;int steps=Mathf.Clamp(Mathf.CeilToInt(dt/.02f),1,16);float step=dt/steps;
   for(int i=0;i<steps && b.active;i++){
    Vector3 delta=b.velocity*step+Physics.gravity*(.5f*step*step);float length=delta.magnitude;
    if(length>0 && Physics.Raycast(b.position,delta/length,out var hit,length,b.mask,QueryTriggerInteraction.Ignore)){
     candidates.Clear();hit.collider.GetComponentsInParent<MonoBehaviour>(false,candidates);
     foreach(var c in candidates)if(c is IDamageable target){target.ApplyDamage(b.damage);break;}
     b.position=hit.point;b.active=false;
    }else b.position+=delta;
    b.velocity+=Physics.gravity*step;b.age+=step;if(b.age>3)b.active=false;
   }
   b.tracer.SetPosition(0,previous);b.tracer.SetPosition(1,b.position);b.tracer.enabled=b.active;
  }
 }
}
}

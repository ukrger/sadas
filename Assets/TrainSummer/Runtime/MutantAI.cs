using UnityEngine;
using UnityEngine.AI;
namespace TrainSummer {
[RequireComponent(typeof(NavMeshAgent))]
public sealed class MutantAI:MonoBehaviour,IDamageable {
 public float startingHealth=100,sightRange=20,hearingRange=30,damage=10;
 public Vector3 home;public PlayerVitals target;public FirstPersonMotor player;public PlayerFlashlight flashlight;public Animator animator;
 public float Health {get;private set;}=100;
 NavMeshAgent agent;Vector3 investigate;float alertUntil,nextThink,nextAttack;bool heard,reportedNavigationError;
 void Awake(){agent=GetComponent<NavMeshAgent>();Health=startingHealth;}
 void OnEnable(){NoiseBus.Heard+=Hear;}
 void OnDisable(){NoiseBus.Heard-=Hear;}
 void Hear(Vector3 point,float radius){if(Health<=0 || Vector3.Distance(transform.position,point)>Mathf.Min(radius,hearingRange))return;investigate=point;alertUntil=Time.time+8;heard=true;}
 bool ClearSight(){
  Vector3 from=transform.position+Vector3.up*1.5f,to=player.transform.position+Vector3.up*.9f;
  if(Physics.Raycast(from,(to-from).normalized,out var hit,Vector3.Distance(from,to),~((1<<8)|(1<<9)),QueryTriggerInteraction.Ignore))return hit.collider.GetComponentInParent<PlayerVitals>()==target;
  return false;
 }
 void Update(){
  if(Time.deltaTime<=0 || Health<=0 || player==null || target==null)return;
  if(!agent.isOnNavMesh){
   if(NavMesh.SamplePosition(transform.position,out var spawn,4,NavMesh.AllAreas)){agent.Warp(spawn.position);}
   else {if(!reportedNavigationError){Debug.LogWarning("Mutant has no station NavMesh: "+name,this);reportedNavigationError=true;}return;}
  }
  if(animator!=null)animator.SetFloat("Speed",agent.velocity.magnitude);
  if(Time.time<nextThink)return;nextThink=Time.time+.2f;
  if(!target.Alive){agent.isStopped=true;return;}
  float distance=Vector3.Distance(transform.position,player.transform.position);bool sight=false;
  float visibility=RenderSettings.sun!=null?Mathf.Lerp(.55f,1,Mathf.Clamp01(RenderSettings.sun.intensity)):1;
  if(distance<sightRange*visibility){float angle=Vector3.Angle(transform.forward,player.transform.position-transform.position);if(angle<65)sight=ClearSight();}
  if(!sight && flashlight!=null && flashlight.lamp.enabled && distance<flashlight.lamp.range){
   float angle=Vector3.Angle(flashlight.lamp.transform.forward,transform.position+Vector3.up-player.view.transform.position);
   if(angle<flashlight.lamp.spotAngle*.5f)sight=ClearSight();
  }
  if(sight){investigate=player.transform.position;heard=true;alertUntil=Time.time+6;}
  if(sight && distance<1.65f){
   agent.isStopped=true;Vector3 flat=player.transform.position-transform.position;flat.y=0;if(flat.sqrMagnitude>.01f)transform.rotation=Quaternion.LookRotation(flat);
   if(Time.time>=nextAttack){target.ApplyDamage(damage);nextAttack=Time.time+1.3f;}return;
  }
  agent.isStopped=false;
  Vector3 destination=heard && Time.time<alertUntil?investigate:home+new Vector3(Mathf.Sin(Time.time*.08f+home.x)*6,0,Mathf.Cos(Time.time*.08f+home.z)*6);
  if(NavMesh.SamplePosition(destination,out var point,3,NavMesh.AllAreas))agent.SetDestination(point.position);
 }
 public void ApplyDamage(float amount){
  if(amount<=0 || Health<=0)return;Health=Mathf.Max(0,Health-amount);
  if(Health<=0){agent.enabled=false;gameObject.SetActive(false);}
  else if(player!=null){Hear(player.transform.position,100);}
 }
 public void RestoreState(float health,Vector3 position,Quaternion rotation){
  Health=health;heard=false;alertUntil=0;nextAttack=Time.time+1;
  if(agent==null)agent=GetComponent<NavMeshAgent>();agent.enabled=false;transform.SetPositionAndRotation(position,rotation);
  if(health>0)agent.enabled=true;
 }
}
}

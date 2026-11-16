using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainSummer {
[DefaultExecutionOrder(300)]
public sealed class PlayerMelee:MonoBehaviour {
 public FirstPersonMotor player;public PrototypeUI ui;public ResourceInventory inventory;public PlayerVitals vitals;public Transform weaponPivot;
 public float damage=25,reach=1.8f,swingDuration=.55f,cooldown=.7f;
 readonly List<MonoBehaviour> candidates=new List<MonoBehaviour>(8);
 bool swinging,hitResolved;float startedAt,nextSwing;
 Vector3 idlePosition;Quaternion idleRotation;
 void Start(){if(weaponPivot!=null){idlePosition=weaponPivot.localPosition;idleRotation=weaponPivot.localRotation;}}
 void Update(){
  if(weaponPivot==null || inventory==null || player==null)return;
  var mouse=Mouse.current;
  bool allowed=player.Locked && (ui==null || !ui.InventoryOpen) && (vitals==null || vitals.Alive);
  bool visible=inventory.SelectedWeapon==WeaponSlot.Axe && inventory.HasAxe && (vitals==null || vitals.Alive);weaponPivot.gameObject.SetActive(visible);
  if(!visible || !allowed){swinging=false;ResetPose();return;}
  if(mouse!=null && mouse.leftButton.wasPressedThisFrame && !swinging && Time.time>=nextSwing){
   swinging=true;hitResolved=false;startedAt=Time.time;nextSwing=Time.time+Mathf.Max(cooldown,swingDuration);
  }
  if(!swinging){ResetPose();return;}
  float t=(Time.time-startedAt)/Mathf.Max(.1f,swingDuration);
  float phase=Mathf.Sin(Mathf.Clamp01(t)*Mathf.PI);
  weaponPivot.localPosition=idlePosition+new Vector3(-.18f,-.07f,.1f)*phase;
  weaponPivot.localRotation=idleRotation*Quaternion.Euler(65*phase,-25*phase,-35*phase);
  if(!hitResolved && t>=.4f){hitResolved=true;ResolveHit();}
  if(t>=1){swinging=false;ResetPose();}
 }
 public void Cancel(){swinging=false;hitResolved=true;}
 void ResetPose(){weaponPivot.localPosition=idlePosition;weaponPivot.localRotation=idleRotation;}
 void ResolveHit(){
  // First contact wins, so walls occlude damage. Player layer is excluded.
  if(!Physics.SphereCast(player.view.transform.position,.12f,player.view.transform.forward,out var hit,reach,~(1<<player.gameObject.layer),QueryTriggerInteraction.Ignore))return;
  candidates.Clear();hit.collider.GetComponentsInParent<MonoBehaviour>(false,candidates);
  foreach(var component in candidates)if(component is IDamageable target){target.ApplyDamage(damage);break;}
 }
}
}

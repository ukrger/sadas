using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainSummer {
[DefaultExecutionOrder(420)]public sealed class WeaponPresentation:MonoBehaviour {
 public FirstPersonMotor player;public PrototypeUI ui;public ResourceInventory inventory;
 public Transform pistol,shotgun,rifle,pistolSight,shotgunSight,rifleSight;
 [Range(40,75)]public float aimedFov=58;public float eyeRelief=.23f;
 readonly AimModel aim=new AimModel();Vector2 sway;float normalFov;WeaponSlot previous;
 sealed class Pose {public Transform root,sight;public Vector3 hipPosition;public Quaternion hipRotation;}
 PlayerPistol pistolController;PlayerShotgun shotgunController;PlayerRifle rifleController;
 void Awake(){pistolController=GetComponent<PlayerPistol>();shotgunController=GetComponent<PlayerShotgun>();rifleController=GetComponent<PlayerRifle>();}
 Pose[] poses;
 public float AimBlend=>aim.Blend;
 void Start(){normalFov=player.view.fieldOfView;poses=new[]{Make(pistol,pistolSight),Make(shotgun,shotgunSight),Make(rifle,rifleSight)};}
 Pose Make(Transform root,Transform sight)=>new Pose{root=root,sight=sight,hipPosition=root!=null?root.localPosition:Vector3.zero,hipRotation=root!=null?root.localRotation:Quaternion.identity};
 public void ResetView(){aim.Reset();sway=Vector2.zero;if(poses!=null)foreach(var pose in poses)if(pose.root!=null){pose.root.localPosition=pose.hipPosition;pose.root.localRotation=pose.hipRotation;}if(player!=null){player.LookMultiplier=1;player.MovementMultiplier=1;if(normalFov>0)player.view.fieldOfView=normalFov;}}
 public float SpreadDegrees(float hip,float aimed)=>aim.Spread(hip,aimed);
 public Vector3 ShotDirection(float hipDegrees,float aimedDegrees){
  float angle=SpreadDegrees(hipDegrees,aimedDegrees);Vector2 disk=Random.insideUnitCircle*Mathf.Tan(Mathf.Clamp(angle,0,15)*Mathf.Deg2Rad);
  return player.view.transform.TransformDirection(new Vector3(disk.x,disk.y,1).normalized);
 }
 public void Kick(float amount){if(player!=null){float kick=amount*Mathf.Lerp(1,.75f,aim.Blend);player.AddRecoil(kick,Random.Range(-kick*.18f,kick*.18f));}}
 public static void SightPose(Transform sight,float relief,out Vector3 position,out Quaternion rotation){
  // Marker is expressed directly in the weapon pivot's coordinates, facing along the sight line.
  rotation=Quaternion.Inverse(sight.localRotation);position=new Vector3(0,0,relief)-rotation*sight.localPosition;
 }
 void LateUpdate(){
  if(GameMenu.Paused)return;
  var selected=inventory.SelectedWeapon;bool switched=previous!=selected;previous=selected;
  Pose pose=poses==null?null:(selected==WeaponSlot.Pistol?poses[0]:(selected==WeaponSlot.Shotgun?poses[1]:(selected==WeaponSlot.Rifle?poses[2]:null)));
  bool valid=pose!=null && pose.root!=null && pose.sight!=null && pose.root.gameObject.activeSelf && player.Locked && player.InputReady && !ui.InventoryOpen && player.vitals.Alive;
  bool reloading=(selected==WeaponSlot.Pistol && pistolController!=null && pistolController.Reloading) || (selected==WeaponSlot.Shotgun && shotgunController!=null && shotgunController.Reloading) || (selected==WeaponSlot.Rifle && rifleController!=null && rifleController.Reloading);
  if(switched || !valid){ResetView();if(!valid)return;}
  var mouse=Mouse.current;aim.Advance(Time.deltaTime,mouse!=null && mouse.rightButton.isPressed,valid && !reloading);
  player.view.fieldOfView=Mathf.Lerp(normalFov,aimedFov,aim.Blend);player.LookMultiplier=aim.Sensitivity;player.MovementMultiplier=aim.Movement;
  sway=Vector2.Lerp(sway,mouse!=null?Vector2.ClampMagnitude(mouse.delta.ReadValue()*.00015f,.012f):Vector2.zero,Mathf.Clamp01(Time.deltaTime*12));
  var keyboard=Keyboard.current;bool moving=keyboard!=null && (keyboard.wKey.isPressed || keyboard.aKey.isPressed || keyboard.sKey.isPressed || keyboard.dKey.isPressed);
  float bob=moving?Mathf.Sin(Time.time*10)*.006f:Mathf.Sin(Time.time*1.5f)*.0015f;
  Vector3 delta=pose.root.localPosition-pose.hipPosition;Quaternion rotationDelta=Quaternion.Inverse(pose.hipRotation)*pose.root.localRotation;
  SightPose(pose.sight,eyeRelief,out var aimedPosition,out var aimedRotation);
  pose.root.localPosition=Vector3.Lerp(pose.hipPosition,aimedPosition,aim.Blend)+delta*Mathf.Lerp(1,.45f,aim.Blend)+new Vector3(-sway.x,-sway.y+bob,0)*(1-aim.Blend*.85f);
  pose.root.localRotation=Quaternion.Slerp(pose.hipRotation,aimedRotation,aim.Blend)*Quaternion.Slerp(rotationDelta,Quaternion.identity,aim.Blend*.55f);
 }
 void OnDisable(){ResetView();}
}
}

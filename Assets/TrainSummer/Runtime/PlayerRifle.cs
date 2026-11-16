using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainSummer {
[DefaultExecutionOrder(297)]public sealed class PlayerRifle:MonoBehaviour {
 public FirstPersonMotor player;public PrototypeUI ui;public ResourceInventory inventory;public Transform pivot;public BallisticProjectiles projectiles;public ProceduralAudio audioEffects;public WeaponPresentation presentation;
 public float damage=26,bulletSpeed=430,shotInterval=.11f,reloadSeconds=2.1f;
 public bool Reloading=>reloading;
 Vector3 idle;Quaternion idleRotation;float nextShot,reloadEnd,recoil;bool reloading;
 void Start(){idle=pivot.localPosition;idleRotation=pivot.localRotation;}
 public void Cancel(){reloading=false;recoil=0;nextShot=Time.time+.1f;}
 void Update(){
  if(GameMenu.Paused)return;
  bool active=!ui.InventoryOpen && inventory.HasRifle && inventory.SelectedWeapon==WeaponSlot.Rifle && player.vitals.Alive;pivot.gameObject.SetActive(active);
  if(!active || !player.Locked || !player.InputReady){Cancel();return;}
  var k=Keyboard.current;var m=Mouse.current;
  if(k!=null && k.rKey.wasPressedThisFrame && inventory.RifleMagazine<30 && inventory.RifleAmmo>0 && !reloading){reloading=true;reloadEnd=Time.time+reloadSeconds;}
  if(reloading && Time.time>=reloadEnd){inventory.ReloadRifle();reloading=false;}
  if(m!=null && m.leftButton.isPressed && !reloading && Time.time>=nextShot && inventory.RifleMagazine>0){
   Vector3 direction=presentation!=null?presentation.ShotDirection(1.15f,.18f):player.view.transform.forward;
   if(projectiles.Fire(player.view.transform.position+player.view.transform.forward*.1f,direction,bulletSpeed,damage,~((1<<player.gameObject.layer)|(1<<9)))){
    inventory.TrySpendRifleRound();nextShot=Time.time+shotInterval;recoil=1;NoiseBus.Emit(player.transform.position,75);if(audioEffects!=null)audioEffects.Shot();presentation?.Kick(.55f);
   }
  }
  recoil=Mathf.MoveTowards(recoil,0,Time.deltaTime*9);pivot.localPosition=idle+new Vector3(0,recoil*.02f,-recoil*.06f)+Vector3.down*(reloading?.18f:0);pivot.localRotation=idleRotation*Quaternion.Euler(-recoil*7,0,reloading?23:0);
 }
}
}

using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainSummer {
[DefaultExecutionOrder(290)]public sealed class PlayerPistol:MonoBehaviour {
 public FirstPersonMotor player;public PrototypeUI ui;public ResourceInventory inventory;public Transform pivot;public BallisticProjectiles projectiles;public ProceduralAudio audioEffects;
 public float damage=30,bulletSpeed=280,shotInterval=.22f,reloadSeconds=1.4f;
 Vector3 idle;Quaternion idleRotation;float nextShot,reloadEnd,recoil;bool reloading;
 public bool Reloading=>reloading;
 void Start(){idle=pivot.localPosition;idleRotation=pivot.localRotation;}
 public void Cancel(){reloading=false;recoil=0;}
 void Update(){
  if(GameMenu.Paused)return;
  bool active=!ui.InventoryOpen && inventory.HasPistol && inventory.SelectedWeapon==WeaponSlot.Pistol && player.vitals.Alive;
  pivot.gameObject.SetActive(active);
  bool allowed=active && player.Locked && !ui.InventoryOpen;
  if(!allowed){Cancel();return;}
  var k=Keyboard.current;var m=Mouse.current;
  if(k!=null && k.rKey.wasPressedThisFrame && inventory.Magazine<12 && inventory.Ammo>0 && !reloading){reloading=true;reloadEnd=Time.time+reloadSeconds;}
  if(reloading && Time.time>=reloadEnd){inventory.Reload();reloading=false;}
  if(m!=null && m.leftButton.wasPressedThisFrame && !reloading && Time.time>=nextShot && inventory.Magazine>0){
   int mask=~((1<<player.gameObject.layer)|(1<<9));
   if(projectiles.Fire(player.view.transform.position+player.view.transform.forward*.1f,player.view.transform.forward,bulletSpeed,damage,mask)){
    inventory.TrySpendRound();nextShot=Time.time+shotInterval;recoil=1;NoiseBus.Emit(player.transform.position,60);if(audioEffects!=null)audioEffects.Shot();
   }
  }
  recoil=Mathf.MoveTowards(recoil,0,Time.deltaTime*6);
  pivot.localPosition=idle+new Vector3(0,recoil*.025f,-recoil*.06f)+Vector3.down*(reloading?.16f:0);
  pivot.localRotation=idleRotation*Quaternion.Euler(-recoil*12,0,reloading?25:0);
 }
}
}

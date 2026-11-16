using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainSummer {
[DefaultExecutionOrder(295)]public sealed class PlayerShotgun:MonoBehaviour {
 public FirstPersonMotor player;public PrototypeUI ui;public ResourceInventory inventory;public Transform pivot;public BallisticProjectiles projectiles;public ProceduralAudio audioEffects;public WeaponPresentation presentation;
 public float pelletDamage=12,pelletSpeed=230,spreadDegrees=3.2f,shotInterval=.95f,shellLoadSeconds=.65f;
 public bool Reloading=>reloading;
 Vector3 idle;Quaternion idleRotation;float nextShot,nextShell,recoil;bool reloading;
 void Start(){idle=pivot.localPosition;idleRotation=pivot.localRotation;}
 public void Cancel(){reloading=false;recoil=0;nextShot=Time.time+.15f;}
 void Update(){
  if(GameMenu.Paused)return;
  bool active=!ui.InventoryOpen && inventory.HasShotgun && inventory.SelectedWeapon==WeaponSlot.Shotgun && player.vitals.Alive;pivot.gameObject.SetActive(active);
  if(!active || !player.Locked || !player.InputReady){Cancel();return;}
  var k=Keyboard.current;var m=Mouse.current;
  if(k!=null && k.rKey.wasPressedThisFrame && inventory.Tube<5 && inventory.Shells>0 && !reloading){reloading=true;nextShell=Time.time+shellLoadSeconds;}
  if(reloading && Time.time>=nextShell){inventory.ReloadShell();nextShell=Time.time+shellLoadSeconds;if(inventory.Tube>=5 || inventory.Shells<=0)reloading=false;}
  if(m!=null && m.leftButton.wasPressedThisFrame && inventory.Tube>0 && Time.time>=nextShot){
   int mask=~((1<<player.gameObject.layer)|(1<<9));
   if(projectiles.FirePellets(player.view.transform.position+player.view.transform.forward*.1f,player.view.transform.rotation,8,presentation!=null?presentation.SpreadDegrees(spreadDegrees,2.0f):spreadDegrees,pelletSpeed,pelletDamage,mask)){
    inventory.TrySpendShell();reloading=false;nextShot=Time.time+shotInterval;recoil=1;presentation?.Kick(2.2f);NoiseBus.Emit(player.transform.position,85);if(audioEffects!=null)audioEffects.Shotgun();
   }
  }
  recoil=Mathf.MoveTowards(recoil,0,Time.deltaTime*3.5f);
  pivot.localPosition=idle+new Vector3(0,recoil*.035f,-recoil*.12f)+Vector3.down*(reloading?.13f:0);
  pivot.localRotation=idleRotation*Quaternion.Euler(-recoil*14,0,reloading?18:0);
 }
}
}

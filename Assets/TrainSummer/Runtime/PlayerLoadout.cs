using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainSummer {
[DefaultExecutionOrder(250)]public sealed class PlayerLoadout:MonoBehaviour {
 public ResourceInventory inventory;public FirstPersonMotor player;public PrototypeUI ui;
 void Update(){if(!player.Locked || ui.InventoryOpen || !player.vitals.Alive)return;var k=Keyboard.current;if(k==null)return;
  if(k.digit1Key.wasPressedThisFrame)inventory.Equip(WeaponSlot.Axe);
  if(k.digit2Key.wasPressedThisFrame)inventory.Equip(WeaponSlot.Pistol);
 }
}
}

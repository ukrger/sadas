using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainSummer {
public sealed class PlayerFlashlight:MonoBehaviour {
 public Light lamp;public FirstPersonMotor player;public PrototypeUI ui;
 void Update(){var k=Keyboard.current;if(k!=null && player.Locked && !ui.InventoryOpen && player.vitals.Alive && k.fKey.wasPressedThisFrame)lamp.enabled=!lamp.enabled;}
}
}

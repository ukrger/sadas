using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainSummer {
[RequireComponent(typeof(CharacterController))]
[DefaultExecutionOrder(100)]
public sealed class FirstPersonMotor:MonoBehaviour {
 public Camera view; public TrainMotor train; public float walkSpeed=4,sensitivity=0.12f;
 CharacterController controller; Transform support; Vector3 supportPoint; Quaternion supportRotation;
 float pitch,vertical; public bool Locked=>Cursor.lockState==CursorLockMode.Locked;
 void Awake(){controller=GetComponent<CharacterController>();Lock(true);}
 void Lock(bool value){Cursor.lockState=value?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!value;}
 void Update(){
  var k=Keyboard.current;var m=Mouse.current;if(k==null||m==null)return;
  if(k.escapeKey.wasPressedThisFrame)Lock(!Locked);
  if(support!=null){
   Vector3 carry=support.TransformPoint(supportPoint)-transform.position;
   float yaw=(support.rotation*Quaternion.Inverse(supportRotation)).eulerAngles.y;
   controller.Move(carry);transform.Rotate(0,yaw,0);
  }
  if(Locked){
   Vector2 look=m.delta.ReadValue()*sensitivity;transform.Rotate(0,look.x,0);
   pitch=Mathf.Clamp(pitch-look.y,-80,80);view.transform.localRotation=Quaternion.Euler(pitch,0,0);
  }
  Vector2 move=Locked?new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0)):Vector2.zero;
  move=Vector2.ClampMagnitude(move,1);
  if(controller.isGrounded && vertical<0)vertical=-2;
  if(Locked && controller.isGrounded && k.spaceKey.wasPressedThisFrame)vertical=5;
  vertical-=18*Time.deltaTime;
  Vector3 velocity=(transform.right*move.x+transform.forward*move.y)*walkSpeed+Vector3.up*vertical;
  controller.Move(velocity*Time.deltaTime);
  support=null;
  // Short foot probe excludes player's own layer via the scene's collision mask.
  if(vertical<=0 && Physics.Raycast(transform.position+Vector3.up*0.12f,Vector3.down,out var hit,0.35f,~(1<<gameObject.layer),QueryTriggerInteraction.Ignore)){
   var platform=hit.collider.GetComponentInParent<TrainMotor>();if(platform!=null)support=platform.transform;
  }
  train.PlayerAboard=support!=null;
  if(support!=null){supportPoint=support.InverseTransformPoint(transform.position);supportRotation=support.rotation;}
  if(transform.position.y< -15)Respawn();
 }
 void Respawn(){controller.enabled=false;transform.position=train.transform.position+new Vector3(0,1.4f,-2);controller.enabled=true;vertical=0;}
}
}

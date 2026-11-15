using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainSummer {
[RequireComponent(typeof(CharacterController))]
[DefaultExecutionOrder(100)]
public sealed class FirstPersonMotor:MonoBehaviour {
 public Camera view; public TrainMotor train; public float walkSpeed=4,sensitivity=0.12f;
 CharacterController controller; Transform support; Vector3 supportPoint; Quaternion supportRotation;
 Vector3 inheritedVelocity; float pitch,vertical;
 public bool Locked=>Cursor.lockState==CursorLockMode.Locked;
 void Awake(){controller=GetComponent<CharacterController>();Lock(true);}
 void OnDisable(){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
 void Lock(bool value){Cursor.lockState=value?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!value;}
 void Update(){
  var k=Keyboard.current;var m=Mouse.current;if(k==null||m==null)return;
  float dt=Time.deltaTime;if(dt<=0)return;
  if(k.escapeKey.wasPressedThisFrame)Lock(!Locked);
  bool jump=Locked && controller.isGrounded && k.spaceKey.wasPressedThisFrame;
  Vector3 platformVelocity=Vector3.zero;
  bool hadSupport=support!=null;
  if(hadSupport){
   Vector3 carry=support.TransformPoint(supportPoint)-transform.position;
   platformVelocity=carry/dt;
   float yaw=(support.rotation*Quaternion.Inverse(supportRotation)).eulerAngles.y;
   transform.Rotate(0,yaw,0);
   if(jump){inheritedVelocity=Vector3.ProjectOnPlane(platformVelocity,Vector3.up);support=null;}
   else controller.Move(carry);
  }
  if(Locked){
   Vector2 look=m.delta.ReadValue()*sensitivity;transform.Rotate(0,look.x,0);
   pitch=Mathf.Clamp(pitch-look.y,-80,80);view.transform.localRotation=Quaternion.Euler(pitch,0,0);
  }
  Vector2 move=Locked?new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0)):Vector2.zero;
  move=Vector2.ClampMagnitude(move,1);
  if(controller.isGrounded && vertical<0)vertical=-2;
  if(jump)vertical=5;
  vertical-=18*dt;
  Vector3 velocity=(transform.right*move.x+transform.forward*move.y)*walkSpeed+Vector3.up*vertical;
  // Attached players get platform displacement once. In flight, preserve launch velocity.
  if(support==null)velocity+=inheritedVelocity;
  controller.Move(velocity*dt);
  support=null;
  if(vertical<=0 && Physics.Raycast(transform.position+Vector3.up*.12f,Vector3.down,out var hit,.35f,~(1<<gameObject.layer),QueryTriggerInteraction.Ignore)){
   var platform=hit.collider.GetComponentInParent<TrainMotor>();if(platform!=null)support=platform.transform;
  }
  if(support!=null){
   inheritedVelocity=Vector3.zero;supportPoint=support.InverseTransformPoint(transform.position);supportRotation=support.rotation;
  }else if(controller.isGrounded && vertical<=0)inheritedVelocity=Vector3.zero;
  else if(hadSupport && !jump)inheritedVelocity=Vector3.ProjectOnPlane(platformVelocity,Vector3.up);
  Vector3 local=train.transform.InverseTransformPoint(transform.position);
  train.PlayerAboard=support!=null || (Mathf.Abs(local.x)<1.5f && Mathf.Abs(local.z)<6 && local.y>1 && local.y<4);
  if(transform.position.y< -15)Respawn();
 }
 void Respawn(){controller.enabled=false;transform.position=train.transform.position+new Vector3(0,1.4f,-2);controller.enabled=true;vertical=0;inheritedVelocity=Vector3.zero;support=null;}
}
}

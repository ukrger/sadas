using UnityEngine;
namespace TrainSummer {
public sealed class BreakableWood:MonoBehaviour,IDamageable,IInteractable {
 public float startingHealth=60;public float Health {get;private set;}=60;
 public string Prompt(bool english)=>english?"LMB — break boards with the axe":"ЛКМ — разбить доски топором";
 public string Interact(TrainMotor train,ResourceInventory inventory,bool english)=>english?"Take the axe at the train workbench and strike with LMB.":"Возьми топор у верстака в поезде и ударь ЛКМ.";
 public void RestoreHealth(float value){Health=Mathf.Max(0,value);}
 void Awake(){Health=Mathf.Max(1,startingHealth);}
 public void ApplyDamage(float amount){
  if(amount<=0 || Health<=0)return;
  Health=Mathf.Max(0,Health-amount);if(Health<=0)gameObject.SetActive(false);
 }
}
}

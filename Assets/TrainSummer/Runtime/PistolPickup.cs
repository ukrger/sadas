using UnityEngine;
namespace TrainSummer {
public sealed class PistolPickup:MonoBehaviour,IInteractable {
 bool collected;public void RestoreCollected(bool value){collected=value;}
 public string Prompt(bool en)=>en?"E — take pistol":"E — взять пистолет";
 public string Interact(TrainMotor train,ResourceInventory inventory,bool en){
  if(collected || inventory==null || !inventory.TryTakePistol())return en?"Pistol already owned.":"Пистолет уже есть.";
  collected=true;gameObject.SetActive(false);return en?"2 — pistol, R — reload.":"2 — пистолет, R — перезарядка.";
 }
}
}

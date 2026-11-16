using UnityEngine;
namespace TrainSummer {
public sealed class AmmoPickup:MonoBehaviour,IInteractable {
 public int amount=24;bool collected;public void RestoreCollected(bool value){collected=value;}
 public string Prompt(bool en)=>en?"E — take ammo":"E — взять патроны";
 public string Interact(TrainMotor train,ResourceInventory inventory,bool en){
  if(collected || inventory==null || !inventory.TryAddAmmo(amount))return en?"Ammo unavailable.":"Патроны недоступны.";
  collected=true;gameObject.SetActive(false);return en?$"+{amount} cartridges.":$"+{amount} патронов.";
 }
}
}

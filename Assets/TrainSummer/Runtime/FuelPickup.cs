using UnityEngine;
namespace TrainSummer {
public sealed class FuelPickup:MonoBehaviour,IInteractable {
 public float amount=20;
 bool collected;public void RestoreCollected(bool value){collected=value;}
 public string Prompt(bool english)=>english?"E — collect fuel":"E — взять топливо";
 public string Interact(TrainMotor train,ResourceInventory inventory,bool english){
  if(collected || train==null)return english?"Fuel unavailable.":"Топливо недоступно.";
  Collect(train);return english?"Fuel collected.":"Топливо собрано.";
 }
 public void Collect(TrainMotor train){if(collected || train==null)return;collected=true;train.AddFuel(amount);gameObject.SetActive(false);}
}
}

using UnityEngine;
namespace TrainSummer {
public sealed class MedkitPickup:MonoBehaviour,IInteractable {
 bool collected;public void RestoreCollected(bool value){collected=value;}
 public string Prompt(bool english)=>english?"E — take medkit":"E — взять аптечку";
 public string Interact(TrainMotor train,ResourceInventory inventory,bool english){
  if(collected || inventory==null || !inventory.TryAddMedkit())return english?"Cannot take medkit.":"Не удалось взять аптечку.";
  collected=true;gameObject.SetActive(false);return english?"Medkit collected. H — heal.":"Аптечка собрана. H — лечение.";
 }
}
}

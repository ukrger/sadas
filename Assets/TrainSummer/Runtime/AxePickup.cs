using UnityEngine;
namespace TrainSummer {
public sealed class AxePickup:MonoBehaviour,IInteractable {
 bool collected;public void RestoreCollected(bool value){collected=value;}
 public string Prompt(bool english)=>english?"E — take axe":"E — взять топор";
 public string Interact(TrainMotor train,ResourceInventory inventory,bool english){
  if(collected || inventory==null || !inventory.TryTakeAxe())return english?"You already have an axe.":"Топор уже есть.";
  collected=true;gameObject.SetActive(false);return english?"Axe collected. LMB — strike, 1 — equip.":"Топор взят. ЛКМ — удар, 1 — экипировка.";
 }
}
}

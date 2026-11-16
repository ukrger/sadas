using UnityEngine;
namespace TrainSummer {
public sealed class SurvivalPickup:MonoBehaviour,IInteractable {
 public SurvivalItem kind;public int amount=1;bool collected;
 public void RestoreCollected(bool value){collected=value;}
 public string Prompt(bool en)=>en?"E — collect "+Name(true):"E — взять "+Name(false);
 string Name(bool en)=>kind==SurvivalItem.Water?(en?"water":"воду"):(kind==SurvivalItem.Food?(en?"food":"консервы"):(en?"repair kit":"ремонтный набор"));
 public string Interact(TrainMotor train,ResourceInventory inventory,bool en){
  if(collected || inventory==null || !inventory.TryAddSupply(kind,amount))return en?"Cannot take this item.":"Не удалось взять предмет.";
  collected=true;gameObject.SetActive(false);return en?"Collected "+Name(true):"Получено: "+Name(false);
 }
}
}

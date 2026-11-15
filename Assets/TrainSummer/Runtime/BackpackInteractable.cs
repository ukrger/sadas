using UnityEngine;
namespace TrainSummer {
public sealed class BackpackInteractable:MonoBehaviour,IInteractable {
 public string Prompt(bool english)=>english?"E — inspect backpack":"E — осмотреть рюкзак";
 public string Interact(TrainMotor train,ResourceInventory inventory,bool english){
  if(inventory!=null){var ui=inventory.GetComponent<PrototypeUI>();if(ui!=null)ui.OpenInventory();}
  return english?"Backpack contents opened.":"Содержимое рюкзака открыто.";
 }
}
}

using UnityEngine;
namespace TrainSummer {
public sealed class ShellPickup:MonoBehaviour,IInteractable {
 public int amount=12;
 public string Prompt(bool english)=>english?"E — take shotgun shells":"E — взять патроны для дробовика";
 public string Interact(TrainMotor train,ResourceInventory bag,bool english){if(bag==null || !bag.TryAddShells(amount))return english?"No inventory space.":"Нет места в рюкзаке.";gameObject.SetActive(false);return english?"Shotgun shells taken.":"Патроны для дробовика взяты.";}
}
}

using UnityEngine;
namespace TrainSummer {
public sealed class RifleAmmoPickup:MonoBehaviour,IInteractable {
 public int amount=45;
 public string Prompt(bool english)=>english?"E — take rifle ammunition":"E — взять патроны для автомата";
 public string Interact(TrainMotor train,ResourceInventory bag,bool english){if(bag==null || !bag.TryAddRifleAmmo(amount))return english?"No inventory space.":"Нет места в рюкзаке.";gameObject.SetActive(false);return english?"Rifle ammunition taken.":"Патроны для автомата взяты.";}
}
}

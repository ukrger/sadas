using UnityEngine;
namespace TrainSummer {
public sealed class RiflePickup:MonoBehaviour,IInteractable {
 public string Prompt(bool english)=>english?"E — take the rifle":"E — взять автомат";
 public string Interact(TrainMotor train,ResourceInventory bag,bool english){if(bag==null || !bag.TryTakeRifle())return english?"You already have a rifle.":"Автомат уже есть.";gameObject.SetActive(false);return english?"Rifle equipped. 4 — select, hold LMB — fire.":"Автомат взят. 4 — выбрать, удерживай ЛКМ — огонь.";}
}
}

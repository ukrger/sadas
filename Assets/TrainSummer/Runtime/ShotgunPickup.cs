using UnityEngine;
namespace TrainSummer {
public sealed class ShotgunPickup:MonoBehaviour,IInteractable {
 public string Prompt(bool english)=>english?"E — take the shotgun":"E — взять дробовик";
 public string Interact(TrainMotor train,ResourceInventory bag,bool english){if(bag==null || !bag.TryTakeShotgun())return english?"You already have a shotgun.":"Дробовик уже есть.";gameObject.SetActive(false);return english?"Shotgun equipped. 3 — select, R — load shells.":"Дробовик взят. 3 — выбрать, R — зарядить.";}
}
}

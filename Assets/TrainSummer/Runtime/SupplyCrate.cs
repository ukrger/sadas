using UnityEngine;
namespace TrainSummer {
public sealed class SupplyCrate:MonoBehaviour,IInteractable {
 public int metalParts=4,cloth=2;public bool Looted {get;private set;}
 public string Prompt(bool english)=>Looted?(english?"Empty supply crate":"Пустой ящик"):(english?"E — search supply crate":"E — обыскать ящик");
 public string Interact(TrainMotor train,ResourceInventory inventory,bool english){
  if(Looted)return english?"The crate is empty.":"Ящик пуст.";
  if(inventory==null || !inventory.TryAdd(metalParts,cloth))return english?"Cannot carry these supplies.":"Не хватает места для припасов.";
  Looted=true;
  return english?$"Collected: {metalParts} parts, {cloth} cloth.":$"Собрано: детали — {metalParts}, ткань — {cloth}.";
 }
}
}

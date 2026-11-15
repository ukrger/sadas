using UnityEngine;
namespace TrainSummer {
public sealed class RepairBench:MonoBehaviour,IInteractable {
 public TrainMaintenance maintenance;
 public string Prompt(bool english){
  if(maintenance==null)return english?"Workbench unavailable":"Верстак недоступен";
  return english?$"E — repair (+{maintenance.repairAmount:0}%, {maintenance.partsPerRepair} parts + {maintenance.clothPerRepair} cloth)":$"E — ремонт (+{maintenance.repairAmount:0}%, {maintenance.partsPerRepair} детали + {maintenance.clothPerRepair} ткань)";
 }
 public string Interact(TrainMotor train,ResourceInventory inventory,bool english){
  if(maintenance==null || train!=maintenance.train)return english?"Workbench unavailable.":"Верстак недоступен.";
  switch(maintenance.TryRepair(inventory)){
   case RepairResult.Repaired:return english?"Train repaired.":"Поезд отремонтирован.";
   case RepairResult.Moving:return english?"Stop the train before repairing.":"Для ремонта останови поезд.";
   case RepairResult.AlreadyHealthy:return english?"No repair needed.":"Ремонт не требуется.";
   case RepairResult.MissingResources:return english?"Not enough parts or cloth. Search station crates.":"Не хватает деталей или ткани. Обыщи ящики на станциях.";
   default:return english?"Repair unavailable.":"Ремонт недоступен.";
  }
 }
}
}

using UnityEngine;
namespace TrainSummer {
public enum RepairResult {Repaired,Moving,AlreadyHealthy,MissingResources,InvalidConfiguration}
public sealed class TrainMaintenance:MonoBehaviour {
 public TrainMotor train;[Range(0,100)]public float startingCondition=60;
 public float wearPerMeter=.006f,repairAmount=25;
 public int partsPerRepair=3,clothPerRepair=1;
 public float Condition {get;private set;}=60;
 public bool Broken=>Condition<=0;
 void Awake(){Condition=Mathf.Clamp(startingCondition,0,100);}
 public void ApplyWear(float distance){if(distance>0)Condition=Mathf.Max(0,Condition-distance*Mathf.Max(0,wearPerMeter));}
 public RepairResult TryRepair(ResourceInventory inventory){
  if(train==null || inventory==null || repairAmount<=0 || partsPerRepair<0 || clothPerRepair<0 || partsPerRepair+clothPerRepair<=0)return RepairResult.InvalidConfiguration;
  if(train.Speed>.1f)return RepairResult.Moving;
  if(Condition>=100)return RepairResult.AlreadyHealthy;
  if(!inventory.TrySpend(partsPerRepair,clothPerRepair))return RepairResult.MissingResources;
  Condition=Mathf.Min(100,Condition+repairAmount);return RepairResult.Repaired;
 }
}
}

using System.Collections.Generic;
namespace TrainSummer {
public static class SaveMigration {
 public static bool Upgrade(SaveData data,WorldState[] initial,float dwell=180){
  if(data==null || data.world==null || data.inventory==null)return false;
  if(data.schema==3)return true;if((data.schema!=1 && data.schema!=2) || initial==null || data.stopIndex<0 || data.stopIndex>4)return false;
  int oldSchema=data.schema;
  var known=new HashSet<string>();foreach(var state in initial){if(state==null || string.IsNullOrEmpty(state.id) || !known.Add(state.id))return false;}
  var old=new Dictionary<string,WorldState>();foreach(var state in data.world){if(state==null || string.IsNullOrEmpty(state.id) || !known.Contains(state.id) || old.ContainsKey(state.id))return false;old.Add(state.id,state);}
  foreach(var state in initial)if(!old.ContainsKey(state.id)){
   bool supplies=state.id.EndsWith("/Supply WaterBottle") || state.id.EndsWith("/Supply CannedFood") || state.id.EndsWith("/Supply RepairKit");
   bool shotgun=state.id.EndsWith("/Shotgun Pickup") || state.id.EndsWith("/Shell Pickup");
   bool rifle=state.id.EndsWith("/Rifle Pickup") || state.id.EndsWith("/Rifle Ammo Pickup");
   if(!shotgun && !rifle && !(oldSchema==1 && supplies))return false;
  }
  var merged=new WorldState[initial.Length];for(int i=0;i<initial.Length;i++)merged[i]=old.TryGetValue(initial[i].id,out var state)?state:initial[i];
  if(oldSchema==1){data.hunger=100;data.thirst=100;data.inventory.water=0;data.inventory.food=0;data.inventory.repairKits=0;}
  data.inventory.rifle=false;data.inventory.rifleAmmo=0;data.inventory.rifleMagazine=0;data.inventory.shotgun=false;data.inventory.shells=0;data.inventory.tube=0;
  // Departed old stations cannot be revisited by train: credit their mission relays.
  data.expedition=new ExpeditionState{relayMask=(1<<data.stopIndex)-1,timedStop=data.parked && data.stopIndex<4?data.stopIndex:-1,remaining=data.parked && data.stopIndex<4?dwell:0,automatic=true};
  data.world=merged;data.schema=3;return true;
 }
}
}

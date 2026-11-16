using System.Collections.Generic;
namespace TrainSummer {
public static class SaveMigration {
 // Existing entities retain their states; newly added supplies use the scene's initial state.
 public static bool Upgrade(SaveData data,WorldState[] initial){
  if(data==null || data.world==null || data.inventory==null)return false;
  if(data.schema==2)return true;if(data.schema!=1 || initial==null)return false;
  var known=new HashSet<string>();foreach(var state in initial){if(state==null || string.IsNullOrEmpty(state.id) || !known.Add(state.id))return false;}
  var old=new Dictionary<string,WorldState>();foreach(var state in data.world){if(state==null || string.IsNullOrEmpty(state.id) || !known.Contains(state.id) || old.ContainsKey(state.id))return false;old.Add(state.id,state);}
  foreach(var state in initial)if(!old.ContainsKey(state.id) && !state.id.EndsWith("/Supply WaterBottle") && !state.id.EndsWith("/Supply CannedFood") && !state.id.EndsWith("/Supply RepairKit"))return false;
  var merged=new WorldState[initial.Length];for(int i=0;i<initial.Length;i++)merged[i]=old.TryGetValue(initial[i].id,out var state)?state:initial[i];
  data.hunger=100;data.thirst=100;data.inventory.water=0;data.inventory.food=0;data.inventory.repairKits=0;data.world=merged;data.schema=2;return true;
 }
}
}

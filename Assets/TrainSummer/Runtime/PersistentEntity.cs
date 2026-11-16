using UnityEngine;
namespace TrainSummer {
public sealed class PersistentEntity:MonoBehaviour {
 public string id;
 public WorldState Capture(){
  var data=new WorldState{id=id,active=gameObject.activeSelf,position=transform.position,euler=transform.eulerAngles};
  var crate=GetComponent<SupplyCrate>();if(crate!=null)data.looted=crate.Looted;
  var wood=GetComponent<BreakableWood>();if(wood!=null)data.health=wood.Health;
  var enemy=GetComponent<MutantAI>();if(enemy!=null)data.health=enemy.Health;
  return data;
 }
 public void Restore(WorldState data){
  var crate=GetComponent<SupplyCrate>();if(crate!=null)crate.RestoreLooted(data.looted);
  var wood=GetComponent<BreakableWood>();if(wood!=null)wood.RestoreHealth(data.health);
  var enemy=GetComponent<MutantAI>();if(enemy!=null)enemy.RestoreState(data.health,data.position,Quaternion.Euler(data.euler));
  var fuel=GetComponent<FuelPickup>();if(fuel!=null)fuel.RestoreCollected(!data.active);
  var medkit=GetComponent<MedkitPickup>();if(medkit!=null)medkit.RestoreCollected(!data.active);
  var axe=GetComponent<AxePickup>();if(axe!=null)axe.RestoreCollected(!data.active);
  var pistol=GetComponent<PistolPickup>();if(pistol!=null)pistol.RestoreCollected(!data.active);
  var ammo=GetComponent<AmmoPickup>();if(ammo!=null)ammo.RestoreCollected(!data.active);
  var supply=GetComponent<SurvivalPickup>();if(supply!=null)supply.RestoreCollected(!data.active);
  gameObject.SetActive(data.active);
 }
}
}

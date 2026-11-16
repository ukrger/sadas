using UnityEngine;
namespace TrainSummer {
public sealed class ResourceInventory:MonoBehaviour {
 public int stackLimit=999;readonly InventoryModel model=new InventoryModel();
 public int MetalParts=>model.MetalParts;public int Cloth=>model.Cloth;public int Medkits=>model.Medkits;public int Ammo=>model.Ammo;public int Magazine=>model.Magazine;
 public bool HasRifle=>model.HasRifle;public int RifleAmmo=>model.RifleAmmo;public int RifleMagazine=>model.RifleMagazine;
 public bool TryTakeRifle()=>model.TryTakeRifle();public bool TrySpendRifleRound()=>model.TrySpendRifleRound();public int ReloadRifle()=>model.ReloadRifle();
 public bool TryAddRifleAmmo(int amount){Sync();return model.TryAddRifleAmmo(amount);}
 public bool HasShotgun=>model.HasShotgun;public int Shells=>model.Shells;public int Tube=>model.Tube;
 public bool TryTakeShotgun()=>model.TryTakeShotgun();public bool TrySpendShell()=>model.TrySpendShell();public bool ReloadShell()=>model.ReloadShell();
 public bool TryAddShells(int amount){Sync();return model.TryAddShells(amount);}
 public int Water=>model.Water;public int Food=>model.Food;public int RepairKits=>model.RepairKits;
 public bool HasAxe=>model.HasAxe;public bool HasPistol=>model.HasPistol;public WeaponSlot SelectedWeapon=>model.SelectedWeapon;
 void Sync(){model.StackLimit=stackLimit;}
 public bool TryAddSupply(SurvivalItem kind,int amount=1){Sync();return model.TryAddSupply(kind,amount);}
 public bool TryUseSupply(SurvivalItem kind)=>model.TryUseSupply(kind);
 public bool TryCraft(CraftRecipe recipe){Sync();return model.TryCraft(recipe);}
 public bool TryDrink(PlayerNeeds needs){if(model.Water<=0 || needs==null || !needs.Drink())return false;return model.TryUseSupply(SurvivalItem.Water);}
 public bool TryEat(PlayerNeeds needs){if(model.Food<=0 || needs==null || !needs.Eat())return false;return model.TryUseSupply(SurvivalItem.Food);}
 public bool TryAdd(int parts,int cloth){Sync();return model.TryAdd(parts,cloth);}
 public bool TrySpend(int parts,int cloth)=>model.TrySpend(parts,cloth);
 public bool TryAddMedkit(int amount=1){Sync();return model.TryAddMedkit(amount);}
 public bool TryHeal(PlayerVitals vitals,float amount=35){if(model.Medkits<=0 || vitals==null || !vitals.Heal(amount))return false;return model.TrySpendMedkit();}
 public bool TryTakeAxe()=>model.TryTakeAxe();public bool TryTakePistol()=>model.TryTakePistol();
 public bool TryAddAmmo(int amount){Sync();return model.TryAddAmmo(amount);}
 public bool TrySpendRound()=>model.TrySpendRound();public int Reload()=>model.Reload();public void Equip(WeaponSlot slot)=>model.Equip(slot);
 public InventoryState Capture()=>model.Capture();public void Restore(InventoryState data){Sync();model.Restore(data);}
 public bool IsValid(InventoryState data){Sync();return model.IsValid(data);}
}
}

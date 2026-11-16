using System;
namespace TrainSummer {
// Engine-independent transactions. Used by the actual Unity component and CoreChecks.
public sealed class InventoryModel {
 public int StackLimit=999;
 int parts,cloth,medkits,ammo,magazine,water,food,repairKits;bool axe,pistol;WeaponSlot selected;
 public int MetalParts=>parts;public int Cloth=>cloth;public int Medkits=>medkits;public int Ammo=>ammo;public int Magazine=>magazine;
 public int Water=>water;public int Food=>food;public int RepairKits=>repairKits;
 public bool HasAxe=>axe;public bool HasPistol=>pistol;public WeaponSlot SelectedWeapon=>selected;
 public int Count(SurvivalItem kind)=>kind==SurvivalItem.Water?water:(kind==SurvivalItem.Food?food:(kind==SurvivalItem.RepairKit?repairKits:0));
 public bool TryAddSupply(SurvivalItem kind,int amount=1){
  if((int)kind<0 || (int)kind>2 || amount<=0 || amount>StackLimit-Count(kind))return false;
  if(kind==SurvivalItem.Water)water+=amount;else if(kind==SurvivalItem.Food)food+=amount;else repairKits+=amount;return true;
 }
 public bool TryUseSupply(SurvivalItem kind){
  if(Count(kind)<=0)return false;if(kind==SurvivalItem.Water)water--;else if(kind==SurvivalItem.Food)food--;else if(kind==SurvivalItem.RepairKit)repairKits--;else return false;return true;
 }
 public bool TryCraft(CraftRecipe recipe){
  if(recipe==CraftRecipe.RepairKit){if(repairKits>=StackLimit || !TrySpend(3,1))return false;repairKits++;return true;}
  if(recipe==CraftRecipe.Medkit){if(medkits>=StackLimit || !TrySpend(1,2))return false;medkits++;return true;}
  return false;
 }
 public bool TryAdd(int metal,int fabric){if(metal<0 || fabric<0 || metal>StackLimit-parts || fabric>StackLimit-cloth)return false;parts+=metal;cloth+=fabric;return true;}
 public bool TrySpend(int metal,int fabric){if(metal<0 || fabric<0 || parts<metal || cloth<fabric)return false;parts-=metal;cloth-=fabric;return true;}
 public bool TryAddMedkit(int count=1){if(count<=0 || count>StackLimit-medkits)return false;medkits+=count;return true;}
 public bool TrySpendMedkit(){if(medkits<=0)return false;medkits--;return true;}
 public bool TryTakeAxe(){if(axe)return false;axe=true;selected=WeaponSlot.Axe;return true;}
 public bool TryTakePistol(){if(pistol)return false;pistol=true;magazine=12;selected=WeaponSlot.Pistol;return true;}
 public bool TryAddAmmo(int count){if(count<=0 || count>StackLimit-ammo)return false;ammo+=count;return true;}
 public bool TrySpendRound(){if(!pistol || magazine<=0)return false;magazine--;return true;}
 public int Reload(){int count=Math.Min(12-magazine,ammo);if(!pistol || count<=0)return 0;ammo-=count;magazine+=count;return count;}
 public void Equip(WeaponSlot slot){if(slot==WeaponSlot.Axe && !axe || slot==WeaponSlot.Pistol && !pistol || (int)slot<0 || (int)slot>2)return;selected=selected==slot?WeaponSlot.None:slot;}
 public InventoryState Capture()=>new InventoryState{parts=parts,cloth=cloth,medkits=medkits,ammo=ammo,magazine=magazine,axe=axe,pistol=pistol,weapon=(int)selected,water=water,food=food,repairKits=repairKits};
 public bool IsValid(InventoryState d){
  if(d==null)return false;
  foreach(int n in new[]{d.parts,d.cloth,d.medkits,d.ammo,d.water,d.food,d.repairKits})if(n<0 || n>StackLimit)return false;
  return d.magazine>=0 && d.magazine<=12 && (d.pistol || d.magazine==0) && d.weapon>=0 && d.weapon<=2 && (d.weapon!=1 || d.axe) && (d.weapon!=2 || d.pistol);
 }
 public void Restore(InventoryState d){
  if(!IsValid(d))throw new ArgumentException("Invalid inventory snapshot");
  parts=d.parts;cloth=d.cloth;medkits=d.medkits;ammo=d.ammo;magazine=d.magazine;axe=d.axe;pistol=d.pistol;selected=(WeaponSlot)d.weapon;water=d.water;food=d.food;repairKits=d.repairKits;
 }
}
}

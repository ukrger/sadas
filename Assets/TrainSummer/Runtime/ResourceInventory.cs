using UnityEngine;
namespace TrainSummer {
// Runtime state only. The UI representation is independent of these transactions.
public sealed class ResourceInventory:MonoBehaviour {
 public int stackLimit=999;
 [SerializeField]int metalParts,cloth,medkits;
 [SerializeField]bool hasAxe;
 public int Medkits=>medkits;public bool HasAxe=>hasAxe;
 public bool TryTakeAxe(){if(hasAxe)return false;hasAxe=true;return true;}
 public bool TryAddMedkit(int amount=1){if(amount<=0 || amount>stackLimit-medkits)return false;medkits+=amount;return true;}
 public bool TryHeal(PlayerVitals vitals,float amount=35){
  if(medkits<=0 || vitals==null || !vitals.Heal(amount))return false;
  medkits--;return true;
 }
 public int MetalParts=>metalParts;public int Cloth=>cloth;
 public bool TryAdd(int parts,int fabric){
  if(parts<0 || fabric<0 || parts>stackLimit-metalParts || fabric>stackLimit-cloth)return false;
  metalParts+=parts;cloth+=fabric;return true;
 }
 public bool TrySpend(int parts,int fabric){
  if(parts<0 || fabric<0 || metalParts<parts || cloth<fabric)return false;
  metalParts-=parts;cloth-=fabric;return true;
 }
}
}

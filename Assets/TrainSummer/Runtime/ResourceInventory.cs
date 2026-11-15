using UnityEngine;
namespace TrainSummer {
// Runtime state only. The UI representation is independent of these transactions.
public sealed class ResourceInventory:MonoBehaviour {
 public int stackLimit=999;
 [SerializeField]int metalParts,cloth;
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

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
namespace TrainSummer.Tests {
public sealed class PersistenceAndCombatTests {
 [Test]public void RestoringCrateAndInventoryDoesNotDuplicateLoot(){
  var player=new GameObject("Player");var obj=new GameObject("Crate");
  try{
   var inventory=player.AddComponent<ResourceInventory>();var crate=obj.AddComponent<SupplyCrate>();var entity=obj.AddComponent<PersistentEntity>();entity.id="test/crate";
   var before=entity.Capture();var items=inventory.Capture();crate.Interact(null,inventory,false);Assert.AreEqual(4,inventory.MetalParts);
   entity.Restore(before);inventory.Restore(items);Assert.IsFalse(crate.Looted);crate.Interact(null,inventory,false);Assert.AreEqual(4,inventory.MetalParts);
  }finally{Object.DestroyImmediate(obj);Object.DestroyImmediate(player);}
 }
 [Test]public void ReloadThroughUnityComponentConservesAmmo(){
  var obj=new GameObject("Inventory");try{var inventory=obj.AddComponent<ResourceInventory>();inventory.TryTakePistol();inventory.TryAddAmmo(24);inventory.TrySpendRound();int before=inventory.Magazine+inventory.Ammo;Assert.AreEqual(1,inventory.Reload());Assert.AreEqual(before,inventory.Magazine+inventory.Ammo);}finally{Object.DestroyImmediate(obj);}
 }
 [Test]public void LocalizedMenuCatalogsHaveSameKeys(){
  var ru=Resources.Load<TextAsset>("Localization/menu_ru");var en=Resources.Load<TextAsset>("Localization/menu_en");Assert.IsNotNull(ru);Assert.IsNotNull(en);
  var keys=new HashSet<string>();foreach(var entry in JsonUtility.FromJson<LanguageCatalog>(ru.text).entries)Assert.IsTrue(keys.Add(entry.key));
  foreach(var entry in JsonUtility.FromJson<LanguageCatalog>(en.text).entries)Assert.IsTrue(keys.Remove(entry.key));Assert.AreEqual(0,keys.Count);
 }
}
}

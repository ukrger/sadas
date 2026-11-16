using NUnit.Framework;
using UnityEngine;
namespace TrainSummer.Tests {
public sealed class CraftingAndMigrationTests {
 [Test]public void MissingIngredientLeavesResourcesUntouched(){
  var inventory=new InventoryModel();inventory.TryAdd(5,0);Assert.IsFalse(inventory.TryCraft(CraftRecipe.RepairKit));Assert.AreEqual(5,inventory.MetalParts);Assert.AreEqual(0,inventory.RepairKits);
 }
 [Test]public void LegacySaveKeepsLootAndInitializesOnlyNewSupplies(){
  var old=new WorldState{id="Station 1/Crate",active=true,looted=true};
  var initial=new[]{new WorldState{id="Station 1/Crate",active=true,looted=false},new WorldState{id="Station 1/Supply WaterBottle",active=true}};
  var data=new SaveData{schema=1,inventory=new InventoryState{parts=7},world=new[]{old}};
  Assert.IsTrue(SaveMigration.Upgrade(data,initial));Assert.AreEqual(3,data.schema);Assert.AreEqual(100,data.hunger);Assert.AreEqual(100,data.thirst);Assert.AreEqual(7,data.inventory.parts);Assert.IsTrue(data.world[0].looted);Assert.IsTrue(data.world[1].active);
 }
 [Test]public void LegacySaveMissingAnOldEntityIsRejected(){
  var data=new SaveData{schema=1,inventory=new InventoryState(),world=new WorldState[0]};
  Assert.IsFalse(SaveMigration.Upgrade(data,new[]{new WorldState{id="Station 1/Crate",active=true}}));Assert.AreEqual(1,data.schema);
 }
 [Test]public void DuplicateEntityInLegacySaveIsRejected(){
  var entity=new WorldState{id="Station 1/Crate"};var data=new SaveData{schema=1,inventory=new InventoryState(),world=new[]{entity,entity}};Assert.IsFalse(SaveMigration.Upgrade(data,new[]{entity}));
 }
 [Test]public void CurrentSchemaDoesNotResetNeeds(){
  var data=new SaveData{schema=2,hunger=25,thirst=30,inventory=new InventoryState(),world=new WorldState[0]};Assert.IsTrue(SaveMigration.Upgrade(data,new WorldState[0]));Assert.AreEqual(25,data.hunger);Assert.AreEqual(30,data.thirst);
 }
 [Test]public void KitRepairDoesNotSpendRawMaterials(){
  var trainObject=new GameObject("Train");var player=new GameObject("Inventory");
  try{
   var train=trainObject.AddComponent<TrainMotor>();var maintenance=trainObject.AddComponent<TrainMaintenance>();maintenance.train=train;
   var inventory=player.AddComponent<ResourceInventory>();inventory.TryAddSupply(SurvivalItem.RepairKit);float before=maintenance.Condition;
   Assert.AreEqual(RepairResult.Repaired,maintenance.TryUseKit(inventory));Assert.AreEqual(0,inventory.RepairKits);Assert.AreEqual(0,inventory.MetalParts);Assert.That(maintenance.Condition,Is.EqualTo(before+25));
  }finally{Object.DestroyImmediate(player);Object.DestroyImmediate(trainObject);}
 }
}
}

using NUnit.Framework;
using UnityEngine;
namespace TrainSummer.Tests {
public sealed class ResourcesAndRepairTests {
 GameObject trainObject,playerObject,crateObject,routeObject;
 TrainMotor train;TrainMaintenance maintenance;ResourceInventory inventory;SupplyCrate crate;
 [SetUp]public void Setup(){
  trainObject=new GameObject("Train");train=trainObject.AddComponent<TrainMotor>();maintenance=trainObject.AddComponent<TrainMaintenance>();maintenance.train=train;train.maintenance=maintenance;
  playerObject=new GameObject("Inventory");inventory=playerObject.AddComponent<ResourceInventory>();
  crateObject=new GameObject("Crate");crate=crateObject.AddComponent<SupplyCrate>();
  routeObject=new GameObject("Route");train.route=routeObject.AddComponent<RoutePath>();
 }
 [TearDown]public void Cleanup(){Object.DestroyImmediate(crateObject);Object.DestroyImmediate(playerObject);Object.DestroyImmediate(trainObject);Object.DestroyImmediate(routeObject);}
 [Test]public void CrateIsLootedOnlyOnce(){
  crate.Interact(train,inventory,false);crate.Interact(train,inventory,false);
  Assert.IsTrue(crate.Looted);Assert.AreEqual(4,inventory.MetalParts);Assert.AreEqual(2,inventory.Cloth);
 }
 [Test]public void FullInventoryDoesNotConsumeCrate(){
  inventory.stackLimit=3;crate.Interact(train,inventory,false);
  Assert.IsFalse(crate.Looted);Assert.AreEqual(0,inventory.MetalParts);Assert.AreEqual(0,inventory.Cloth);
 }
 [Test]public void RepairSpendsResourcesAndRestoresCondition(){
  inventory.TryAdd(4,2);float before=maintenance.Condition;
  Assert.AreEqual(RepairResult.Repaired,maintenance.TryRepair(inventory));
  Assert.That(maintenance.Condition,Is.EqualTo(before+25).Within(.001f));Assert.AreEqual(1,inventory.MetalParts);Assert.AreEqual(1,inventory.Cloth);
 }
 [Test]public void MissingClothDoesNotSpendParts(){
  inventory.TryAdd(10,0);float before=maintenance.Condition;
  Assert.AreEqual(RepairResult.MissingResources,maintenance.TryRepair(inventory));Assert.AreEqual(10,inventory.MetalParts);Assert.AreEqual(before,maintenance.Condition);
 }
 [Test]public void HealthyTrainDoesNotSpendResources(){
  inventory.TryAdd(50,50);maintenance.TryRepair(inventory);maintenance.TryRepair(inventory);
  int before=inventory.MetalParts;
  Assert.AreEqual(RepairResult.AlreadyHealthy,maintenance.TryRepair(inventory));Assert.AreEqual(before,inventory.MetalParts);Assert.AreEqual(100,maintenance.Condition);
 }
 [Test]public void MovingTrainCannotBeRepaired(){
  inventory.TryAdd(10,10);train.Tick(1);Assert.Greater(train.Speed,.1f);
  Assert.AreEqual(RepairResult.Moving,maintenance.TryRepair(inventory));Assert.AreEqual(10,inventory.MetalParts);
 }
 [Test]public void NegativeTransactionsAreRejected(){
  Assert.IsFalse(inventory.TryAdd(-1,4));Assert.IsFalse(inventory.TrySpend(-1,0));Assert.AreEqual(0,inventory.MetalParts);Assert.AreEqual(0,inventory.Cloth);
 }
 [Test]public void BrokenTrainCannotDepartUntilRepaired(){
  maintenance.ApplyWear(100000);train.PlayerAboard=true;
  Assert.IsTrue(maintenance.Broken);Assert.IsFalse(train.Depart());
  inventory.TryAdd(3,1);Assert.AreEqual(RepairResult.Repaired,maintenance.TryRepair(inventory));Assert.IsTrue(train.Depart());
 }
}
}

using NUnit.Framework;
using UnityEngine;
namespace TrainSummer.Tests {
public sealed class GearAndHealthTests {
 GameObject playerObject,propObject;ResourceInventory inventory;PlayerVitals vitals;
 [SetUp]public void Setup(){playerObject=new GameObject("Player");inventory=playerObject.AddComponent<ResourceInventory>();vitals=playerObject.AddComponent<PlayerVitals>();propObject=new GameObject("Prop");}
 [TearDown]public void Cleanup(){Object.DestroyImmediate(propObject);Object.DestroyImmediate(playerObject);}
 [Test]public void MedkitPickupCannotBeCollectedTwice(){
  var pickup=propObject.AddComponent<MedkitPickup>();pickup.Interact(null,inventory,false);pickup.Interact(null,inventory,false);Assert.AreEqual(1,inventory.Medkits);
 }
 [Test]public void HealingCapsHealthAndConsumesOneKit(){
  inventory.TryAddMedkit(2);Assert.IsTrue(inventory.TryHeal(vitals));Assert.AreEqual(100,vitals.Health);Assert.AreEqual(1,inventory.Medkits);
 }
 [Test]public void HealthyPlayerKeepsMedkit(){
  vitals.Heal(100);inventory.TryAddMedkit();Assert.IsFalse(inventory.TryHeal(vitals));Assert.AreEqual(1,inventory.Medkits);
 }
 [Test]public void DeadPlayerCannotBeHealed(){
  vitals.ApplyDamage(1000);inventory.TryAddMedkit();Assert.IsFalse(inventory.TryHeal(vitals));Assert.AreEqual(1,inventory.Medkits);Assert.IsFalse(vitals.Alive);
 }
 [Test]public void DuplicateAxeIsRejected(){Assert.IsTrue(inventory.TryTakeAxe());Assert.IsFalse(inventory.TryTakeAxe());Assert.IsTrue(inventory.HasAxe);}
 [Test]public void ThreeAxeHitsBreakBoards(){
  var boards=propObject.AddComponent<BreakableWood>();boards.ApplyDamage(25);boards.ApplyDamage(25);Assert.IsTrue(propObject.activeSelf);boards.ApplyDamage(25);Assert.IsFalse(propObject.activeSelf);Assert.AreEqual(0,boards.Health);
 }
 [Test]public void NegativeDamageCannotHeal(){float before=vitals.Health;vitals.ApplyDamage(-100);Assert.AreEqual(before,vitals.Health);}
 [Test]public void InvalidMedkitAmountsAreRejected(){Assert.IsFalse(inventory.TryAddMedkit(-1));Assert.IsFalse(inventory.TryAddMedkit(0));Assert.AreEqual(0,inventory.Medkits);}
 [Test]public void RespawnRestoresAliveState(){vitals.ApplyDamage(1000);vitals.ResetAfterRespawn();Assert.IsTrue(vitals.Alive);Assert.AreEqual(vitals.maxHealth,vitals.Health);}
}
}

using NUnit.Framework;
using UnityEngine;
namespace TrainSummer.Tests {
public sealed class JourneyTests {
 [Test]public void VersionTwoMigrationAddsOnlyNewShotgunEntitiesAndCreditsDepartedRelays(){
  var old=new WorldState{id="Station 1/Crate",looted=true};
  var initial=new[]{new WorldState{id=old.id},new WorldState{id="Train/Repair Workbench/Shotgun Pickup",active=true},new WorldState{id="Station 2/Station House/Shell Pickup",active=true}};
  var data=new SaveData{schema=2,stopIndex=2,parked=true,hunger=23,thirst=34,inventory=new InventoryState(),world=new[]{old}};
  Assert.IsTrue(SaveMigration.Upgrade(data,initial,180));Assert.AreEqual(3,data.schema);Assert.AreEqual(3,data.expedition.relayMask);Assert.AreEqual(2,data.expedition.timedStop);Assert.AreEqual(180,data.expedition.remaining);
  Assert.AreEqual(23,data.hunger);Assert.AreEqual(34,data.thirst);Assert.IsTrue(data.world[0].looted);Assert.IsTrue(data.world[1].active);Assert.IsFalse(data.inventory.shotgun);
 }
 [Test]public void VersionTwoCannotInventUnrelatedMissingEntities(){
  var data=new SaveData{schema=2,inventory=new InventoryState(),world=new WorldState[0]};
  Assert.IsFalse(SaveMigration.Upgrade(data,new[]{new WorldState{id="Unexpected/Crate"}}));Assert.AreEqual(2,data.schema);
 }
 [Test]public void DepartureWaitsForObjectiveEvenAfterTimerExpires(){
  var rules=new ExpeditionModel();rules.Advance(999,true,0,180);Assert.IsFalse(rules.DepartureDue(true,0,true,true,true));rules.Collect(0);Assert.IsTrue(rules.DepartureDue(true,0,true,true,true));
 }
 [Test]public void TrainCannotBypassStationRelayGate(){
  var routeObject=new GameObject("Route");var trainObject=new GameObject("Train");
  try{
   var route=routeObject.AddComponent<RoutePath>();var train=trainObject.AddComponent<TrainMotor>();train.route=route;
   var mission=trainObject.AddComponent<ExpeditionSession>();mission.train=train;train.expedition=mission;
   train.Restore(new SaveData{distance=160,fuel=30,parked=true,aboard=true,stopIndex=0});
   Assert.IsFalse(train.Depart());Assert.AreEqual(0,train.StopIndex);Assert.IsTrue(train.Parked);
   mission.Collect(0);Assert.IsTrue(train.Depart());Assert.AreEqual(1,train.StopIndex);Assert.IsFalse(train.Parked);
  }finally{Object.DestroyImmediate(trainObject);Object.DestroyImmediate(routeObject);}
 }
 [Test]public void GeneratedSceneHasAllJourneyReferences(){
  // Run after generating a scene; do not mutate a user's currently open scene in a test.
  var mission=Object.FindObjectOfType<ExpeditionSession>();if(mission==null)Assert.Ignore("Generate a Train Summer scene first.");
  Assert.AreSame(mission,mission.train.expedition);Assert.NotNull(mission.player);Assert.AreEqual(4,Object.FindObjectsOfType<MissionRelay>(true).Length);
  Assert.NotNull(mission.player.GetComponent<PlayerShotgun>().pivot);Assert.NotNull(Object.FindObjectOfType<GameMenu>().expedition);Assert.NotNull(Object.FindObjectOfType<SessionSave>().expedition);
 }
}
}

using NUnit.Framework;
using UnityEngine;
namespace TrainSummer.Tests {
public sealed class RouteAndStopsTests {
 GameObject routeObject,trainObject;
 RoutePath route;TrainMotor train;
 [SetUp]public void Setup(){
  routeObject=new GameObject("Test Route");route=routeObject.AddComponent<RoutePath>();route.Rebuild();
  trainObject=new GameObject("Test Train");train=trainObject.AddComponent<TrainMotor>();train.route=route;
 }
 [TearDown]public void Cleanup(){Object.DestroyImmediate(trainObject);Object.DestroyImmediate(routeObject);}
 [Test]public void ArcLengthIsAtLeastForwardExtent(){Assert.GreaterOrEqual(route.Length,route.forwardExtent);}
 [Test]public void EqualDistanceStepsProduceEqualTravel(){
  for(float s=0;s<route.Length-1;s+=17){
   float distance=Vector3.Distance(route.Evaluate(s).position,route.Evaluate(s+1).position);
   Assert.That(distance,Is.EqualTo(1).Within(.003f));
  }
 }
 [Test]public void PoseAppliesRouteTransform(){
  var original=route.Evaluate(400);routeObject.transform.SetPositionAndRotation(new Vector3(70,5,-30),Quaternion.Euler(0,55,0));
  var moved=route.Evaluate(400);
  Assert.That(Vector3.Distance(moved.position,routeObject.transform.TransformPoint(original.position)),Is.LessThan(.001f));
  Assert.That(Quaternion.Angle(moved.rotation,routeObject.transform.rotation*original.rotation),Is.LessThan(.01f));
 }
 [Test]public void HeadingMatchesTrackDirection(){
  for(float s=1;s<route.Length-1;s+=19){
   var tangent=route.Evaluate(s+.2f).position-route.Evaluate(s-.2f).position;
   Assert.That(Vector3.Angle(tangent,route.Evaluate(s).rotation*Vector3.forward),Is.LessThan(.1f));
  }
 }
 [TestCase(20)][TestCase(60)][TestCase(144)]
 public void StopsRequireBoardingAndAdvanceInOrder(int fps){
  train.AddFuel(100);
  foreach(float stop in train.stops){
   for(int i=0;i<fps*180 && !train.Parked;i++)train.Tick(1f/fps);
   Assert.IsTrue(train.Parked);Assert.That(train.Distance,Is.EqualTo(stop).Within(.01f));
   train.PlayerAboard=false;Assert.IsFalse(train.Depart());
   train.PlayerAboard=true;Assert.IsTrue(train.Depart());
  }
  for(int i=0;i<fps*180 && !train.Finished;i++)train.Tick(1f/fps);
  Assert.IsTrue(train.Finished);Assert.IsFalse(train.Depart());
 }
 [Test]public void CollectedCanisterCannotGiveFuelTwice(){
  var can=new GameObject("Fuel").AddComponent<FuelPickup>();
  try{float before=train.Fuel;can.Collect(train);can.Collect(train);Assert.That(train.Fuel,Is.EqualTo(before+can.amount));}
  finally{Object.DestroyImmediate(can.gameObject);}
 }
}
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace TrainSummer.Tests {
public sealed class BallisticsTests {
 GameObject poolObject,targetObject,wall;
 [TearDown]public void Cleanup(){Time.timeScale=1;if(poolObject!=null)Object.DestroyImmediate(poolObject);if(targetObject!=null)Object.DestroyImmediate(targetObject);if(wall!=null)Object.DestroyImmediate(wall);}
 [Test]public void IncompletePelletVolleyDoesNotConsumePoolSlots(){
  poolObject=new GameObject("Pool");var pool=poolObject.AddComponent<BallisticProjectiles>();
  for(int shot=0;shot<57;shot++)Assert.IsTrue(pool.Fire(Vector3.zero,Vector3.forward,230,5,~0));
  Assert.IsFalse(pool.FirePellets(Vector3.zero,Quaternion.identity,8,2,230,5,~0));
  for(int slot=0;slot<7;slot++)Assert.IsTrue(pool.Fire(Vector3.zero,Vector3.forward,230,5,~0));
  Assert.IsFalse(pool.Fire(Vector3.zero,Vector3.forward,230,5,~0));
 }
 [UnityTest]public IEnumerator ShotgunVolleyUsesAllPellets(){
  poolObject=new GameObject("Pool");var pool=poolObject.AddComponent<BallisticProjectiles>();targetObject=GameObject.CreatePrimitive(PrimitiveType.Cube);targetObject.transform.position=new Vector3(0,1,2);
  var target=targetObject.AddComponent<BreakableWood>();target.RestoreHealth(200);Physics.SyncTransforms();
  Assert.IsTrue(pool.FirePellets(new Vector3(0,1,0),Quaternion.identity,8,0,230,12,~0));yield return new WaitForSeconds(.1f);Assert.AreEqual(104,target.Health);
 }
 [UnityTest]public IEnumerator WallBlocksBulletDamage(){
  poolObject=new GameObject("Pool");var pool=poolObject.AddComponent<BallisticProjectiles>();
  targetObject=GameObject.CreatePrimitive(PrimitiveType.Cube);targetObject.transform.position=new Vector3(0,1,2);var target=targetObject.AddComponent<BreakableWood>();
  wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=new Vector3(0,1,1);wall.transform.localScale=new Vector3(2,2,.1f);Physics.SyncTransforms();
  Assert.IsTrue(pool.Fire(new Vector3(0,1,0),Vector3.forward,280,25,~0));yield return new WaitForSeconds(.1f);Assert.AreEqual(60,target.Health);
 }
 [UnityTest]public IEnumerator BulletDamagesTargetAfterPauseEnds(){
  poolObject=new GameObject("Pool");var pool=poolObject.AddComponent<BallisticProjectiles>();
  targetObject=GameObject.CreatePrimitive(PrimitiveType.Cube);targetObject.transform.position=new Vector3(0,1,2);var target=targetObject.AddComponent<BreakableWood>();Physics.SyncTransforms();
  Time.timeScale=0;Assert.IsTrue(pool.Fire(new Vector3(0,1,0),Vector3.forward,280,25,~0));yield return new WaitForSecondsRealtime(.1f);Assert.AreEqual(60,target.Health);
  Time.timeScale=1;yield return new WaitForSeconds(.1f);Assert.AreEqual(35,target.Health);
 }
}
}

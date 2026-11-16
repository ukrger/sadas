using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace TrainSummer.Tests {
public sealed class BallisticsTests {
 GameObject poolObject,targetObject,wall;
 [TearDown]public void Cleanup(){Time.timeScale=1;if(poolObject!=null)Object.DestroyImmediate(poolObject);if(targetObject!=null)Object.DestroyImmediate(targetObject);if(wall!=null)Object.DestroyImmediate(wall);}
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

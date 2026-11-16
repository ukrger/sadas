using NUnit.Framework;
using UnityEngine;
namespace TrainSummer.Tests {
public sealed class AimingTests {
 [TestCase(0,.226f,-.035f,0)]
 [TestCase(0,.2075f,.08f,0)]
 [TestCase(0,.2465f,.19f,0)]
 [TestCase(.02f,.18f,.05f,7)]
 public void SightMarkerSolvesToCameraCenter(float x,float y,float z,float tilt){
  var root=new GameObject("Weapon");var marker=new GameObject("Aim reference");
  try{marker.transform.SetParent(root.transform,false);marker.transform.localPosition=new Vector3(x,y,z);marker.transform.localRotation=Quaternion.Euler(tilt,0,0);
   WeaponPresentation.SightPose(marker.transform,.23f,out var position,out var rotation);
   root.transform.SetPositionAndRotation(position,rotation);
   Assert.That(Vector3.Distance(marker.transform.position,new Vector3(0,0,.23f)),Is.LessThan(.00001f));
   Assert.That(Vector3.Angle(marker.transform.forward,Vector3.forward),Is.LessThan(.001f));
  }finally{Object.DestroyImmediate(root);}
 }
 [Test]public void AimingDoesNotAlterGravityOrCreateAmmo(){
  var aim=new AimModel();var bag=new InventoryModel();bag.TryTakeRifle();int total=bag.RifleMagazine+bag.RifleAmmo;
  aim.Advance(.3f,true,true);Assert.Less(aim.Spread(1.15f,.18f),.2f);Assert.AreEqual(total,bag.RifleMagazine+bag.RifleAmmo);
 }
}
}

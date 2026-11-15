using UnityEngine;
namespace TrainSummer {
public sealed class FuelPickup:MonoBehaviour {
 public float amount=20;
 bool collected;
 public void Collect(TrainMotor train){if(collected || train==null)return;collected=true;train.AddFuel(amount);gameObject.SetActive(false);}
}
}

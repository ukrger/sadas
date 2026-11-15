using UnityEngine;
namespace TrainSummer {
public sealed class FuelPickup:MonoBehaviour {
 public float amount=20;
 public void Collect(TrainMotor train){train.AddFuel(amount);gameObject.SetActive(false);}
}
}

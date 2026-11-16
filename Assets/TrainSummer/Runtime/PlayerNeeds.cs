using UnityEngine;
namespace TrainSummer {
public sealed class PlayerNeeds:MonoBehaviour {
 public PlayerVitals vitals;public float hungerRate=.025f,thirstRate=.045f;
 readonly NeedsModel model=new NeedsModel();public float Hunger=>model.Hunger;public float Thirst=>model.Thirst;
 void Update(){if(vitals!=null && vitals.Alive){float damage=model.Advance(Time.deltaTime,hungerRate,thirstRate);if(damage>0)vitals.ApplyDamage(damage);}}
 public bool Drink()=>vitals!=null && vitals.Alive && model.Drink();public bool Eat()=>vitals!=null && vitals.Alive && model.Eat();
 public void Restore(float hunger,float thirst){model.Restore(hunger,thirst);}
}
}

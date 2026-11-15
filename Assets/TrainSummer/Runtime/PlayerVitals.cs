using UnityEngine;
namespace TrainSummer {
public sealed class PlayerVitals:MonoBehaviour,IDamageable {
 public float startingHealth=70,maxHealth=100;
 public float Health {get;private set;}=70;
 public bool Alive=>Health>0;
 void Awake(){Health=Mathf.Clamp(startingHealth,0,maxHealth);}
 public void ApplyDamage(float amount){if(amount>0 && Alive)Health=Mathf.Max(0,Health-amount);}
 public bool Heal(float amount){if(!Alive || amount<=0 || Health>=maxHealth)return false;Health=Mathf.Min(maxHealth,Health+amount);return true;}
 public void ResetAfterRespawn(){Health=Mathf.Max(1,maxHealth);}
}
}

using System;
namespace TrainSummer {
// Independent of Unity: exact empty-meter duration is used even across long frames.
public sealed class NeedsModel {
 double hunger=90,thirst=85;
 public float Hunger=>(float)hunger;public float Thirst=>(float)thirst;
 public void Restore(float food,float water){if(!AtomicFileStore.Range(food,0,100) || !AtomicFileStore.Range(water,0,100))throw new ArgumentException("Invalid needs state");hunger=food;thirst=water;}
 public bool Drink(float amount=45){if(amount<=0 || !AtomicFileStore.Finite(amount) || thirst>=100)return false;thirst=Math.Min(100,thirst+amount);return true;}
 public bool Eat(float amount=35){if(amount<=0 || !AtomicFileStore.Finite(amount) || hunger>=100)return false;hunger=Math.Min(100,hunger+amount);return true;}
 public float Advance(float dt,float hungerRate=.025f,float thirstRate=.045f){
  if(dt<=0 || !AtomicFileStore.Finite(dt) || !AtomicFileStore.Finite(hungerRate) || !AtomicFileStore.Finite(thirstRate) || hungerRate<0 || thirstRate<0)return 0;
  double emptyFood=hunger==0?dt:(hungerRate>0?Math.Max(0,dt-hunger/hungerRate):0);
  double emptyWater=thirst==0?dt:(thirstRate>0?Math.Max(0,dt-thirst/thirstRate):0);
  hunger=Math.Max(0,hunger-(double)hungerRate*dt);thirst=Math.Max(0,thirst-(double)thirstRate*dt);
  return (float)(Math.Max(emptyFood,emptyWater)*1.5);
 }
}
}

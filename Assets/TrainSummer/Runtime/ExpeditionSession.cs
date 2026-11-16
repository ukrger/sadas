using UnityEngine;
namespace TrainSummer {
[DefaultExecutionOrder(180)]public sealed class ExpeditionSession:MonoBehaviour {
 public TrainMotor train;public FirstPersonMotor player;public float dwellSeconds=180;
 readonly ExpeditionModel model=new ExpeditionModel();
 public ExpeditionModel Model=>model;
 public bool Won=>model.Won(train.Finished);
 public bool CanLeave=>!train.Parked || model.CanLeave(train.StopIndex);
 public bool Collect(int index)=>model.Collect(index);
 public ExpeditionState Capture()=>model.Capture();
 public void Restore(ExpeditionState data,int stopIndex,bool parked){model.Restore(data,stopIndex,parked,dwellSeconds);}
 void Update(){
  if(Time.deltaTime<=0 || !player.vitals.Alive || Won)return;
  model.Advance(Time.deltaTime,train.Parked,train.StopIndex,dwellSeconds);
  if(model.DepartureDue(train.Parked,train.StopIndex,train.PlayerAboard,train.Fuel>0,train.maintenance==null || !train.maintenance.Broken))train.Depart();
 }
 public string Objective(bool english){
  if(Won)return english?"Radio link restored. Route complete.":"Радиосвязь восстановлена. Маршрут пройден.";
  if(train.Parked && !model.HasRelay(train.StopIndex))return english?"Recover the relay from this station's radio.":"Забери модуль из рации на этой станции.";
  return english?"Recover 4 radio relays and reach the route's end.":"Собери 4 радиомодуля и доберись до конца маршрута.";
 }
 public string Schedule(bool english){
  if(!train.Parked)return "";
  if(!model.Automatic)return english?"Manual departure: E in the cab.":"Ручное отправление: E в кабине.";
  if(model.Remaining>0)return (english?"Departure in ":"Отправление через ")+(Mathf.CeilToInt(model.Remaining)/60).ToString("00")+":"+(Mathf.CeilToInt(model.Remaining)%60).ToString("00");
  if(!model.HasRelay(train.StopIndex))return english?"Departure held: recover the relay.":"Ожидание: забери радиомодуль.";
  if(!train.PlayerAboard)return english?"Departure held: return to the train.":"Ожидание: вернись в поезд.";
  return english?"Departure held: check fuel and repairs.":"Ожидание: проверь топливо и состояние поезда.";
 }
}
}

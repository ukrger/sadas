using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
namespace TrainSummer {
[DefaultExecutionOrder(400)]public sealed class SessionSave:MonoBehaviour {
 public FirstPersonMotor player;public ResourceInventory inventory;public TrainMotor train;public SummerLighting clock;
 public string LastMessage {get;private set;}="";
 PersistentEntity[] entities;Dictionary<string,PersistentEntity> byId;SaveData initial;float nextAuto;int lastStop;
 string Profile=>AtomicFileStore.Hash(train.route.forwardExtent.ToString("R",CultureInfo.InvariantCulture)+"/"+train.route.amplitude.ToString("R",CultureInfo.InvariantCulture)+"/"+train.route.wavelength.ToString("R",CultureInfo.InvariantCulture)+"/"+string.Join(",",Array.ConvertAll(train.stops,x=>x.ToString("R",CultureInfo.InvariantCulture))));
 string PathFor(bool auto)=>Path.Combine(Application.persistentDataPath,(auto?"autosave_":"save_")+Profile.Substring(0,12)+".json");
 void Start(){
  entities=FindObjectsOfType<PersistentEntity>(true);byId=new Dictionary<string,PersistentEntity>();
  foreach(var e in entities){if(string.IsNullOrEmpty(e.id) || byId.ContainsKey(e.id)){Debug.LogError("Save IDs must be unique: "+e.name);enabled=false;return;}byId.Add(e.id,e);}
  initial=Capture();lastStop=train.StopIndex;nextAuto=Time.time+120;
 }
 void Update(){if(Time.deltaTime<=0 || !player.vitals.Alive)return;
  if(Time.time>=nextAuto || lastStop!=train.StopIndex){Save(true);nextAuto=Time.time+120;lastStop=train.StopIndex;}
 }
 SaveData Capture(){
  var data=new SaveData{routeId=Profile,distance=train.Distance,fuel=train.Fuel,condition=train.maintenance.Condition,speed=train.Speed,stopIndex=train.StopIndex,parked=train.Parked,brake=train.Brake,aboard=train.PlayerAboard,health=player.vitals.Health,timeOfDay=clock.timeOfDay,hunger=player.GetComponent<PlayerNeeds>().Hunger,thirst=player.GetComponent<PlayerNeeds>().Thirst,lookPitch=player.LookPitch,flashlight=player.GetComponent<PlayerFlashlight>().lamp.enabled,inventory=inventory.Capture(),world=new WorldState[entities.Length]};
  data.playerPosition=data.aboard?train.transform.InverseTransformPoint(player.transform.position):player.transform.position;
  data.playerEuler=(data.aboard?Quaternion.Inverse(train.transform.rotation)*player.transform.rotation:player.transform.rotation).eulerAngles;
  for(int i=0;i<entities.Length;i++)data.world[i]=entities[i].Capture();return data;
 }
 public bool Save(bool auto=false){
  if(initial==null)return false;
  try{var data=Capture();if(!Validate(data))throw new InvalidDataException("Invalid runtime state");string payload=JsonUtility.ToJson(data);var envelope=new SaveEnvelope{payload=payload,sha256=AtomicFileStore.Hash(payload)};AtomicFileStore.Write(PathFor(auto),JsonUtility.ToJson(envelope,true));LastMessage=Localizer.English?"Progress saved.":"Прогресс сохранён.";return true;}
  catch(Exception ex){Debug.LogWarning("Save failed: "+ex.Message);LastMessage=Localizer.English?"Could not save progress.":"Не удалось сохранить прогресс.";return false;}
 }
 public bool Load(bool auto=false){
  if(initial==null)return false;string path=PathFor(auto);SaveData data;bool backup=false;
  if(!File.Exists(path) && !File.Exists(path+".bak") && !auto)path=PathFor(true);
  try{data=ReadValidated(path);}
  catch(Exception primary){
   try{data=ReadValidated(path+".bak");backup=true;}
   catch(Exception){Debug.LogWarning("Load rejected before changing game state: "+primary.Message);LastMessage=Localizer.English?"No compatible save or backup.":"Нет совместимого сохранения или резервной копии.";return false;}
  }
  try{ApplyTransactional(data);LastMessage=backup?(Localizer.English?"Backup loaded.":"Загружена резервная копия."):(Localizer.English?"Progress loaded.":"Прогресс загружен.");return true;}
  catch(Exception ex){Debug.LogWarning("Scene restore failed: "+ex.Message);LastMessage=Localizer.English?"Could not restore this scene.":"Не удалось восстановить сцену.";return false;}
 }
 SaveData ReadValidated(string path){
  var envelope=JsonUtility.FromJson<SaveEnvelope>(File.ReadAllText(path));
  if(envelope==null || (envelope.schema!=1 && envelope.schema!=2) || string.IsNullOrEmpty(envelope.payload) || envelope.sha256!=AtomicFileStore.Hash(envelope.payload))throw new InvalidDataException("Save checksum/schema mismatch");
  var data=JsonUtility.FromJson<SaveData>(envelope.payload);if(!SaveMigration.Upgrade(data,initial.world) || !Validate(data))throw new InvalidDataException("Save validation failed");return data;
 }
 bool Finite(Vector3 v)=>AtomicFileStore.Range(v.x,-100000,100000) && AtomicFileStore.Range(v.y,-100000,100000) && AtomicFileStore.Range(v.z,-100000,100000);
 bool Validate(SaveData d){
  if(d==null || d.schema!=2 || d.routeId!=Profile || d.inventory==null || d.world==null || d.world.Length!=entities.Length)return false;
  if(!AtomicFileStore.Range(d.distance,0,train.route.Length) || !AtomicFileStore.Range(d.fuel,0,100000) || !AtomicFileStore.Range(d.condition,0,100) || !AtomicFileStore.Range(d.health,0,player.vitals.maxHealth) || !AtomicFileStore.Range(d.speed,0,train.cruiseSpeed+.01f) || !AtomicFileStore.Range(d.timeOfDay,0,1))return false;
  if(!AtomicFileStore.Range(d.lookPitch,-80,80) || !AtomicFileStore.Range(d.hunger,0,100) || !AtomicFileStore.Range(d.thirst,0,100))return false;
  if(d.stopIndex<0 || d.stopIndex>train.stops.Length || !Finite(d.playerPosition) || !Finite(d.playerEuler))return false;
  float next=d.stopIndex<train.stops.Length?Mathf.Min(train.stops[d.stopIndex],train.route.Length):train.route.Length;
  if(d.stopIndex>0 && d.distance<train.stops[d.stopIndex-1]-.02f)return false;
  if(d.parked && d.speed>.01f)return false;
  if(d.distance>next+.02f || (d.parked && Mathf.Abs(d.distance-next)>.02f))return false;
  if(!inventory.IsValid(d.inventory))return false;
  var inv=d.inventory;foreach(int value in new[]{inv.parts,inv.cloth,inv.medkits,inv.ammo})if(value<0 || value>inventory.stackLimit)return false;
  if(inv.weapon<0 || inv.weapon>2 || (inv.weapon==1 && !inv.axe) || (inv.weapon==2 && !inv.pistol))return false;
  if(inv.magazine<0 || inv.magazine>12 || (!inv.pistol && inv.magazine!=0))return false;
  var seen=new HashSet<string>();foreach(var w in d.world){if(w==null || string.IsNullOrEmpty(w.id) || !byId.ContainsKey(w.id) || !seen.Add(w.id) || !Finite(w.position) || !Finite(w.euler) || !AtomicFileStore.Range(w.health,0,10000))return false;}
  return true;
 }
 public bool NewGame(){if(initial==null)return false;try{ApplyTransactional(initial);return true;}catch(Exception ex){Debug.LogWarning("Reset failed: "+ex.Message);return false;}}
 void ApplyTransactional(SaveData data){
  var previous=Capture();try{Apply(data);}catch{try{Apply(previous);}catch(Exception rollback){Debug.LogError("Restore rollback failed: "+rollback.Message);}throw;}
 }
 void Apply(SaveData data){
  // Every field and entity ID has been checked before mutating the live scene.
  player.GetComponent<PrototypeUI>().CloseInventory();
  train.Restore(data);train.maintenance.RestoreCondition(data.condition);inventory.Restore(data.inventory);player.vitals.RestoreHealth(data.health);clock.timeOfDay=data.timeOfDay;player.GetComponent<PlayerNeeds>().Restore(data.hunger,data.thirst);
  foreach(var world in data.world)byId[world.id].Restore(world);
  Vector3 position=data.aboard?train.transform.TransformPoint(data.playerPosition):data.playerPosition;
  Quaternion rotation=data.aboard?train.transform.rotation*Quaternion.Euler(data.playerEuler):Quaternion.Euler(data.playerEuler);player.RestorePose(position,rotation,data.lookPitch);player.GetComponent<PlayerFlashlight>().lamp.enabled=data.flashlight;
  var melee=player.GetComponent<PlayerMelee>();if(melee!=null)melee.Cancel();var pistol=player.GetComponent<PlayerPistol>();if(pistol!=null)pistol.Cancel();
  foreach(var pool in FindObjectsOfType<BallisticProjectiles>())pool.Clear();Physics.SyncTransforms();lastStop=train.StopIndex;nextAuto=Time.time+120;
 }
}
}

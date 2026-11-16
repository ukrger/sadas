using UnityEngine;
using UnityEngine.Rendering;
namespace TrainSummer.Editor {
public static class JourneySetup {
 public static ExpeditionSession Configure(FirstPersonMotor player,PrototypeUI ui,ResourceInventory inventory,Material metal){
  var mission=new GameObject("Expedition Mission").AddComponent<ExpeditionSession>();mission.train=player.train;mission.player=player;player.train.expedition=mission;ui.expedition=mission;
  var bench=player.train.transform.Find("Repair Workbench");
  var pickup=CombatSetup.Prop("Shotgun Pickup","PumpShotgun",bench,new Vector3(-.4f,1.06f,.04f),new Vector3(0,.08f,.25f),new Vector3(.10f,.24f,1.2f),metal);pickup.transform.localRotation=Quaternion.Euler(0,90,0);pickup.AddComponent<ShotgunPickup>();
  CombatSetup.Prop("Shell Pickup","AmmoBox",bench,new Vector3(.4f,1.02f,.10f),new Vector3(0,.06f,0),new Vector3(.15f,.12f,.15f),metal).AddComponent<ShellPickup>();
  var pivot=new GameObject("Equipped Shotgun").transform;pivot.SetParent(player.view.transform,false);pivot.localPosition=new Vector3(.16f,-.25f,.40f);
  var gun=ModelSetup.Attach("Assets/Models/PumpShotgun.fbx",pivot);NoShadows(gun);Hands(pivot,false);
  var pistol=player.GetComponent<PlayerPistol>();Hands(pistol.pivot,true);
  var shotgun=player.gameObject.AddComponent<PlayerShotgun>();shotgun.player=player;shotgun.ui=ui;shotgun.inventory=inventory;shotgun.pivot=pivot;shotgun.projectiles=pistol.projectiles;shotgun.audioEffects=pistol.audioEffects;pivot.gameObject.SetActive(false);
  var riflePickup=CombatSetup.Prop("Rifle Pickup","AutomaticRifle",player.train.transform,new Vector3(-1.15f,1.5f,-2.7f),new Vector3(0,.05f,.2f),new Vector3(.12f,.5f,1.2f),metal);riflePickup.transform.localRotation=Quaternion.Euler(-90,0,0);riflePickup.AddComponent<RiflePickup>();
  CombatSetup.Prop("Rifle Ammo Pickup","AmmoBox",bench,new Vector3(-.4f,1.02f,-.15f),new Vector3(0,.06f,0),new Vector3(.15f,.12f,.15f),metal).AddComponent<RifleAmmoPickup>();
  var riflePivot=new GameObject("Equipped Rifle").transform;riflePivot.SetParent(player.view.transform,false);riflePivot.localPosition=new Vector3(.17f,-.25f,.40f);
  NoShadows(ModelSetup.Attach("Assets/Models/AutomaticRifle.fbx",riflePivot));Hands(riflePivot,false);
  var rifle=player.gameObject.AddComponent<PlayerRifle>();rifle.player=player;rifle.ui=ui;rifle.inventory=inventory;rifle.pivot=riflePivot;rifle.projectiles=pistol.projectiles;rifle.audioEffects=pistol.audioEffects;riflePivot.gameObject.SetActive(false);
  var presentation=player.gameObject.AddComponent<WeaponPresentation>();presentation.player=player;presentation.ui=ui;presentation.inventory=inventory;presentation.pistol=pistol.pivot;presentation.shotgun=pivot;presentation.rifle=riflePivot;pistol.presentation=presentation;shotgun.presentation=presentation;rifle.presentation=presentation;ui.presentation=presentation;
  presentation.pistolSight=Sight(pistol.pivot,new Vector3(0,.226f,-.035f));presentation.shotgunSight=Sight(pivot,new Vector3(0,.2075f,.08f));presentation.rifleSight=Sight(riflePivot,new Vector3(0,.2465f,.19f));
  foreach(var station in Object.FindObjectsOfType<Transform>()){
   if(!station.name.StartsWith("Station ") || station.name=="Station House")continue;
   var house=station.Find("Station House");if(house==null || !int.TryParse(station.name.Substring(8),out int number))continue;
   var signal=CombatSetup.Prop("Rail Signal","RailSignal",station,new Vector3(-3.8f,0,4),new Vector3(0,1.65f,0),new Vector3(.7f,3.3f,.7f),metal);var signalView=signal.AddComponent<RailSignalView>();signalView.mission=mission;signalView.stationIndex=number-1;
   CombatSetup.Prop("Rifle Ammo Pickup","AmmoBox",house,new Vector3(-1.6f,.29f,1.7f),new Vector3(0,.06f,0),new Vector3(.15f,.12f,.15f),metal).AddComponent<RifleAmmoPickup>();
   var radio=CombatSetup.Prop("Mission Radio","FieldRadio",house,new Vector3(1.1f,1.12f,.75f),new Vector3(0,.38f,0),new Vector3(.48f,.82f,.23f),metal);
   var relay=radio.AddComponent<MissionRelay>();relay.mission=mission;relay.stationIndex=number-1;
   CombatSetup.Prop("Shell Pickup","AmmoBox",house,new Vector3(-2.0f,.29f,1.7f),new Vector3(0,.06f,0),new Vector3(.15f,.12f,.15f),metal).AddComponent<ShellPickup>();
  }
  return mission;
 }
 static Transform Sight(Transform parent,Vector3 position){var sight=new GameObject("Aim Reference").transform;sight.SetParent(parent,false);sight.localPosition=position;sight.localRotation=Quaternion.identity;return sight;}
 static void NoShadows(GameObject obj){if(obj==null)return;foreach(var r in obj.GetComponentsInChildren<Renderer>(true))r.shadowCastingMode=ShadowCastingMode.Off;}
 static void Hands(Transform pivot,bool rightOnly){
  var visual=ModelSetup.Attach("Assets/Models/PlayerHands.fbx",pivot);if(visual==null)return;NoShadows(visual);
  if(rightOnly){visual.transform.localPosition+=new Vector3(0,.07f,-.08f);
   var group=visual.GetComponent<LODGroup>();var lods=group.GetLODs();
   for(int i=0;i<lods.Length;i++){
    var keep=new System.Collections.Generic.List<Renderer>();foreach(var r in lods[i].renderers)if(!r.name.Contains("HandLeft"))keep.Add(r);
    lods[i].renderers=keep.ToArray();
   }
   group.SetLODs(lods);
   foreach(var r in visual.GetComponentsInChildren<Renderer>(true))if(r.name.Contains("HandLeft"))r.gameObject.SetActive(false);
  }
  // Foreground hands remain at their highest detail; they follow the weapon's recoil/reload pose.
  foreach(var group in visual.GetComponentsInChildren<LODGroup>())group.ForceLOD(0);
 }
}
}

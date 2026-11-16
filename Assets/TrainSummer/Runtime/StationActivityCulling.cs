using System;
using UnityEngine;
namespace TrainSummer {
[Serializable]public sealed class StationActivity {public GameObject root;public float distance;}
// Keeps authored stations in memory, but suspends their renderers, colliders, AI and NavMesh.
// This is activation culling, not additive scene streaming or asset unloading.
[DefaultExecutionOrder(-150)]public sealed class StationActivityCulling:MonoBehaviour {
 public TrainMotor train;public FirstPersonMotor player;public StationActivity[] stations;
 public float activateRadius=420,deactivateRadius=500;
 float nextCheck;bool ready;
 void Start(){ready=true;Refresh();}
 void Update(){if(ready && Time.unscaledTime>=nextCheck){nextCheck=Time.unscaledTime+.25f;Refresh();}}
 public void Refresh(){
  if(!ready || train==null || player==null || stations==null)return;
  float playerDistance=train.route.ProjectDistance(player.transform.position);
  foreach(var station in stations){
   if(station==null || station.root==null)continue;
   float distance=Mathf.Min(Mathf.Abs(station.distance-train.Distance),Mathf.Abs(station.distance-playerDistance));
   float radius=station.root.activeSelf?Mathf.Max(activateRadius,deactivateRadius):activateRadius;
   bool active=distance<=radius;if(station.root.activeSelf!=active)station.root.SetActive(active);
  }
 }
}
}

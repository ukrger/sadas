using UnityEngine;
namespace TrainSummer {
public sealed class RailSignalView:MonoBehaviour {
 public ExpeditionSession mission;public int stationIndex;
 Renderer[] lenses;MaterialPropertyBlock block;bool previous,initialized;
 void Awake(){lenses=GetComponentsInChildren<Renderer>(true);block=new MaterialPropertyBlock();}
 void Update(){
  bool green=mission.Model.HasRelay(stationIndex);if(initialized && green==previous)return;previous=green;initialized=true;
  foreach(var lens in lenses){bool red=lens.name.Contains("SignalRed"),go=lens.name.Contains("SignalGreen");if(!red && !go)continue;
   bool on=go?green:!green;Color color=go?new Color(.025f,.45f,.11f):new Color(.48f,.025f,.015f);
   block.Clear();block.SetColor("_BaseColor",color*(on?1:.15f));block.SetColor("_EmissionColor",on?color*3:Color.black);lens.SetPropertyBlock(block);
  }
 }
}
}

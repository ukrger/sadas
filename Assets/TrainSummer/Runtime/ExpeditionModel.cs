using System;
namespace TrainSummer {
[Serializable]public sealed class ExpeditionState {
 public int relayMask,timedStop=-1;public float remaining;public bool automatic=true;
}
// Pure mission/schedule rules shared by Unity and the standalone checks.
public sealed class ExpeditionModel {
 public const int StationCount=4;
 ExpeditionState state=new ExpeditionState();
 public int RelayMask=>state.relayMask;public int TimedStop=>state.timedStop;
 public float Remaining=>state.remaining;public bool Automatic {get=>state.automatic;set=>state.automatic=value;}
 public int Recovered {get{int n=0;for(int i=0;i<StationCount;i++)if(HasRelay(i))n++;return n;}}
 public bool HasRelay(int station)=>station>=0 && station<StationCount && (state.relayMask&(1<<station))!=0;
 public bool Collect(int station){if(station<0 || station>=StationCount || HasRelay(station))return false;state.relayMask|=1<<station;return true;}
 public bool CanLeave(int station)=>HasRelay(station);
 public bool Won(bool finished)=>finished && state.relayMask==15;
 public void Advance(float dt,bool parked,int station,float dwell){
  if(!parked || station<0 || station>=StationCount || !Finite(dt) || dt<0 || !Finite(dwell) || dwell<0)return;
  if(state.timedStop!=station){state.timedStop=station;state.remaining=dwell;}
  state.remaining=Math.Max(0,state.remaining-dt);
 }
 public bool DepartureDue(bool parked,int station,bool aboard,bool fueled,bool serviceable)=>state.automatic && parked && state.timedStop==station && state.remaining<=0 && CanLeave(station) && aboard && fueled && serviceable;
 public ExpeditionState Capture()=>new ExpeditionState{relayMask=state.relayMask,timedStop=state.timedStop,remaining=state.remaining,automatic=state.automatic};
 public static bool Valid(ExpeditionState d,int stopIndex,bool parked,float maxDwell){
  if(d==null || d.relayMask<0 || d.relayMask>15 || d.timedStop< -1 || d.timedStop>=StationCount || d.timedStop>stopIndex || !Finite(d.remaining) || d.remaining<0 || d.remaining>maxDwell)return false;
  if(d.timedStop==-1 && d.remaining!=0)return false;
  // A parked frame can precede its first schedule tick; the previous stop remains valid.
  return stopIndex>=0 && stopIndex<=StationCount && (d.relayMask&((1<<stopIndex)-1))==((1<<stopIndex)-1);
 }
 public void Restore(ExpeditionState value,int stopIndex,bool parked,float maxDwell){
  if(!Valid(value,stopIndex,parked,maxDwell))throw new ArgumentException("Invalid expedition state");
  state=new ExpeditionState{relayMask=value.relayMask,timedStop=value.timedStop,remaining=value.remaining,automatic=value.automatic};
 }
 static bool Finite(float v)=>!float.IsNaN(v) && !float.IsInfinity(v);
}
}

using System;
using System.Collections.Generic;
namespace TrainSummer {
// Pure deterministic layout rules, shared by the runtime and standalone checks.
public static class EnvironmentLayout {
 public const int TileSize=80,TreesPerTile=28,GrassPerTile=192,PoolCapacity=10;
 public struct Placement {public float distance,offset,yaw,scale;public int species;public bool active;}
 static uint Mix(uint value){unchecked{value^=value>>16;value*=0x7feb352d;value^=value>>15;value*=0x846ca68b;return value^(value>>16);}}
 static float Unit(int tile,int slot,int salt){unchecked{return (Mix((uint)tile*73856093u^(uint)slot*19349663u^(uint)salt)&0xffffff)/16777216f;}}
 static bool NearStation(float distance,float[] stops,float range){if(stops!=null)foreach(float stop in stops)if(Math.Abs(distance-stop)<range)return true;return false;}
 public static Placement Ruin(int tile,float[] stops){
  float distance=tile*TileSize+40,offset=Unit(tile,0,51)>.5f?36:-36;return new Placement{distance=distance,offset=offset,yaw=offset>0?90:-90,scale=1,active=tile>=0 && tile%3==1 && !NearStation(distance,stops,65)};
 }
 public static Placement Tree(int tile,int slot,float[] stops){
  float distance=tile*TileSize+Unit(tile,slot,1)*TileSize;float sign=slot%2==0?-1:1;float offset=sign*(14+Unit(tile,slot,2)*52);
  if(sign>0 && offset<27 && NearStation(distance,stops,32))offset=32+Unit(tile,slot,3)*30;
  var ruin=Ruin(tile,stops);if(ruin.active && Math.Abs(distance-ruin.distance)<8 && Math.Abs(offset-ruin.offset)<8)offset=sign*(Math.Abs(ruin.offset)+11+Unit(tile,slot,7)*10);
  return new Placement{distance=distance,offset=offset,yaw=Unit(tile,slot,4)*360,scale=.82f+Unit(tile,slot,5)*.36f,species=Unit(tile,slot,6)>.48f?1:0,active=true};
 }
 public static Placement Grass(int tile,int slot,float[] stops){
  float distance=tile*TileSize+Unit(tile,slot,10)*TileSize;float sign=slot%2==0?-1:1;float offset=sign*(2.8f+Unit(tile,slot,11)*10);
  if(sign>0 && NearStation(distance,stops,28))offset+=22;
  return new Placement{distance=distance,offset=offset,yaw=Unit(tile,slot,12)*360,scale=.8f+Unit(tile,slot,13)*.8f,species=2,active=true};
 }
 public static void Window(int trainCenter,int playerCenter,List<int> result){
  result.Clear();for(int i=trainCenter-2;i<=trainCenter+4;i++)result.Add(i);
  for(int i=playerCenter-1;i<=playerCenter+1;i++)if(!result.Contains(i))result.Add(i);
  result.Sort();
 }
}
}

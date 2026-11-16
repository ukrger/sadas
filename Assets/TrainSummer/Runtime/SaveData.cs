using System;
using UnityEngine;
namespace TrainSummer {
[Serializable]public sealed class SaveEnvelope {public int schema=2;public string payload,sha256;}
[Serializable]public sealed class WorldState {public string id;public bool active,looted;public float health;public Vector3 position,euler;}
[Serializable]public sealed class SaveData {
 public int schema=2;public string routeId;
 public float distance,fuel,condition,speed,health,timeOfDay,lookPitch,hunger,thirst;public int stopIndex;public bool parked,brake,aboard,flashlight;
 public Vector3 playerPosition,playerEuler;public InventoryState inventory;public WorldState[] world;
}
}

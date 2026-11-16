using System;
using UnityEngine;
namespace TrainSummer {
public static class NoiseBus {
 public static event Action<Vector3,float> Heard;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset(){Heard=null;}
 public static void Emit(Vector3 position,float radius){Heard?.Invoke(position,radius);}
}
}

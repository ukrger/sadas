using UnityEngine;
using UnityEngine.AI;
namespace TrainSummer {
public sealed class StationNavigation:MonoBehaviour {
 public NavMeshData data;NavMeshDataInstance instance;
 void OnEnable(){if(data!=null)instance=NavMesh.AddNavMeshData(data);}
 void OnDisable(){if(instance.valid)instance.Remove();}
}
}

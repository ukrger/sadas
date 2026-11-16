using System;
using UnityEngine;
namespace TrainSummer {
[Serializable]public sealed class VegetationPart {public Mesh mesh;public int submesh;public Material material;public Matrix4x4 localToTree;}
[Serializable]public sealed class VegetationLod {public float distance;public VegetationPart[] parts;}
public sealed class VegetationDefinition:ScriptableObject {public Bounds bounds;public float maxDistance=300;public VegetationLod[] lods;}
public struct VegetationInstance {public int species;public Matrix4x4 matrix;public Bounds bounds;public Vector3 position;public float scale;}
public sealed class VegetationTile {public bool active;public VegetationInstance[] instances;}
}

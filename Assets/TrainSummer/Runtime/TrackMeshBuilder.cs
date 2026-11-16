using UnityEngine;
namespace TrainSummer {
// Reused buffers; rail/sleeper boxes are baked into two render meshes and one collider mesh per tile.
public sealed class TrackMeshBuilder {
 readonly Vector3[] vertices;readonly Vector3[] normals;readonly Vector2[] uv;readonly int[] indices;int boxes;
 static readonly Vector3[] corners={new Vector3(-1,-1,-1),new Vector3(1,-1,-1),new Vector3(1,1,-1),new Vector3(-1,1,-1),new Vector3(-1,-1,1),new Vector3(1,-1,1),new Vector3(1,1,1),new Vector3(-1,1,1)};
 static readonly int[] faces={0,3,2,1,5,6,7,4,4,7,3,0,1,2,6,5,3,7,6,2,4,0,1,5};
 public TrackMeshBuilder(int capacity){vertices=new Vector3[capacity*24];normals=new Vector3[vertices.Length];uv=new Vector2[vertices.Length];indices=new int[capacity*36];
  for(int box=0;box<capacity;box++)for(int face=0;face<6;face++){
   int v=box*24+face*4,t=box*36+face*6;indices[t]=v;indices[t+1]=v+1;indices[t+2]=v+2;indices[t+3]=v;indices[t+4]=v+2;indices[t+5]=v+3;
   uv[v]=Vector2.zero;uv[v+1]=Vector2.up;uv[v+2]=Vector2.one;uv[v+3]=Vector2.right;
  }
 }
 public void Clear(){boxes=0;}
 public void Box(Vector3 center,Quaternion rotation,Vector3 size){
  int offset=boxes++*24;
  for(int face=0;face<6;face++){
   int first=faces[face*4],second=faces[face*4+1],third=faces[face*4+2];Vector3 normal=rotation*Vector3.Cross(corners[second]-corners[first],corners[third]-corners[first]).normalized;
   for(int corner=0;corner<4;corner++){int index=offset+face*4+corner;vertices[index]=center+rotation*Vector3.Scale(corners[faces[face*4+corner]],size*.5f);normals[index]=normal;}
  }
 }
 public void Apply(Mesh mesh){if(boxes*24!=vertices.Length)throw new System.InvalidOperationException("Track mesh capacity mismatch");mesh.Clear();mesh.vertices=vertices;mesh.normals=normals;mesh.uv=uv;mesh.triangles=indices;mesh.RecalculateBounds();}
}
}

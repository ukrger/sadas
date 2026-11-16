using NUnit.Framework;
using UnityEngine;
using UnityEditor;
namespace TrainSummer.Tests {
public sealed class EnvironmentTests {
 [Test]public void RouteProjectionRoundTripsWorldPositions(){
  var obj=new GameObject("Route");try{var route=obj.AddComponent<RoutePath>();route.Rebuild();obj.transform.SetPositionAndRotation(new Vector3(25,0,-40),Quaternion.Euler(0,35,0));
   for(float s=-150;s<route.Length+150;s+=37)Assert.That(route.ProjectDistance(route.Evaluate(s).position),Is.EqualTo(s).Within(.01f));
  }finally{Object.DestroyImmediate(obj);}
 }
 [Test]public void TrackBoxHasOutwardNormalsAndCorrectBounds(){
  var mesh=new Mesh();try{var builder=new TrackMeshBuilder(1);builder.Box(Vector3.zero,Quaternion.identity,Vector3.one*2);builder.Apply(mesh);
   Assert.AreEqual(24,mesh.vertexCount);Assert.AreEqual(36,mesh.triangles.Length);Assert.That(Vector3.Distance(mesh.bounds.size,Vector3.one*2),Is.LessThan(.0001f));
   var vertices=mesh.vertices;var normals=mesh.normals;for(int i=0;i<vertices.Length;i++)Assert.Greater(Vector3.Dot(vertices[i],normals[i]),0);
   builder.Clear();builder.Box(Vector3.right*5,Quaternion.identity,Vector3.one*2);builder.Apply(mesh);Assert.That(mesh.bounds.center.x,Is.EqualTo(5).Within(.0001f));
  }finally{Object.DestroyImmediate(mesh);}
 }
 [Test]public void BoundsTransformCoversRotationAndScale(){
  var bounds=new Bounds(new Vector3(1,2,3),new Vector3(2,4,6));var matrix=Matrix4x4.TRS(new Vector3(10,2,-3),Quaternion.Euler(0,90,0),Vector3.one*2);
  var result=VegetationRenderer.TransformBounds(bounds,matrix);Assert.That(Vector3.Distance(result.center,new Vector3(16,6,-5)),Is.LessThan(.001f));Assert.That(Vector3.Distance(result.size,new Vector3(12,8,4)),Is.LessThan(.001f));
 }
 [Test]public void FoliageShaderImportsWithoutErrors(){var shader=Shader.Find("TrainSummer/FoliageWind");Assert.IsNotNull(shader);Assert.IsFalse(ShaderUtil.ShaderHasError(shader));}
}
}

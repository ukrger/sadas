using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace TrainSummer.Editor {
public static class SurfaceBaker {
 const string Folder="Assets/TrainSummer/Textures/Procedural";
 static readonly Dictionary<SurfaceStyle,Texture2D[]> cache=new Dictionary<SurfaceStyle,Texture2D[]>();
 public static void Apply(Material material,string key){
  if(material==null || material.shader.name!="Universal Render Pipeline/Lit")return;
  SurfaceStyle style;
  if(key.Contains("Wood") || key=="HouseTrim")style=SurfaceStyle.Wood;
  else if(key.Contains("Steel") || key.Contains("Roof"))style=SurfaceStyle.Steel;
  else if(key.Contains("Paint") || key=="SupplyKit")style=SurfaceStyle.Paint;
  else if(key.Contains("Canvas") || key.Contains("Cloth") || key.Contains("Sleeve"))style=SurfaceStyle.Fabric;
  else if(key.Contains("Leather") || key.Contains("Glove") || key.Contains("Grip") || key=="TrainSeat")style=SurfaceStyle.Leather;
  else if(key=="EnvPlaster" || key=="HouseStone" || key=="Concrete")style=SurfaceStyle.Plaster;
  else if(key=="EnvBrick")style=SurfaceStyle.Brick;
  else return;
  var maps=Load(style);material.SetTexture("_BaseMap",maps[0]);material.SetTexture("_BumpMap",maps[1]);material.SetTexture("_MetallicGlossMap",maps[2]);material.SetFloat("_BumpScale",.5f);material.SetFloat("_Smoothness",1);
  material.EnableKeyword("_NORMALMAP");material.EnableKeyword("_METALLICSPECGLOSSMAP");EditorUtility.SetDirty(material);
 }
 static Texture2D[] Load(SurfaceStyle style){
  if(cache.TryGetValue(style,out var existing) && existing[0]!=null)return existing;
  Directory.CreateDirectory(Folder);var maps=new Texture2D[3];string[] roles={"Albedo","Normal","MetalSmooth"};
  for(int role=0;role<3;role++){
   string path=Folder+"/"+style+"_"+roles[role]+".png";
   if(!File.Exists(path))Bake(style,role,path);
   AssetDatabase.ImportAsset(path);var importer=AssetImporter.GetAtPath(path) as TextureImporter;
   importer.textureType=role==1?TextureImporterType.NormalMap:TextureImporterType.Default;importer.sRGBTexture=role==0;importer.mipmapEnabled=true;importer.isReadable=false;importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=4;importer.maxTextureSize=256;importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();
   maps[role]=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
  }
  cache[style]=maps;return maps;
 }
 static void Bake(SurfaceStyle style,int role,string path){
  const int size=256;var pixels=new Color[size*size];
  for(int y=0;y<size;y++)for(int x=0;x<size;x++){
   float u=x/(float)size,v=y/(float)size;var sample=SurfacePattern.Sample(style,u,v);Color color;
   if(role==0)color=new Color(sample.r,sample.g,sample.b,1);
   else if(role==1){SurfacePattern.Normal(style,u,v,out float nx,out float ny,out float nz);color=new Color(nx*.5f+.5f,ny*.5f+.5f,nz*.5f+.5f,1);}
   else color=new Color(sample.metallic,0,0,sample.smoothness);
   pixels[y*size+x]=color;
  }
  var texture=new Texture2D(size,size,TextureFormat.RGBA32,false,true);texture.SetPixels(pixels);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);
 }
}
}

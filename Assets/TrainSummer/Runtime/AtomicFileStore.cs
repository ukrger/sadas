using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace TrainSummer {
// No Unity dependency: validated separately with the standalone .NET check project.
public static class AtomicFileStore {
 public static string Hash(string content){using(var sha=SHA256.Create()){return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(content))).Replace("-","").ToLowerInvariant();}}
 public static void Write(string path,string content){
  string directory=Path.GetDirectoryName(Path.GetFullPath(path));Directory.CreateDirectory(directory);
  string temp=path+".tmp";
  try{
   byte[] data=new UTF8Encoding(false).GetBytes(content);
   using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None)){stream.Write(data,0,data.Length);stream.Flush(true);}
   if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);
  }finally{if(File.Exists(temp))File.Delete(temp);}
 }
 public static bool Finite(float value)=>!float.IsNaN(value) && !float.IsInfinity(value);
 public static bool Range(float value,float min,float max)=>Finite(value) && value>=min && value<=max;
}
}

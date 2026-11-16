using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using TrainSummer;
// Offline PNG export of the same production SurfacePattern used by the Unity editor baker.
static class PatternExport {
 public static void Export(string folder){
  Directory.CreateDirectory(folder);const int size=256;string[] names={"Albedo","Normal","MetalSmooth"};
  foreach(SurfaceStyle style in Enum.GetValues(typeof(SurfaceStyle)))for(int role=0;role<3;role++){
   var data=new byte[size*(size*4+1)];
   for(int row=0;row<size;row++)for(int x=0;x<size;x++){
    int y=size-1-row,index=row*(size*4+1)+1+x*4;float u=x/(float)size,v=y/(float)size;var s=SurfacePattern.Sample(style,u,v);float r,g,b,a=1;
    if(role==0){r=s.r;g=s.g;b=s.b;}
    else if(role==1){SurfacePattern.Normal(style,u,v,out float nx,out float ny,out float nz);r=nx*.5f+.5f;g=ny*.5f+.5f;b=nz*.5f+.5f;}
    else{r=s.metallic;g=b=0;a=s.smoothness;}
    data[index]=Byte(r);data[index+1]=Byte(g);data[index+2]=Byte(b);data[index+3]=Byte(a);
   }
   using var file=File.Create(Path.Combine(folder,style+"_"+names[role]+".png"));file.Write(new byte[]{137,80,78,71,13,10,26,10});var header=new byte[13];Big(header,0,size);Big(header,4,size);header[8]=8;header[9]=6;Chunk(file,"IHDR",header);
   using var payload=new MemoryStream();using(var zip=new ZLibStream(payload,CompressionLevel.Optimal,true))zip.Write(data);Chunk(file,"IDAT",payload.ToArray());Chunk(file,"IEND",Array.Empty<byte>());
   Console.WriteLine("BAKED "+style+" "+names[role]);
  }
 }
 static byte Byte(float x)=>(byte)Math.Clamp((int)Math.Round(x*255),0,255);
 static void Big(byte[] b,int start,int value){b[start]=(byte)(value>>24);b[start+1]=(byte)(value>>16);b[start+2]=(byte)(value>>8);b[start+3]=(byte)value;}
 static void Chunk(Stream output,string type,byte[] content){
  byte[] count=new byte[4];Big(count,0,content.Length);output.Write(count);byte[] name=Encoding.ASCII.GetBytes(type);output.Write(name);output.Write(content);
  uint crc=0xffffffff;foreach(var b in name)crc=Update(crc,b);foreach(var b in content)crc=Update(crc,b);Big(count,0,unchecked((int)(crc^0xffffffff)));output.Write(count);
 }
 static uint Update(uint crc,byte value){crc^=value;for(int i=0;i<8;i++)crc=(crc>>1)^((crc&1)!=0?0xedb88320u:0);return crc;}
}

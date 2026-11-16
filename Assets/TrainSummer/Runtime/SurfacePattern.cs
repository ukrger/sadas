using System;
namespace TrainSummer {
public enum SurfaceStyle {Steel,Wood,Paint,Fabric,Leather,Plaster,Brick}
public struct SurfaceSample {public float r,g,b,height,metallic,smoothness;}
// Deterministic tileable original material patterns. No Unity dependencies or external textures.
public static class SurfacePattern {
 static float Frac(float x)=>x-(float)Math.Floor(x);
 static float Clamp(float v,float min=0,float max=1)=>Math.Max(min,Math.Min(max,v));
 static float Mix(float a,float b,float t)=>a+(b-a)*t;
 static float Hash(int x,int y){unchecked{uint n=(uint)x*374761393u+(uint)y*668265263u+9137u;n=(n^(n>>13))*1274126177u;return (n^(n>>16))*(1f/uint.MaxValue);}}
 static float Noise(float u,float v,int grid){
  float x=Frac(u)*grid,y=Frac(v)*grid;int ix=(int)x,iy=(int)y;float fx=x-ix,fy=y-iy;fx=fx*fx*(3-2*fx);fy=fy*fy*(3-2*fy);
  return Mix(Mix(Hash(ix,iy),Hash((ix+1)%grid,iy),fx),Mix(Hash(ix,(iy+1)%grid),Hash((ix+1)%grid,(iy+1)%grid),fx),fy);
 }
 public static SurfaceSample Sample(SurfaceStyle style,float u,float v){
  u=Frac(u);v=Frac(v);float broad=Noise(u,v,4),fine=Noise(u,v,32),grain=Noise(u,v,128);float shade=.78f+fine*.18f;
  var s=new SurfaceSample{r=shade,g=shade,b=shade,height=.5f+fine*.08f,metallic=0,smoothness=.2f};
  switch(style){
   case SurfaceStyle.Steel:{float rust=Clamp((broad-.49f)*5);s.r=Mix(.93f,.85f,rust);s.g=Mix(.95f,.40f,rust);s.b=Mix(.97f,.16f,rust);s.height=.5f+rust*.08f+grain*.02f;s.metallic=1-rust*.95f;s.smoothness=Mix(.48f,.1f,rust);break;}
   case SurfaceStyle.Wood:{float rings=(float)Math.Sin((u*22+Noise(u,v,8)*.5f)*Math.PI*2);float knots=Noise(u,v,4);shade=.66f+rings*.10f+fine*.18f+knots*.05f;s.r=shade;s.g=shade*.94f;s.b=shade*.84f;s.height=.5f+rings*.035f+grain*.02f;s.smoothness=.15f+fine*.1f;break;}
   case SurfaceStyle.Paint:{float wear=Clamp((broad-.58f)*8);s.r=Mix(.96f,.48f,wear);s.g=Mix(.97f,.51f,wear);s.b=Mix(.94f,.45f,wear);s.height=.55f-wear*.03f+grain*.01f;s.metallic=wear*.7f;s.smoothness=.22f+fine*.1f;break;}
   case SurfaceStyle.Fabric:{float weave=(float)(Math.Sin(u*Math.PI*2*64)*Math.Sin(v*Math.PI*2*64));s.r=s.g=s.b=.82f+weave*.08f+fine*.08f;s.height=.5f+weave*.04f;s.smoothness=.06f;break;}
   case SurfaceStyle.Leather:{s.r=s.g=s.b=.72f+fine*.18f+grain*.08f;s.height=.5f+grain*.05f;s.smoothness=.22f+fine*.15f;break;}
   case SurfaceStyle.Plaster:{s.r=s.g=s.b=.75f+broad*.12f+fine*.08f;s.height=.5f+fine*.04f+grain*.02f;s.smoothness=.08f;break;}
   case SurfaceStyle.Brick:{float row=(float)Math.Floor(v*8),x=Frac(u*4+(row%2)*.5f),y=Frac(v*8);float mortar=x<.035f || y<.07f?1:0;s.r=Mix(.85f,.65f,mortar);s.g=Mix(.70f,.68f,mortar);s.b=Mix(.59f,.64f,mortar);s.height=Mix(.56f,.46f,mortar)+fine*.015f;s.smoothness=.1f;break;}
  }
  s.r=Clamp(s.r);s.g=Clamp(s.g);s.b=Clamp(s.b);return s;
 }
 // Tangent-space normal, evaluated with periodic central differences.
 public static void Normal(SurfaceStyle style,float u,float v,out float x,out float y,out float z){
  const float step=1f/256;float dx=(Sample(style,u+step,v).height-Sample(style,u-step,v).height)*6;
  float dy=(Sample(style,u,v+step).height-Sample(style,u,v-step).height)*6;float length=(float)Math.Sqrt(dx*dx+dy*dy+1);x=-dx/length;y=-dy/length;z=1/length;
 }
}
}

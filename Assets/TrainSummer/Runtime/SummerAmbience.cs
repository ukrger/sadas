using UnityEngine;
namespace TrainSummer {
// Original synthesized placeholder audio, generated once. Replace with recorded audio for final art.
public sealed class SummerAmbience:MonoBehaviour {
 public TrainMotor train;public SummerLighting clock;
 AudioSource rails,air;AudioClip railClip,airClip;
 void Awake(){
  const int rate=22050,length=rate*4;var a=new float[length];var b=new float[length];var random=new System.Random(913);float low=0;
  for(int i=0;i<length;i++){
   float t=i/(float)rate;float n=(float)(random.NextDouble()*2-1);low=Mathf.Lerp(low,n,.025f);
   float clack=Mathf.Exp(-Mathf.Repeat(t,.5f)*70);
   a[i]=Mathf.Sin(t*Mathf.PI*2*55)*.07f+low*.5f+n*clack*.15f;
   float bird=Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*Mathf.PI*2*.75f)),18)*Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*Mathf.PI*2*7)),8);
   b[i]=low*.12f+Mathf.Sin(t*Mathf.PI*2*2100+Mathf.Sin(t*Mathf.PI*2*3)*18)*bird*.06f;
  }
  railClip=Clip("Synthesized rail rhythm",a,rate);airClip=Clip("Synthesized summer air",b,rate);
  rails=Source(railClip);air=Source(airClip);rails.volume=0;air.volume=.2f;
 }
 AudioClip Clip(string label,float[] data,int rate){var c=AudioClip.Create(label,data.Length,1,rate,false);c.SetData(data,0);return c;}
 AudioSource Source(AudioClip clip){var s=gameObject.AddComponent<AudioSource>();s.clip=clip;s.spatialBlend=0;s.loop=true;s.playOnAwake=false;s.Play();return s;}
 void Update(){float speed=train!=null?train.Speed/Mathf.Max(.1f,train.cruiseSpeed):0;rails.volume=Mathf.Clamp01(speed)*.45f;rails.pitch=Mathf.Lerp(.65f,1.15f,speed);air.volume=.10f+.16f*(clock!=null?clock.Daylight:1);}
 void OnDestroy(){if(railClip!=null)Destroy(railClip);if(airClip!=null)Destroy(airClip);}
}
}

using UnityEngine;
namespace TrainSummer {
[RequireComponent(typeof(AudioSource))]public sealed class ProceduralAudio:MonoBehaviour {
 AudioSource source;AudioClip shot,shotgun;
 void Awake(){source=GetComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;shot=AudioClip.Create("Procedural pistol shot",5512,1,22050,false);
  var samples=new float[5512];var random=new System.Random(712);
  for(int i=0;i<samples.Length;i++){float t=i/22050f;float noise=(float)(random.NextDouble()*2-1);samples[i]=(.45f*noise+.2f*Mathf.Sin(t*480))*Mathf.Exp(-t*36);}
  shot.SetData(samples,0);
  shotgun=AudioClip.Create("Procedural shotgun report",8820,1,22050,false);var heavy=new float[8820];
  for(int i=0;i<heavy.Length;i++){float t=i/22050f;heavy[i]=(.65f*(float)(random.NextDouble()*2-1)+.3f*Mathf.Sin(t*310))*Mathf.Exp(-t*20);}shotgun.SetData(heavy,0);
 }
 public void Shot(){if(source!=null && shot!=null)source.PlayOneShot(shot,.35f);}
 public void Shotgun(){if(source!=null && shotgun!=null)source.PlayOneShot(shotgun,.45f);}
 void OnDestroy(){if(shot!=null)Destroy(shot);if(shotgun!=null)Destroy(shotgun);}
}
}

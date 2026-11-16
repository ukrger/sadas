using UnityEngine;
namespace TrainSummer {
[RequireComponent(typeof(AudioSource))]public sealed class ProceduralAudio:MonoBehaviour {
 AudioSource source;AudioClip shot;
 void Awake(){source=GetComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;shot=AudioClip.Create("Procedural pistol shot",5512,1,22050,false);
  var samples=new float[5512];var random=new System.Random(712);
  for(int i=0;i<samples.Length;i++){float t=i/22050f;float noise=(float)(random.NextDouble()*2-1);samples[i]=(.45f*noise+.2f*Mathf.Sin(t*480))*Mathf.Exp(-t*36);}
  shot.SetData(samples,0);
 }
 public void Shot(){if(source!=null && shot!=null)source.PlayOneShot(shot,.35f);}
 void OnDestroy(){if(shot!=null)Destroy(shot);}
}
}

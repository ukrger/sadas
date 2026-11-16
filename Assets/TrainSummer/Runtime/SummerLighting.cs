using UnityEngine;
namespace TrainSummer {
public sealed class SummerLighting:MonoBehaviour {
 public Light sun,moon;public float dayLengthSeconds=1200;[Range(0,1)]public float timeOfDay=.4f;
 public float Daylight {get;private set;}
 Material previousSky,runtimeSky;
 void OnEnable(){previousSky=RenderSettings.skybox;if(previousSky!=null){runtimeSky=Instantiate(previousSky);runtimeSky.name="Runtime Summer Sky";RenderSettings.skybox=runtimeSky;}Apply();}
 void Update(){timeOfDay=Mathf.Repeat(timeOfDay+Time.deltaTime/Mathf.Max(60,dayLengthSeconds),1);Apply();}
 public void Apply(){
  if(sun==null)return;
  float angle=timeOfDay*360-90;sun.transform.rotation=Quaternion.Euler(angle,145,0);
  float height=Mathf.Sin(angle*Mathf.Deg2Rad);Daylight=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.08f,.3f,height));
  sun.intensity=1.2f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.02f,.25f,height));
  sun.color=Color.Lerp(new Color(1,.53f,.28f),new Color(1,.96f,.85f),Mathf.Clamp01(height*3));
  if(moon!=null){moon.transform.rotation=Quaternion.Euler(angle+180,145,0);moon.color=new Color(.55f,.65f,1);moon.intensity=.13f*(1-Daylight);RenderSettings.sun=Daylight>.05f?sun:moon;}
  RenderSettings.ambientLight=Color.Lerp(new Color(.025f,.04f,.075f),new Color(.48f,.57f,.66f),Daylight);
  RenderSettings.fogColor=Color.Lerp(new Color(.015f,.025f,.055f),new Color(.65f,.78f,.87f),Daylight);
  if(runtimeSky!=null && runtimeSky.HasProperty("_Daylight")){runtimeSky.SetFloat("_Daylight",Daylight);runtimeSky.SetVector("_SunDirection",-sun.transform.forward);runtimeSky.SetVector("_MoonDirection",sun.transform.forward);}
 }
 void OnDisable(){if(runtimeSky!=null){if(RenderSettings.skybox==runtimeSky)RenderSettings.skybox=previousSky;Destroy(runtimeSky);runtimeSky=null;}}
}
}

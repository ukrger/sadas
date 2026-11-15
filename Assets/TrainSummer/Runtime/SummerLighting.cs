using UnityEngine;
namespace TrainSummer {
public sealed class SummerLighting:MonoBehaviour {
 public Light sun; public float dayLengthSeconds=1200; [Range(0,1)]public float timeOfDay=0.4f;
 void Update(){
  timeOfDay=Mathf.Repeat(timeOfDay+Time.deltaTime/Mathf.Max(60,dayLengthSeconds),1);
  float angle=timeOfDay*360-90;sun.transform.rotation=Quaternion.Euler(angle,145,0);
  float height=Mathf.Sin(angle*Mathf.Deg2Rad);
  sun.intensity=Mathf.Lerp(0,1.2f,Mathf.SmoothStep(0,0.35f,height));
  sun.color=Color.Lerp(new Color(1,0.53f,0.28f),new Color(1,0.96f,0.85f),Mathf.Clamp01(height*3));
  RenderSettings.ambientLight=Color.Lerp(new Color(.025f,.04f,.075f),new Color(.48f,.57f,.66f),Mathf.Clamp01(height*2));
  RenderSettings.fogColor=Color.Lerp(new Color(.015f,.025f,.055f),new Color(.65f,.78f,.87f),Mathf.Clamp01(height*2));
 }
}
}

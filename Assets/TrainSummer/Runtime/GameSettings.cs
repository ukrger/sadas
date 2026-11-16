using System;
namespace TrainSummer {
[Serializable]public sealed class GameSettings {
 public int schema=1,quality=1,textureLimit=0,shadowResolution=2048,msaa=2,screenMode=0,width=1280,height=720,fpsLimit=60;
 public float shadowDistance=80,drawDistance=300,renderScale=1,sensitivity=.12f;
 public bool english,showFps=true,postProcessing=true,vSync;
}
}

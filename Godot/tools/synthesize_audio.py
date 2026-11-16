"""Original placeholder game audio: no third-party recordings. Run with Python 3."""
import wave,math,random,struct
from pathlib import Path
out=Path(__file__).resolve().parents[1]/'assets';rate=22050
for name,seconds in [('train',4),('summer',8),('shot',.24),('foot',.12)]:
 rng=random.Random(913);low=0.;samples=[]
 for i in range(int(rate*seconds)):
  t=i/rate;n=rng.uniform(-1,1);low+=.025*(n-low)
  if name=='train':value=.07*math.sin(t*math.tau*55)+low*.5+n*.15*math.exp(-(t%.5)*70)
  elif name=='summer':
   chirp=max(0,math.sin(t*math.tau*.375))**18*max(0,math.sin(t*math.tau*7))**8
   value=low*.10+math.sin(t*math.tau*2100+math.sin(t*math.tau*3)*18)*chirp*.06
  elif name=='shot':value=(n*.5+math.sin(t*480)*.2)*math.exp(-t*36)
  else:value=(low+n*.06)*math.exp(-t*45)
  samples.append(struct.pack('<h',round(max(-1,min(1,value))*32767)))
 with wave.open(str(out/(name+'.wav')),'wb') as f:f.setparams((1,2,rate,0,'NONE','not compressed'));f.writeframes(b''.join(samples))
 print('Generated',name)

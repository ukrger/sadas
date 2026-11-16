"""Original rail signal prop, with separate red/green lenses for Unity property blocks."""
import sys,argparse
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from prop_pipeline import PropBuilder
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
b=PropBuilder(a.project,'RailSignal',[('SignalPaint',(.18,.22,.17),.4),('GunSteel',(.12,.14,.15),.8),('SignalRed',(.48,.025,.015),.1),('SignalGreen',(.025,.45,.11),.1),('GearMark',(.77,.71,.5),0)])
for lod in range(3):
 b.begin(lod)
 b.box('Concrete foot',(0,.10,0),(.7,.2,.7),0,.03);b.cylinder('Post',(0,1.40,0),.075,2.6,1)
 b.box('Signal housing',(0,2.9,0),(.42,.88,.20),0,.025)
 for y,mi in [(3.13,2),(2.72,3)]:
  b.cylinder('Lamp surround',(0,y,.12),.145,.08,1,'z');b.cylinder('Lens',(0,y,.166),.112,.012,mi,'z');b.box('Lens hood',(0,y+.15,.20),(.32,.035,.28),1,.008)
 if lod<2:
  b.box('Number plate',(0,2.1,.095),(.29,.23,.025),4,.008)
  for y in [.4,.7,1.0,1.3,1.6,1.9]:b.box('Ladder rung',(.12,y,-.11),(.30,.025,.04),1,.003)
 if lod==0:
  for x in [-.24,.24]:
   for z in [-.24,.24]:b.cylinder('Foundation bolt',(x,.22,z),.025,.04,1)
 b.finish()
b.export()
if a.render:b.render(1.65,5.2,(4,-6,3.3))

"""blender -b --python generate_repair_bench.py -- --project /UnityProject [--render]"""
import sys,argparse,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from prop_pipeline import PropBuilder
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
b=PropBuilder(a.project,'RepairBench',[('PropWood',(.30,.19,.09),0),('PropSteel',(.13,.15,.14),.75),('PropPaint',(.26,.32,.16),.2),('PropMark',(.65,.57,.34),0)])
for lod in range(3):
 b.begin(lod)
 b.box('Worktop',(0,.94,0),(1.8,.12,.7),0)
 for x in [-.76,.76]:
  for z in [-.26,.26]:b.box('Leg',(x,.45,z),(.1,.9,.1),1)
 b.box('Bottom shelf',(0,.22,0),(1.6,.08,.58),0)
 b.box('Backboard',(0,1.35,.30),(1.8,.75,.07),2)
 if lod<2:
  for x in [-.76,.76]:b.box('Support',(x,1.3,.28),(.08,.9,.08),1)
  b.box('Vise base',(-.56,1.04,-.18),(.35,.08,.22),1)
  b.box('Vise body',(-.56,1.13,-.12),(.25,.16,.12),2)
  for z in [-.21,.03]:b.box('Vise jaw',(-.56,1.19,z),(.31,.12,.055),1)
  b.cylinder('Vise screw',(-.56,1.08,-.33),.025,.3,1,'z')
  b.cylinder('Vise handle',(-.56,1.08,-.48),.014,.27,1,'y')
  b.box('Toolbox',(.42,1.12,.04),(.48,.24,.29),2)
  b.box('Toolbox lid',(.42,1.26,.04),(.5,.04,.31),1)
  b.box('Toolbox handle',(.42,1.33,.04),(.2,.04,.03),1)
  for x in [.33,.51]:b.box('Handle riser',(x,1.30,.04),(.03,.08,.03),1)
  for x in [-.1,.18,.46]:
   b.box('Hanging tool',(x,1.45,.245),(.035,.30,.025),1)
   for side in [-1,1]:b.box('Wrench jaw',(x+side*.035,1.60,.245),(.025,.09,.025),1)
 if lod==0:
  for x in [-.76,.76]:
   for y in [.28,.84]:b.cylinder('Bolt',(x,y,-.318),.018,.012,3,'z')
  for z in [-.22,0,.22]:b.box('Worktop seam',(0,1.003,z),(1.77,.004,.009),1,0)
  for x in [-.65,-.3,.05,.4,.75]:
   for y in [1.16,1.36,1.56]:b.cylinder('Pegboard screw',(x,y,.255),.009,.012,1,'z')
 b.finish()
b.export()
if a.render:b.render(.88,3.25)

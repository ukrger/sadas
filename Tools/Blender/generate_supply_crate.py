"""blender -b --python generate_supply_crate.py -- --project /UnityProject [--render]"""
import sys,argparse
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from prop_pipeline import PropBuilder
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
b=PropBuilder(a.project,'SupplyCrate',[('PropWood',(.30,.19,.09),0),('PropSteel',(.13,.15,.14),.75),('PropPaint',(.26,.32,.16),.2),('PropMark',(.65,.57,.34),0)])
for lod in range(3):
 b.begin(lod)
 b.box('Base',(0,.05,0),(.94,.1,.64),0)
 if lod<2:
  for i in range(4):
   for side in [-1,1]:b.box('Side plank',(0,.15+i*.13,side*.3),(.90,.12,.06),0)
  for side in [-1,1]:
   b.box('End',(side*.45,.36,0),(.06,.57,.60),0)
   for z in [-.22,.22]:b.box('Corner binding',(side*.48,.36,z),(.035,.6,.065),1)
 else:b.box('Body',(0,.35,0),(.94,.6,.64),0)
 b.box('Lid',(0,.68,0),(.98,.09,.68),2)
 for x in [-.3,.3]:
  b.box('Lid strap',(x,.735,0),(.055,.025,.69),1)
  b.box('Front binding',(x,.37,-.34),(.055,.62,.025),1)
  b.box('Rear binding',(x,.37,.34),(.055,.62,.025),1)
 if lod<2:
  for side in [-1,1]:
   b.box('Handle top',(side*.52,.45,0),(.035,.035,.25),1)
   for z in [-.11,.11]:b.box('Handle bracket',(side*.49,.41,z),(.08,.09,.035),1)
  b.box('Latch',(0,.64,-.37),(.12,.17,.035),1)
  b.box('Label',(0,.39,-.343),(.25,.16,.012),3)
 if lod==0:
  for x in [-.3,.3]:
   for y in [.15,.4,.62]:b.cylinder('Rivet',(x,y,-.36),.014,.012,1,'z')
  for z in [-.23,-.08,.08,.23]:b.box('Lid seam',(0,.728,z),(.95,.004,.009),0,0)
 b.finish()
b.export()
if a.render:b.render(.36,1.8)

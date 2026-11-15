"""blender -b --python generate_medkit.py -- --project /UnityProject [--render]"""
import sys,argparse
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from prop_pipeline import PropBuilder
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
b=PropBuilder(a.project,'FieldMedkit',[('GearCanvas',(.20,.25,.12),0),('GearStrap',(.10,.12,.075),0),('GearSteel',(.22,.23,.20),.7),('GearMark',(.77,.71,.50),0)])
for lod in range(3):
 b.begin(lod)
 b.box('Medical case',(0,.14,0),(.32,.25,.16),0,.025)
 b.box('Lid',(0,.285,0),(.33,.05,.17),0,.012)
 # Neutral medical identification: simple light patch with green plus.
 b.box('Patch',(0,.16,-.085),(.14,.13,.012),3,.006)
 b.box('Plus upright',(0,.16,-.094),(.025,.095,.007),0,.002)
 b.box('Plus crossbar',(0,.16,-.095),(.095,.025,.007),0,.002)
 if lod<2:
  for x in [-.105,.105]:
   b.box('Latch',(x,.26,-.094),(.035,.075,.02),2)
   b.box('Handle riser',(x*.65,.335,0),(.025,.07,.025),1)
  b.box('Handle',(0,.37,0),(.16,.025,.025),1)
 if lod==0:
  for side in [-1,1]:
   b.box('Corner guard',(side*.16,.08,0),(.018,.12,.16),2,.004)
   b.box('Rear hinge',(side*.105,.27,.09),(.06,.035,.025),2,.004)
 b.finish()
b.export()
if a.render:b.render(.18,.75)

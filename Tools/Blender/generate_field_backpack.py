"""blender -b --python generate_field_backpack.py -- --project /UnityProject [--render]"""
import sys,argparse
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from prop_pipeline import PropBuilder
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
b=PropBuilder(a.project,'FieldBackpack',[('GearCanvas',(.20,.25,.12),0),('GearStrap',(.10,.12,.075),0),('GearSteel',(.22,.23,.20),.7),('GearLeather',(.24,.13,.055),0)])
for lod in range(3):
 b.begin(lod)
 b.box('Canvas body',(0,.30,0),(.42,.55,.22),0,.055)
 b.box('Top flap',(0,.59,-.01),(.44,.10,.25),0,.035)
 b.box('Front pocket',(0,.22,-.15),(.32,.25,.12),0,.025)
 if lod<2:
  for side in [-1,1]:
   b.box('Side pocket',(side*.245,.24,0),(.10,.28,.18),0,.025)
   b.box('Flap strap',(side*.12,.44,-.142),(.035,.27,.02),1)
   b.box('Buckle',(side*.12,.35,-.16),(.055,.055,.015),2)
   b.box('Shoulder strap',(side*.12,.32,.15),(.07,.43,.035),1,.018)
  b.box('Carry handle',(0,.69,.04),(.16,.035,.035),1)
  for x in [-.065,.065]:b.box('Handle riser',(x,.65,.04),(.035,.07,.035),1)
 if lod==0:
  for y in [.15,.27]:b.box('Pocket webbing',(0,y,-.216),(.30,.025,.008),1,.002)
  b.box('Leather patch',(0,.51,-.146),(.085,.055,.012),3,.005)
  for side in [-1,1]:b.box('Side compression',(side*.30,.3,0),(.012,.035,.17),1,.003)
 b.finish()
b.export()
if a.render:b.render(.35,1.25)

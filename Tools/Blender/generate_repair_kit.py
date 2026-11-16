"""blender -b --python generate_repair_kit.py -- --project /UnityProject [--render]"""
import sys,argparse
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from prop_pipeline import PropBuilder
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
b=PropBuilder(a.project,'RepairKit',[('SupplyKit',(.38,.29,.10),.2),('GearSteel',(.22,.23,.20),.7),('GearStrap',(.10,.12,.075),0),('SupplyLabel',(.68,.70,.54),0)])
for lod in range(3):
 b.begin(lod)
 b.box('Tool roll',(0,.065,0),(.29,.12,.17),0,.016)
 b.box('Folded flap',(0,.13,0),(.30,.035,.18),0,.012)
 for x in [-.09,.09]:b.box('Binding strap',(x,.083,0),(.025,.14,.183),2,.003)
 b.box('Repair label',(0,.085,-.09),(.095,.055,.005),3,.003)
 if lod<2:
  for x in [-.09,.09]:b.box('Buckle',(x,.09,-.098),(.04,.045,.012),1,.002)
  b.box('Carry handle',(0,.17,.015),(.13,.025,.025),2,.005)
  for x in [-.055,.055]:b.box('Handle riser',(x,.148,.015),(.018,.045,.022),2,.003)
 if lod==0:
  for x in [-.11,.0,.11]:b.cylinder('Fastener',(x,.148,.05),.008,.005,1)
 b.finish()
b.export()
if a.render:b.render(.09,.58)

"""blender -b --python generate_ammo_box.py -- --project /UnityProject [--render]"""
import sys,argparse
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from prop_pipeline import PropBuilder
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
b=PropBuilder(a.project,'AmmoBox',[('AmmoCard',(.39,.29,.15),0),('AmmoBrass',(.55,.39,.12),.8),('AmmoLead',(.25,.22,.18),.6),('GearMark',(.77,.71,.50),0)])
for lod in range(3):
 b.begin(lod)
 b.box('Carton',(0,.035,0),(.13,.07,.09),0,.004)
 b.box('Label',(0,.04,-.047),(.09,.035,.004),3,.001)
 b.box('Open lid',(0,.079,.048),(.13,.012,.075),0,.002)
 for i in range(6 if lod==0 else (3 if lod==1 else 1)):
  x=(i%3-1)*.031;z=(i//3-.5)*.032
  b.cylinder('Cartridge',(x,.066,z),.010,.055,1)
  b.cylinder('Bullet tip',(x,.10,z),.007,.014,2)
 if lod==0:
  for x in [-.064,.064]:b.box('Carton fold',(x,.04,0),(.003,.06,.086),0,.001)
 b.finish()
b.export()
if a.render:b.render(.05,.28)

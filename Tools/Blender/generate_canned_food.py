"""blender -b --python generate_canned_food.py -- --project /UnityProject [--render]"""
import sys,argparse,bpy
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from prop_pipeline import PropBuilder
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
b=PropBuilder(a.project,'CannedFood',[('SupplyCan',(.42,.32,.15),.25),('GearSteel',(.22,.23,.20),.7),('SupplyLabel',(.68,.70,.54),0)])
for lod in range(3):
 b.begin(lod)
 b.cylinder('Tin body',(0,.055,0),.055,.105,0)
 for y in [.006,.108]:b.cylinder('Rolled rim',(0,y,0),.057,.008,1)
 b.cylinder('Lid',(0,.112,0),.052,.004,1)
 b.box('Food label',(0,.06,-.055),(.072,.06,.004),2,.002)
 if lod<2:
  b.box('Pull tab hinge',(0,.116,0),(.012,.006,.02),1,.001)
  bpy.ops.mesh.primitive_torus_add(major_segments=16 if lod==0 else 8,minor_segments=6 if lod==0 else 4,location=(0,.014,.119),major_radius=.012,minor_radius=.0025)
  obj=bpy.context.object;obj.name='Pull ring';obj.data.materials.append(b.mats[1]);b.parts.append(obj)
 if lod==0:
  for y in [.02,.09]:b.cylinder('Seam',(0,y,0),.056,.003,1)
 b.finish()
b.export()
if a.render:b.render(.06,.26)

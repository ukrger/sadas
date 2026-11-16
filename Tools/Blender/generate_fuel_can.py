"""Original fuel can, updated to the shared semantic-axis/three-LOD export pipeline.
blender -b --python generate_fuel_can.py -- --project /UnityProject [--render]
"""
import sys,argparse,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from prop_pipeline import PropBuilder
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
b=PropBuilder(a.project,'FuelCan',[('PropPaint',(.26,.32,.16),.2),('PropSteel',(.13,.15,.14),.75),('PropMark',(.65,.57,.34),0)])
for lod in range(3):
 b.begin(lod);b.box('Body',(0,.275,0),(.34,.53,.16),0,.025)
 for x in [-.10,.10]:b.box('Handle support',(x,.595,0),(.035,.13,.055),0,.009)
 b.box('Carry handle',(0,.66,0),(.23,.035,.055),0,.009);b.cylinder('Cap',(.105,.56,0),.04,.035,1)
 if lod<2:
  for side in [-1,1]:
   for angle in [-.65,.65]:
    rib=b.box('Pressed reinforcement',(0,.29,side*.083),(.025,.32,.012),0,.005);rib.rotation_euler.y=angle
 if lod==0:b.box('Identification plate',(0,.40,-.085),(.12,.06,.008),2,.002)
 b.finish()
b.export()
if a.render:b.render(.35,.90,(1.0,1.4,.9))

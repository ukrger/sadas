"""blender -b --python generate_overgrown_house.py -- --project /UnityProject [--render]"""
import sys,argparse,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from environment_pipeline import *
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
b=PropBuilder(a.project,'OvergrownHouse',PALETTE)
ivy=[((-3.14,1.5,-1.1),(.24,1.3,1.5)),((-3.14,1.7,1.7),(.22,1.3,1)),((2.9,1.2,-2.65),(1.4,1.1,.18)),((0,3.1,1.8),(1.7,.45,1.1))]
for lod in range(3):
 b.begin(lod)
 b.box('Foundation',(0,.1,0),(6.6,.2,5.6),4,.02);b.box('Floor',(0,.23,0),(6.1,.06,5.1),3,.01)
 for side in [-1,1]:
  x=side*3.15
  b.box('Lower side',(x,.73,0),(.22,1,5.4),3,.025);b.box('Top side',(x,2.83,0),(.22,.45,5.4),3,.02)
  for z in [-2.65,-1.8,0,1.8,2.65]:b.box('Window pillar',(x,1.915,z),(.23,1.39,.22),4,.015)
 for x in [-2.0,2.0]:
  b.box('Front lower',(x,.73,-2.7),(2.3,1,.22),3,.02);b.box('Front upper',(x,2.83,-2.7),(2.3,.45,.22),3,.02)
  for px in [x-1.1,x+1.1]:b.box('Front pillar',(px,1.915,-2.7),(.23,1.39,.24),4,.015)
 b.box('Door lintel',(0,2.83,-2.7),(1.7,.45,.24),4,.02)
 b.box('Rear wall',(0,1.63,2.7),(6.4,2.8,.22),3,.025)
 # Broken roof: one front quadrant is missing, revealing rafters and the interior.
 slope=math.atan2(1.0,2.85)
 for side in [-1,1]:
  for x in [-1.6,1.6]:
   if side==-1 and x>0:continue
   obj=b.box('Roof sheet',(x,3.57,side*1.425),(3.18,.12,3.02),5,.01);obj.rotation_euler.x=side*slope
 if lod<2:
  for x in [-2.8,-1.4,0,1.4,2.8]:
   for side in [-1,1]:
    obj=b.box('Rafter',(x,3.49,side*1.425),(.11,.14,3.02),0,.004);obj.rotation_euler.x=side*slope
  for x,z in [(-2.4,-3.1),(1.7,-3.3),(2.8,2.9),(-2.8,3.1)]:b.box('Rubble',(x,.15,z),(.5,.3,.38),4,.015)
  for z in [-1.8,-.9,.9,1.8]:branch(b,(-3.29,.08,z),(-3.29,2.9,z+.35),.014,.005,5,6)
  leaf_cloud(b,ivy,500 if lod==0 else 140,.23 if lod==0 else .39,481,6)
 else:distant_crown(b,ivy,6)
 if lod==0:
  for x in [-2.9,-1.3,1.3,2.9]:
   for y in [.35,.62,.89]:b.box('Exposed brick',(x,y,-2.83),(.3,.12,.045),4,.006)
 b.finish()
b.export()
if a.render:outdoor_render(b,1.8,11,(11,13,8))

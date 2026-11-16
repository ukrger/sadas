"""blender -b --python generate_summer_pine.py -- --project /UnityProject [--render]"""
import sys,argparse,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from environment_pipeline import *
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
b=PropBuilder(a.project,'SummerPine',PALETTE)
centers=[]
for layer in range(7):
 height=3.4+layer*.93;radius=2.1-layer*.24
 for spoke in range(5):
  angle=spoke*math.tau/5+layer*.47;centers.append(((math.cos(angle)*radius*.65,height,math.sin(angle)*radius*.65),(radius*.64,.55,radius*.64)))
centers.append(((0,10,0),(.4,.7,.4)))
for lod in range(3):
 b.begin(lod);branch(b,(0,0,0),(0,10.5,0),.25,.018,[12,8,5][lod])
 for i,(center,extent) in enumerate(centers[:-1]):
  if lod>0 and i%2:continue
  branch(b,(0,center[1]-.15,0),(center[0]*1.2,center[1],center[2]*1.2),.045,.008,[6,4,3][lod])
 if lod<2:leaf_cloud(b,centers,[10000,2300][lod],[.30,.48][lod],555,2,needle=True)
 else:distant_crown(b,centers[::3],2)
 b.finish()
b.export()
if a.render:outdoor_render(b,5.1,14,(14,16,10))

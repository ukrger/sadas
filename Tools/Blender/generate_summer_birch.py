"""blender -b --python generate_summer_birch.py -- --project /UnityProject [--render]"""
import sys,argparse,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from environment_pipeline import *
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
b=PropBuilder(a.project,'SummerBirch',PALETTE);b.mats[0].name='EnvBirchBark';b.mats[0].diffuse_color=(.55,.52,.42,1);b.mats[0].node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=b.mats[0].diffuse_color
centers=[]
for i in range(14):
 angle=i*2.39996;h=4.5+(i%5)*.65;radius=1.2+(i%3)*.35;centers.append(((math.cos(angle)*radius,h+1,math.sin(angle)*radius),(1.25,1.2,1.25)))
centers.append(((0,7.9,0),(1.3,1.2,1.3)))
for lod in range(3):
 b.begin(lod);branch(b,(0,0,0),(.08,6.8,0),.22,.045,[12,8,6][lod])
 for i,(center,extent) in enumerate(centers[:-1]):
  if lod==2 and i%2:continue
  end=(center[0]*.9,center[1]-.25,center[2]*.9);branch(b,(0,3.3+(i%5)*.55,0),end,.065,.012,[7,5,4][lod])
 if lod<2:leaf_cloud(b,centers,[1800,430][lod],[.26,.46][lod],118,1)
 else:distant_crown(b,centers,1)
 b.finish()
b.export()
if a.render:outdoor_render(b,4.4,12.2,(12,15,10))

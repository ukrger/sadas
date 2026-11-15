"""blender -b --python generate_survival_axe.py -- --project /UnityProject [--render]"""
import sys,argparse,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from prop_pipeline import PropBuilder
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
b=PropBuilder(a.project,'SurvivalAxe',[('AxeWood',(.31,.16,.065),0),('GearSteel',(.22,.23,.20),.7),('AxeEdge',(.56,.58,.55),.9),('GearStrap',(.10,.12,.075),0)])
for lod in range(3):
 b.begin(lod)
 b.box('Wooden haft',(0,.34,0),(.045,.65,.04),0,.012)
 b.box('Grip',(0,.15,0),(.052,.25,.047),3,.012)
 # Custom extruded wedge: cutting edge to the left, thick eye around the handle.
 outline=[(-.23,.51),(-.28,.70),(-.19,.77),(.025,.70),(.075,.69),(.075,.59),(-.08,.59)]
 verts=[]
 for side in [-1,1]:
  for x,y in outline:
   thickness=.007 if x<-.18 else .035
   verts.append((x,-side*thickness,y))
 n=len(outline);faces=[tuple(range(n-1,-1,-1)),tuple(range(n,n*2))]
 for i in range(n):faces.append((i,(i+1)%n,(i+1)%n+n,i+n))
 import bpy
 mesh=bpy.data.meshes.new('ForgedHead');mesh.from_pydata(verts,[],faces);mesh.update();obj=bpy.data.objects.new('ForgedHead',mesh);bpy.context.collection.objects.link(obj);obj.data.materials.append(b.mats[1]);b.parts.append(obj)
 b.box('Cutting edge',(-.248,.607,0),(.025,.205,.015),2,.003).rotation_euler[1]=-.25
 if lod<2:
  b.box('Head collar',(0,.65,0),(.075,.11,.085),1,.006)
  b.cylinder('Handle pin',(0,.65,-.049),.009,.01,2,'z')
 if lod==0:
  for y in [.06,.10,.14,.18,.22]:b.box('Grip wrap',(0,y,0),(.055,.014,.050),3,.003)
 b.finish()
b.export()
if a.render:b.render(.38,1.3)

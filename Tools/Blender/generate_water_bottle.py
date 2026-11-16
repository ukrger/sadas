"""blender -b --python generate_water_bottle.py -- --project /UnityProject [--render]"""
import sys,argparse,math,bpy
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from prop_pipeline import PropBuilder
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
b=PropBuilder(a.project,'WaterBottle',[('SupplyBottle',(.14,.29,.25),.15),('SupplyLabel',(.68,.70,.54),0),('GearStrap',(.10,.12,.075),0)])
for lod in range(3):
 b.begin(lod);n=[24,14,8][lod]
 profile=[(.0,.031),(.012,.043),(.04,.046),(.17,.046),(.20,.04),(.225,.022),(.247,.022)]
 verts=[]
 for y,r in profile:
  for i in range(n):
   t=i*2*math.pi/n;verts.append((math.cos(t)*r,math.sin(t)*r,y))
 faces=[tuple(range(n-1,-1,-1)),tuple(range((len(profile)-1)*n,len(profile)*n))]
 for j in range(len(profile)-1):
  for i in range(n):faces.append((j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i))
 mesh=bpy.data.meshes.new('BottleProfile');mesh.from_pydata(verts,[],faces);mesh.update();obj=bpy.data.objects.new('Bottle body',mesh);bpy.context.collection.objects.link(obj);obj.data.materials.append(b.mats[0]);b.parts.append(obj)
 for face in mesh.polygons:face.use_smooth=True
 b.cylinder('Screw cap',(0,.255,0),.026,.025,2)
 b.box('Label',(0,.125,-.046),(.065,.075,.004),1,.003)
 if lod<2:
  for y in [.04,.07,.18]:b.cylinder('Moulded ring',(0,y,0),.047,.006,0)
 if lod==0:
  for i in range(12):
   angle=i*math.pi/6;b.box('Cap grip',(math.cos(angle)*.026,.255,math.sin(angle)*.026),(.005,.02,.005),2,.001)
 b.finish()
b.export()
if a.render:b.render(.13,.48)

"""blender -b --python generate_station_house.py -- --project /UnityProject [--render]"""
import bpy,math,argparse,sys,json
from pathlib import Path
from mathutils import Vector
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [];a=p.parse_args(args)
project=Path(a.project).resolve();out=project/'Assets/Models';out.mkdir(parents=True,exist_ok=True);source=project/'Tools/Blender/Source';source.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.context.scene.unit_settings.system='METRIC'
def material(name,c,metal=0):
 m=bpy.data.materials.new(name);m.diffuse_color=(*c,1);m.use_nodes=True;b=m.node_tree.nodes['Principled BSDF'];b.inputs['Base Color'].default_value=(*c,1);b.inputs['Metallic'].default_value=metal;b.inputs['Roughness'].default_value=.8;return m
mats=[material('HouseWood',(.24,.17,.10)),material('HousePaint',(.24,.34,.29)),material('HouseRoof',(.21,.16,.12),.6),material('HouseStone',(.34,.33,.29)),material('HouseTrim',(.43,.37,.24))]
root=bpy.data.objects.new('StationHouse',None);bpy.context.collection.objects.link(root)
parts=[];level=0
def xyz(p):return (p[0],-p[2],p[1])
def box(name,p,size,mi=0,bevel=.01):
 bpy.ops.mesh.primitive_cube_add(size=1,location=xyz(p));o=bpy.context.object;o.name=name;o.dimensions=(size[0],size[2],size[1]);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(mats[mi])
 if level==0 and bevel:
  b=o.modifiers.new('Worn edges','BEVEL');b.width=bevel;b.segments=2;bpy.ops.object.modifier_apply(modifier=b.name)
 parts.append(o);return o
stats={}
for level in range(3):
 parts=[];group=bpy.data.objects.new('LOD'+str(level),None);bpy.context.collection.objects.link(group);group.parent=root
 box('Foundation',(0,.1,0),(6,.2,5),3)
 box('Floor',(0,.24,0),(5.8,.08,4.8),0)
 # Window and doorway apertures are actual gaps, not painted black rectangles.
 for side in [-1,1]:
  x=side*3
  box('Side lower',(x,.7,0),(.16,1,5),1);box('Side upper',(x,2.72,0),(.16,.56,5),1)
  for z in [-2.4,-1.4,0,1.4,2.4]:box('Side post',(x,1.82,z),(.17,1.25,.15),0)
  for h in [1.2,2.42]:box('Sill',(x,h,0),(.25,.1,5.1),4)
 # Front has a 1.4m doorway with 2.3m clearance.
 for side in [-1,1]:
  x=side*1.85
  box('Front lower',(x,.7,-2.5),(2.3,1,.16),1)
  box('Front upper',(x,2.72,-2.5),(2.3,.56,.16),1)
  for px in [side*.7,side*3]:box('Front post',(px,1.82,-2.5),(.16,1.25,.18),0)
 box('Door lintel',(0,2.72,-2.5),(1.4,.56,.18),0)
 box('Rear wall',(0,1.6,2.5),(6,2.8,.16),1)
 # Separate pitched sheets; Blender x rotation corresponds to sloped roof across depth.
 slope=math.atan2(1.1,2.75)
 for side in [-1,1]:
  o=box('Roof',(0,3.55,side*1.375),(6.6,.14,math.sqrt(2.75**2+1.1**2)),2)
  o.rotation_euler[0]=side*slope
 if level<2:
  for x in [-2.7,2.7]:
   box('Corner beam',(x,1.6,2.5),(.18,2.8,.2),0)
   box('Porch post',(x,1.55,-3.4),(.14,2.7,.14),0)
  box('Porch floor',(0,.14,-3),(6,.16,1.2),0)
  box('Porch canopy',(0,2.95,-3.1),(6.4,.14,1.7),2)
  box('Station sign',(0,2.64,-3.45),(1.6,.35,.07),4)
  box('Table',(1.6,1.05,.8),(1.6,.12,.8),0)
  for x in [1,2.2]:
   for z in [.55,1.05]:box('Table leg',(x,.64,z),(.1,.7,.1),0)
  box('Shelf',(-2.4,.95,1.5),(.7,.12,1.5),0)
 if level==0:
  for h in [.45,.8,1.15,2.6,2.9]:
   for x in [-3.09,3.09]:box('Weatherboard seam',(x,h,0),(.012,.018,4.9),0,0)
  # Uneven shutters suggest an abandoned station without blocking the doorway.
  for side in [-1,1]:
   for z in [-1.1,.6]:
    o=box('Shutter',(side*3.12,1.78,z),(.08,.85,.45),0);o.rotation_euler[0]=side*.08
  for z in [-1.8,-.9,0,.9,1.8]:box('Floor seam',(0,.285,z),(5.7,.006,.015),0,0)
  for x in [-2.5,-1.5,-.5,.5,1.5,2.5]:
   for side in [-1,1]:
    o=box('Roof rib',(x,3.65,side*1.375),(.035,.04,2.96),2,0);o.rotation_euler[0]=side*slope
 batches=[[o for o in parts if o.data.materials[0]==m] for m in mats];combined=[]
 for m,objects in zip(mats,batches):
  if not objects:continue
  bpy.ops.object.select_all(action='DESELECT')
  for o in objects:o.select_set(True)
  bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();o=bpy.context.object;o.name='LOD%d_%s'%(level,m.name);o.parent=group;combined.append(o)
 stats['LOD'+str(level)]={'meshes':len(combined),'triangles':sum(sum(len(f.vertices)-2 for f in o.data.polygons) for o in combined)}
for name,pos in [('AxisRight',(1,0,0)),('AxisUp',(0,1,0)),('AxisForward',(0,0,1))]:
 o=bpy.data.objects.new(name,None);bpy.context.collection.objects.link(o);o.parent=root;o.location=xyz(pos)
bpy.ops.object.select_all(action='SELECT');bpy.ops.export_scene.fbx(filepath=str(out/'StationHouse.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',bake_anim=False)
for obj in bpy.context.scene.objects:
 if obj.type=='MESH':hidden=obj.parent.name!='LOD0';obj.hide_render=hidden;obj.hide_set(hidden)
bpy.ops.wm.save_as_mainfile(filepath=str(source/'StationHouse.blend'))
(out/'StationHouse_stats.json').write_text(json.dumps(stats,indent=2));print('MODEL_STATS',stats)
if a.render:
 def aim(o,p):o.rotation_euler=(Vector(p)-o.location).to_track_quat('-Z','Y').to_euler()
 ground=material('Ground',(.16,.21,.11));bpy.ops.mesh.primitive_plane_add(size=200);bpy.context.object.data.materials.append(ground)
 bpy.ops.object.light_add(type='AREA',location=(3,6,11));o=bpy.context.object;o.data.energy=2000;o.data.size=8;aim(o,(0,0,1))
 bpy.ops.object.light_add(type='AREA',location=(-6,-3,7));o=bpy.context.object;o.data.energy=1300;o.data.size=7;aim(o,(0,0,1))
 bpy.ops.object.camera_add(location=(11,13,8));o=bpy.context.object;aim(o,(0,0,1.5));o.data.type='ORTHO';o.data.ortho_scale=11
 s=bpy.context.scene;s.camera=o;s.world.color=(.35,.4,.3);s.render.engine='CYCLES';s.cycles.samples=24;s.cycles.use_denoising=False;s.render.resolution_x=1400;s.render.resolution_y=1000;s.render.resolution_percentage=100
 preview=project/'Previews';preview.mkdir(exist_ok=True);s.render.filepath=str(preview/'StationHouse.png');bpy.ops.render.render(write_still=True)

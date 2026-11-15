"""blender -b --python generate_train_car.py -- --project /path/to/unity [--render]\nStandalone summer railcar, 3 LODs, Unity semantic axis markers, no external assets."""
import argparse, sys, math, json
from pathlib import Path
import bpy
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(args)
project=Path(a.project).resolve();out=project/'Assets/Models';out.mkdir(parents=True,exist_ok=True)
source=project/'Tools/Blender/Source';source.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.context.scene.unit_settings.system='METRIC'
def material(name,color,metal=0,rough=.6):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;b=m.node_tree.nodes['Principled BSDF'];b.inputs['Base Color'].default_value=(*color,1);b.inputs['Metallic'].default_value=metal;b.inputs['Roughness'].default_value=rough;return m
mats=[material('TrainPaint',(.14,.22,.18),.5),material('TrainSteel',(.12,.14,.15),.8),material('TrainWood',(.30,.18,.085)),material('TrainSeat',(.18,.105,.055)),material('TrainTrim',(.72,.63,.40),.45),material('TrainGauge',(.025,.04,.035),.2)]
# Author in Unity semantic x/right, y/up, z/forward, converted to Blender.
def xyz(v):return(v[0],-v[2],v[1])
parts=[];level=0
root=bpy.data.objects.new('SummerRailcar',None);bpy.context.collection.objects.link(root)
def box(n,pos,size,mi=0,bevel=.012):
 bpy.ops.mesh.primitive_cube_add(size=1,location=xyz(pos));o=bpy.context.object;o.name=n;o.dimensions=(size[0],size[2],size[1]);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(mats[mi])
 if level==0 and bevel:
  b=o.modifiers.new('Edge bevel','BEVEL');b.width=bevel;b.segments=2;bpy.ops.object.modifier_apply(modifier=b.name)
  b=o.modifiers.new('Weighted normals','WEIGHTED_NORMAL');bpy.ops.object.modifier_apply(modifier=b.name)
 parts.append(o);return o
def cylinder(n,pos,r,depth,mi=1,axis='x'):
 bpy.ops.mesh.primitive_cylinder_add(vertices=20 if level==0 else 12,radius=r,depth=depth,location=xyz(pos));o=bpy.context.object;o.name=n
 if axis=='x':o.rotation_euler[1]=math.pi/2
 if axis=='z':o.rotation_euler[0]=math.pi/2
 o.data.materials.append(mats[mi]);parts.append(o);return o
counts={}
for level in range(3):
 parts=[];group=bpy.data.objects.new('LOD'+str(level),None);bpy.context.collection.objects.link(group);group.parent=root
 box('Floor',(0,1,0),(3,.3,12),2)
 box('Underframe',(0,.72,0),(2.6,.3,11.8),1)
 # Roof ribs approximate an arched roof without hiding window apertures.
 for x in [-1.28,-.64,0,.64,1.28]:box('Roof panel',(x,4.02-abs(x)*.12,0),(.66,.16,12.2),0)
 for side in [-1,1]:
  x=side*1.5
  for lo,hi in ([(-6,6)] if side<0 else [(-6,-5.6),(-3.4,6)]):
   mid=(lo+hi)/2;span=hi-lo
   box('Lower wall',(x,1.65,mid),(.12,1,span),0)
   box('Upper wall',(x,3.65,mid),(.12,.55,span),0)
   for h in [2.18,3.34]:box('Window rail',(x,h,mid),(.17,.08,span),4)
   # Window openings remain empty; no expensive alpha glass in this prototype.
   for z in [lo+i*1.8 for i in range(int(span/1.8)+1)]+[hi]:box('Window pillar',(x,2.75,z),(.15,1.2,.10),0)
  box('Lower stripe',(x*1.006,1.73,0 if side<0 else 1.3),(.025,.12,12 if side<0 else 9.4),4,0)
 for end in [-1,1]:
  z=end*6
  box('End lower wall',(0,1.7,z),(3,1.1,.12),0)
  box('End upper wall',(0,3.65,z),(3,.6,.12),0)
  for x in [-1.43,0,1.43]:box('End window pillar',(x,2.8,z),(.12,1.1,.14),0)
  box('Coupler',(0,.68,end*6.5),(.3,.22,.9),1)
 for bogie in [-3.8,3.8]:
  box('Bogie frame',(0,.72,bogie),(2.65,.25,2.1),1)
  for z in [bogie-.65,bogie+.65]:
   cylinder('Axle',(0,.68,z),.10,1.9)
   for x in [-.8,.8]:
    cylinder('Wheel',(x,.68,z),.38,.17)
    if level==0:cylinder('Hub',(x*1.15,.68,z),.13,.12,4)
 if level<2:
  for x in [-.74,.74]:box('Frame beam',(x,.7,0),(.12,.25,11.6),1)
  for i in range(4):box('Boarding step',(2.7-i*.3,.15+i*.25,-4.5),(.4,.3,2),1)
  # Interior keeps a 0.9m aisle and clear rear entrance.
  for z in [-1.7,.1,1.9]:
   for side in [-1,1]:
    box('Bench seat',(side*1.04,1.65,z),(.66,.15,1.4),3)
    box('Bench back',(side*1.34,2,z),(.12,.7,1.4),3)
    for dz in [-.45,.45]:box('Bench leg',(side*1.04,1.35,z+dz),(.10,.4,.10),1)
  box('Cab desk',(0,1.6,4.5),(1.8,.8,.6),1)
  box('Control face',(0,2.04,4.5),(1.8,.07,.64),0)
  for x in [-.5,0,.5]:
   cylinder('Gauge',(x,2.09,4.55),.105,.025,5,'y')
   if level==0:box('Gauge needle',(x,2.106,4.55),(.012,.008,.13),4,.002)
  for side in [-1,1]:
   cylinder('Door handrail',(side*0+1.7,2.08,-4.5+side*.94),.028,1.5,4,'y')
 if level==0:
  for z in range(-5,6):
   box('Floor board seam',(0,1.153,z),(2.82,.006,.018),1,0)
   for x in [-1.55,1.55]:
    # No rivets in the doorway.
    if x<0 or not -5.6<z<-3.4:
     box('Panel seam',(x,1.55,z),(.012,.78,.018),1,0)
  for z in [-2,2]:box('Ceiling light',(0,3.88,z),(.22,.06,.8),4)
 # Merge by material: at most six draw meshes per LOD, modifiers already applied.
 combined=[]
 batches=[[o for o in parts if o.data.materials[0]==m] for m in mats]
 for mi,m in enumerate(mats):
  objects=batches[mi]
  if not objects:continue
  bpy.ops.object.select_all(action='DESELECT')
  for o in objects:o.select_set(True)
  bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();o=bpy.context.object;o.name='LOD%d_%s'%(level,m.name);o.parent=group;combined.append(o)
 counts['LOD'+str(level)]={'meshes':len(combined),'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in combined)}
 group.hide_render=level!=0
for n,p in [('AxisRight',(1,0,0)),('AxisUp',(0,1,0)),('AxisForward',(0,0,1))]:
 o=bpy.data.objects.new(n,None);bpy.context.collection.objects.link(o);o.location=xyz(p);o.parent=root
bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.fbx(filepath=str(out/'SummerRailcar.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',bake_anim=False,use_mesh_modifiers=True)
for obj in bpy.context.scene.objects:
 if obj.type=='MESH' and obj.parent and obj.parent.name.startswith('LOD'):
  hidden=obj.parent.name!='LOD0';obj.hide_render=hidden;obj.hide_set(hidden)
bpy.ops.wm.save_as_mainfile(filepath=str(source/'SummerRailcar.blend'))
(out/'SummerRailcar_stats.json').write_text(json.dumps(counts,indent=2))
print('MODEL_STATS',counts)
if a.render:
 def aim(o,point):o.rotation_euler=(Vector(point)-o.location).to_track_quat('-Z','Y').to_euler()
 floor=material('StudioGround',(.11,.14,.12))
 bpy.ops.mesh.primitive_plane_add(size=200);bpy.context.object.data.materials.append(floor)
 for loc,power,size in [((7,3,12),2400,10),((-6,-4,9),1800,9)]:
  bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.data.energy=power;o.data.size=size;aim(o,(0,0,1.8))
 bpy.ops.object.camera_add(location=(15,18,11));cam=bpy.context.object;aim(cam,(0,0,1.8));cam.data.type='ORTHO';cam.data.ortho_scale=17
 s=bpy.context.scene;s.camera=cam;s.world.color=(.3,.3,.3);s.render.engine='CYCLES';s.cycles.samples=20;s.cycles.use_denoising=False;s.render.resolution_x=1400;s.render.resolution_y=900;s.render.resolution_percentage=100
 preview=project/'Previews';preview.mkdir(exist_ok=True)
 s.render.filepath=str(preview/'Railcar_Exterior.png');bpy.ops.render.render(write_still=True)
 # Real cutaway of the same model: hide roof polygons and a near wall via mesh editing on a copy.
 for obj in list(bpy.context.scene.objects):
  if obj.type=='MESH' and obj.parent and obj.parent.name=='LOD0':
   import bmesh
   bm=bmesh.new();bm.from_mesh(obj.data)
   # Local coordinates include joined-object offset; use world center to choose surfaces.
   remove=[]
   for f in bm.faces:
    c=obj.matrix_world@f.calc_center_median()
    if c.z>3.72 or (c.x>1.40 and c.z>1.2):remove.append(f)
   bmesh.ops.delete(bm,geom=remove,context='FACES');bm.to_mesh(obj.data);bm.free()
 cam.location=(13,12,13);aim(cam,(0,0,1.4));cam.data.ortho_scale=16
 s.render.filepath=str(preview/'Railcar_Interior.png');bpy.ops.render.render(write_still=True)

"""blender -b --python render_inventory_layout.py -- --project /UnityProject
Blender layout preview from the exported game meshes, not a Unity screenshot.
"""
import bpy,sys,argparse,math
from pathlib import Path
from mathutils import Vector
p=argparse.ArgumentParser();p.add_argument('--project',required=True);a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []);project=Path(a.project).resolve()
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def material(name,color):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;n=m.node_tree.nodes['Principled BSDF'];n.inputs['Base Color'].default_value=(*color,1);n.inputs['Roughness'].default_value=.9;return m
fabric=material('Inventory fabric',(.19,.14,.09));metal=material('Metal parts',(.22,.26,.23))
vertices=[];faces=[]
for z in range(13):
 for x in range(17):vertices.append(((x/16-.5)*2,-(z/12-.5)*1.65,math.sin(x*1.9+z*.8)*.004))
for z in range(12):
 for x in range(16):i=z*17+x;faces.append((i,i+17,i+18,i+1))
mesh=bpy.data.meshes.new('Cloth mesh');mesh.from_pydata(vertices,[],faces);mesh.update();obj=bpy.data.objects.new('Inventory cloth',mesh);bpy.context.collection.objects.link(obj);obj.data.materials.append(fabric)
def asset(name,pos,rotation=None,scale=1):
 old=set(bpy.context.scene.objects);bpy.ops.import_scene.fbx(filepath=str(project/'Assets/Models'/(name+'.fbx')));imported=[o for o in bpy.context.scene.objects if o not in old]
 root=next(o for o in imported if o.parent is None);root.location=(pos[0],-pos[2],pos[1]);root.scale*=scale
 if rotation:root.rotation_euler=rotation
 for o in imported:
  if o.type=='MESH' and not o.name.startswith('LOD0_'):o.hide_render=True
asset('WaterBottle',(-.72,.01,.42));asset('CannedFood',(-.36,.01,.42));asset('FieldMedkit',(0,.01,.42));asset('RepairKit',(.36,.01,.42));asset('AmmoBox',(.72,.01,.42))
asset('SurvivalAxe',(0,.055,-.66),(math.pi/2,0,0));asset('ServicePistol',(.70,.04,-.38),(0,-math.pi/2,0));asset('FieldBackpack',(1,.03,.36),scale=.45)
for name,pos,size,mat in [('Parts',(-.4,.05,-.32),(.16,.05,.10),metal),('Folded fabric',(-.76,.045,-.32),(.22,.045,.20),fabric)]:
 bpy.ops.mesh.primitive_cube_add(size=1,location=(pos[0],-pos[2],pos[1]));o=bpy.context.object;o.name=name;o.dimensions=(size[0],size[2],size[1]);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(mat)
ground=material('Studio ground',(.09,.11,.09));bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.02));bpy.context.object.data.materials.append(ground)
def aim(o,target):o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler()
for loc,power,size in [((2,3,5),500,4),((-3,-2,4),400,4)]:
 bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.data.energy=power;o.data.size=size;aim(o,(0,0,0))
bpy.ops.object.camera_add(location=(.5,2.8,4));o=bpy.context.object;aim(o,(.08,0,.05));o.data.type='ORTHO';o.data.ortho_scale=2.9
s=bpy.context.scene;s.camera=o;s.world.color=(.3,.3,.3);s.render.engine='CYCLES';s.cycles.samples=24;s.cycles.use_denoising=False;s.render.resolution_x=1500;s.render.resolution_y=1100;s.render.resolution_percentage=100
s.render.filepath=str(project/'Previews/InventoryClothLayout.png');bpy.ops.render.render(write_still=True)

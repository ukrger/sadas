"""Actual exported weapon meshes in the same sight pose as WeaponPresentation.SightPose.
These are Blender alignment previews, NOT Unity screenshots or tests of runtime transitions.
"""
import bpy,math,sys,argparse
from pathlib import Path
from mathutils import Vector,Matrix
from bpy_extras.object_utils import world_to_camera_view
p=argparse.ArgumentParser();p.add_argument('--project',required=True);a=p.parse_args(sys.argv[sys.argv.index('--')+1:]);project=Path(a.project)
def xyz(p):return Vector((p[0],-p[2],p[1]))
def load(name,offset,right_only=False):
 before=set(bpy.context.scene.objects);bpy.ops.import_scene.fbx(filepath=str(project/'Assets/Models'/(name+'.fbx')))
 objects=[o for o in bpy.context.scene.objects if o not in before];meshes=[]
 for o in objects:
  if o.type=='MESH' and o.name.startswith('LOD0_') and not (right_only and 'HandLeft' in o.name):
   transform=o.matrix_world.copy();o.parent=None;o.matrix_world=Matrix.Translation(xyz(offset))@transform;meshes.append(o)
  else:bpy.data.objects.remove(o,do_unlink=True)
 return meshes
for model,marker,label in [('ServicePistol',(0,.226,-.035),'PistolAim'),('PumpShotgun',(0,.2075,.08),'ShotgunAim'),('AutomaticRifle',(0,.2465,.19),'RifleAim')]:
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 position=(0,-marker[1],.23-marker[2]);meshes=load(model,position)
 hands=list(position)
 if model=='ServicePistol':hands[1]+=.07;hands[2]-=.08
 load('PlayerHands',hands,model=='ServicePistol')
 for o in meshes:
  for m in o.data.materials:
   if m is None or not m.use_nodes:continue
   style='Wood' if 'Wood' in m.name else ('Steel' if 'Steel' in m.name else None)
   if style:
    node=m.node_tree.nodes.new('ShaderNodeTexImage');node.image=bpy.data.images.load(str(project/'Assets/TrainSummer/Textures/Procedural'/(style+'_Albedo.png')))
    multiply=m.node_tree.nodes.new('ShaderNodeMixRGB');multiply.blend_type='MULTIPLY';multiply.inputs[0].default_value=1;m.node_tree.links.new(node.outputs['Color'],multiply.inputs[1]);multiply.inputs[2].default_value=m.diffuse_color;m.node_tree.links.new(multiply.outputs['Color'],m.node_tree.nodes['Principled BSDF'].inputs['Base Color'])
 bpy.ops.mesh.primitive_plane_add(size=160,location=(0,-30,-1.65));ground=bpy.context.object
 mat=bpy.data.materials.new('Summer grass');mat.diffuse_color=(.18,.26,.08,1);ground.data.materials.append(mat)
 bpy.ops.mesh.primitive_cube_add(size=1,location=(0,-25,-.05));board=bpy.context.object;board.dimensions=(.9,.08,1.2);wood=bpy.data.materials.new('Target board');wood.diffuse_color=(.36,.29,.16,1);board.data.materials.append(wood)
 # A red aim point centered exactly on the camera sight line.
 bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=8,radius=.08,location=(0,-24.94,0));bpy.context.object.scale=(1,.05,1);red=bpy.data.materials.new('Target dot');red.diffuse_color=(.55,.03,.015,1);bpy.context.object.data.materials.append(red)
 bpy.ops.object.light_add(type='SUN',location=(0,0,10));sun=bpy.context.object;sun.rotation_euler=(.4,-.5,-.6);sun.data.energy=2.5;sun.data.angle=.1
 bpy.ops.object.light_add(type='AREA',location=(1,0,2));bpy.context.object.data.energy=80;bpy.context.object.data.size=3
 bpy.ops.object.camera_add(location=(0,0,0));cam=bpy.context.object;cam.rotation_euler=Vector((0,-1,0)).to_track_quat('-Z','Y').to_euler();cam.data.sensor_fit='VERTICAL';cam.data.sensor_height=24;cam.data.lens=24/(2*math.tan(math.radians(58)/2));cam.data.clip_start=.03
 scene=bpy.context.scene;scene.camera=cam;scene.world.color=(.35,.45,.6);scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=False;scene.render.resolution_x=1400;scene.render.resolution_y=788;scene.render.resolution_percentage=100
 bpy.context.view_layer.update()
 view=world_to_camera_view(scene,cam,xyz((0,0,.23)));assert abs(view.x-.5)<1e-5 and abs(view.y-.5)<1e-5 and abs(view.z-.23)<1e-5
 front_z={'ServicePistol':.127,'PumpShotgun':.79,'AutomaticRifle':.70}[model]
 front=world_to_camera_view(scene,cam,xyz((position[0],position[1]+marker[1],position[2]+front_z)));assert abs(front.x-.5)<1e-5 and abs(front.y-.5)<1e-5 and front.z>view.z
 scene.render.filepath=str(project/'Previews'/(label+'.png'));bpy.ops.render.render(write_still=True);print('AIM ALIGNMENT PASS',model,'screen center',tuple(view))

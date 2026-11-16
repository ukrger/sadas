"""blender -b --python generate_mutant.py -- --project /UnityProject [--render]
Generic rig, rigid segment weights, a baked walk cycle. No external assets.
"""
import sys,argparse,math,bpy
from pathlib import Path
from mathutils import Vector
sys.path.insert(0,str(Path(__file__).resolve().parent))
from prop_pipeline import PropBuilder
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
b=PropBuilder(a.project,'ForestMutant',[('MutantSkin',(.30,.34,.22),0),('MutantCloth',(.18,.20,.14),0),('MutantDark',(.07,.08,.045),0)])
def xyz(v):return Vector((v[0],-v[2],v[1]))
bones={
 'Hips':((0,.85,0),(0,1.05,0),None),'Spine':((0,1.05,0),(0,1.40,.02),'Hips'),'Head':((0,1.40,.02),(0,1.76,.04),'Spine')}
for side,label in [(-1,'L'),(1,'R')]:
 bones['UpperArm'+label]=((side*.22,1.36,0),(side*.36,1.10,.03),'Spine')
 bones['Forearm'+label]=((side*.36,1.10,.03),(side*.42,.84,.09),'UpperArm'+label)
 bones['Thigh'+label]=((side*.12,.88,0),(side*.14,.48,.015),'Hips')
 bones['Shin'+label]=((side*.14,.48,.015),(side*.15,.09,.07),'Thigh'+label)
rigdata=bpy.data.armatures.new('MutantSkeleton');rig=bpy.data.objects.new('MutantRig',rigdata);bpy.context.collection.objects.link(rig);rig.parent=b.root;bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
for name,(head,tail,parent) in bones.items():
 bone=rigdata.edit_bones.new(name);bone.head=xyz(head);bone.tail=xyz(tail)
 if parent:bone.parent=rigdata.edit_bones[parent]
bpy.ops.object.mode_set(mode='OBJECT');rig.select_set(False)
def bind(obj,bone):
 group=obj.vertex_groups.new(name=bone);group.add(list(range(len(obj.data.vertices))),1,'REPLACE');mod=obj.modifiers.new('Skeleton','ARMATURE');mod.object=rig
 return obj
for level in range(3):
 b.begin(level)
 def sphere(name,pos,scale,mi,bone,direction=None):
  bpy.ops.mesh.primitive_uv_sphere_add(segments=[16,10,6][level],ring_count=[10,6,4][level],location=xyz(pos));o=bpy.context.object;o.name=name;o.scale=scale
  if direction is not None:o.rotation_euler=xyz(direction).to_track_quat('Z','Y').to_euler()
  bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(b.mats[mi]);b.parts.append(o);bind(o,bone)
  for f in o.data.polygons:f.use_smooth=True
 sphere('Pelvis',(0,.89,0),(.20,.13,.16),1,'Hips')
 sphere('Torso',(0,1.20,.01),(.25,.14,.30),1,'Spine')
 sphere('Head',(0,1.62,.04),(.115,.105,.16),0,'Head')
 for side,label in [(-1,'L'),(1,'R')]:
  for segment,mi,width in [('UpperArm',0,.080),('Forearm',0,.063),('Thigh',1,.095),('Shin',1,.070)]:
   head,tail,_=bones[segment+label];h,t=Vector(head),Vector(tail);center=(h+t)/2
   sphere(segment+label,center,(width,width,(t-h).length*.59),mi,segment+label,t-h)
  sphere('Hand'+label,(side*.43,.80,.10),(.065,.055,.09),0,'Forearm'+label)
  sphere('Boot'+label,(side*.15,.06,.13),(.09,.15,.06),2,'Shin'+label)
  if level<2:
   eye=b.box('Eye',(side*.045,1.655,.135),(.025,.018,.018),2,.002);bind(eye,'Head')
 if level==0:
  for side in [-1,1]:bind(b.box('Torn cuff',(side*.20,1.32,.015),(.08,.17,.13),1,.012),'Spine')
 if level<2:
  bind(b.box('Nose',(0,1.625,.158),(.035,.052,.055),0,.006),'Head')
  bind(b.box('Mouth',(0,1.565,.142),(.07,.016,.015),2,.002),'Head')
  bind(b.box('Brow',(0,1.69,.135),(.105,.025,.03),0,.005),'Head')
  bind(b.box('Jacket opening',(0,1.20,.151),(.025,.31,.014),2,.002),'Spine')
 b.finish()
# Looping walk. Neutral pose remains separate from the action.
rig.animation_data_create();action=bpy.data.actions.new('Walk');rig.animation_data.action=action
for frame in [1,7,13,19,25]:
 phase=(frame-1)/24*2*math.pi
 for side,label in [(-1,'L'),(1,'R')]:
  for prefix,amount in [('Thigh',.45),('Shin',.18),('UpperArm',-.30),('Forearm',-.12)]:
   bone=rig.pose.bones[prefix+label];bone.rotation_mode='XYZ';bone.rotation_euler.x=side*math.sin(phase)*amount;bone.keyframe_insert(data_path='rotation_euler',frame=frame)
for fc in action.fcurves:
 for key in fc.keyframe_points:key.interpolation='LINEAR'
idle=bpy.data.actions.new('Idle');rig.animation_data.action=idle
for frame in [1,25]:
 for bone in rig.pose.bones:
  bone.rotation_euler=(0,0,0);bone.keyframe_insert(data_path='rotation_euler',frame=frame)
rig.animation_data.action=action
bpy.context.scene.frame_start=1;bpy.context.scene.frame_end=25;bpy.context.scene.frame_set(1)
b.export(animated=True)
if a.render:
 rig.animation_data.action=None
 for bone in rig.pose.bones:bone.rotation_euler=(0,0,0)
 b.render(.9,3.0,camera=(3,-4,2.8))

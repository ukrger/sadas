"""Shared deterministic geometry/export helpers. Blender only, no third-party assets."""
import bpy,json,math
from pathlib import Path
from mathutils import Vector
class PropBuilder:
 def __init__(self,project,name,palette):
  self.project=Path(project).resolve();self.name=name;self.level=0;self.parts=[];self.stats={}
  bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
  bpy.context.scene.unit_settings.system='METRIC'
  self.root=bpy.data.objects.new(name,None);bpy.context.collection.objects.link(self.root);self.mats=[]
  for name,color,metal in palette:
   m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;n=m.node_tree.nodes['Principled BSDF'];n.inputs['Base Color'].default_value=(*color,1);n.inputs['Metallic'].default_value=metal;n.inputs['Roughness'].default_value=.65;self.mats.append(m)
 def begin(self,level):
  self.level=level;self.parts=[];self.group=bpy.data.objects.new('LOD'+str(level),None);bpy.context.collection.objects.link(self.group);self.group.parent=self.root
 def finish(self):
  batches=[[o for o in self.parts if o.data.materials[0]==m] for m in self.mats];combined=[]
  for m,objects in zip(self.mats,batches):
   if not objects:continue
   bpy.ops.object.select_all(action='DESELECT')
   for o in objects:o.select_set(True)
   bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();o=bpy.context.object;o.name='LOD%d_%s'%(self.level,m.name);o.parent=self.group;combined.append(o)
  self.stats['LOD'+str(self.level)]={'meshes':len(combined),'triangles':sum(sum(len(f.vertices)-2 for f in o.data.polygons) for o in combined)}
 def box(self,name,p,size,mi=0,bevel=.01):
  bpy.ops.mesh.primitive_cube_add(size=1,location=(p[0],-p[2],p[1]));o=bpy.context.object;o.name=name;o.dimensions=(size[0],size[2],size[1]);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(self.mats[mi])
  if self.level==0 and bevel:
   b=o.modifiers.new('Bevel','BEVEL');b.width=bevel;b.segments=2;bpy.ops.object.modifier_apply(modifier=b.name)
   b=o.modifiers.new('Normals','WEIGHTED_NORMAL');bpy.ops.object.modifier_apply(modifier=b.name)
  self.parts.append(o);return o
 def cylinder(self,name,p,r,depth,mi=1,axis='y'):
  bpy.ops.mesh.primitive_cylinder_add(vertices=16 if self.level==0 else 8,radius=r,depth=depth,location=(p[0],-p[2],p[1]));o=bpy.context.object;o.name=name
  if axis=='x':o.rotation_euler[1]=math.pi/2
  if axis=='z':o.rotation_euler[0]=math.pi/2
  o.data.materials.append(self.mats[mi]);self.parts.append(o);return o
 def export(self,animated=False):
  for name,p in [('AxisRight',(1,0,0)),('AxisUp',(0,1,0)),('AxisForward',(0,0,1))]:
   o=bpy.data.objects.new(name,None);bpy.context.collection.objects.link(o);o.parent=self.root;o.location=(p[0],-p[2],p[1])
  out=self.project/'Assets/Models';out.mkdir(parents=True,exist_ok=True)
  bpy.ops.object.select_all(action='SELECT');bpy.ops.export_scene.fbx(filepath=str(out/(self.name+'.fbx')),use_selection=True,axis_forward='-Z',axis_up='Y',bake_anim=animated,add_leaf_bones=False)
  for o in bpy.context.scene.objects:
   if o.type=='MESH':hidden=o.parent.name!='LOD0';o.hide_render=hidden;o.hide_set(hidden)
  source=self.project/'Tools/Blender/Source';source.mkdir(parents=True,exist_ok=True);bpy.ops.wm.save_as_mainfile(filepath=str(source/(self.name+'.blend')))
  (out/(self.name+'_stats.json')).write_text(json.dumps(self.stats,indent=2));print('MODEL_STATS',self.name,self.stats)
 def render(self,target,ortho,camera=(3,4,2.8)):
  def aim(o,p):o.rotation_euler=(Vector(p)-o.location).to_track_quat('-Z','Y').to_euler()
  bpy.ops.mesh.primitive_plane_add(size=100);m=bpy.data.materials.new('Studio');m.diffuse_color=(.13,.15,.13,1);bpy.context.object.data.materials.append(m)
  for loc,power,size in [((3,4,6),450,5),((-3,-2,4),350,4)]:
   bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.data.energy=power;o.data.size=size;aim(o,(0,0,target))
  bpy.ops.object.camera_add(location=camera);cam=bpy.context.object;aim(cam,(0,0,target));cam.data.type='ORTHO';cam.data.ortho_scale=ortho
  s=bpy.context.scene;s.camera=cam;s.world.color=(.3,.3,.3);s.render.engine='CYCLES';s.cycles.samples=24;s.cycles.use_denoising=False;s.render.resolution_x=1300;s.render.resolution_y=1000;s.render.resolution_percentage=100
  out=self.project/'Previews';out.mkdir(exist_ok=True);s.render.filepath=str(out/(self.name+'.png'));bpy.ops.render.render(write_still=True)

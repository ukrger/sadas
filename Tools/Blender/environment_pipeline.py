"""Shared meshes for the environment batch. Uses absolute coordinates and vertex wind masks."""
import bpy,math,random
from mathutils import Vector
from prop_pipeline import PropBuilder
PALETTE=[('EnvBark',(.25,.18,.105),0),('EnvLeafBroad',(.20,.38,.075),0),('EnvLeafPine',(.10,.25,.085),0),('EnvPlaster',(.52,.46,.34),0),('EnvBrick',(.32,.19,.12),0),('EnvRoof',(.22,.20,.17),.45),('EnvIvy',(.16,.30,.06),0)]
def xyz(p):return Vector((p[0],-p[2],p[1]))
def mesh_part(builder,name,vertices,faces,material,colors=None):
 mesh=bpy.data.meshes.new(name);mesh.from_pydata([xyz(p) for p in vertices],[],faces);mesh.update();obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);obj.data.materials.append(builder.mats[material]);builder.parts.append(obj)
 attr=mesh.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='POINT')
 for i,v in enumerate(attr.data):v.color=colors[i] if colors else (1,1,1,0)
 for face in mesh.polygons:face.use_smooth=material==0
 return obj

def branch(builder,start,end,radius,end_radius,segments=8,material=0):
 a,b=Vector(start),Vector(end);axis=(b-a).normalized();right=axis.cross(Vector((0,0,1)))
 if right.length<.01:right=axis.cross(Vector((1,0,0)))
 right.normalize();forward=axis.cross(right).normalized();vertices=[]
 for center,r in [(a,radius),(b,end_radius)]:
  for i in range(segments):vertices.append(tuple(center+(right*math.cos(i*math.tau/segments)+forward*math.sin(i*math.tau/segments))*r))
 faces=[tuple(range(segments-1,-1,-1)),tuple(range(segments,segments*2))]
 for i in range(segments):j=(i+1)%segments;faces.append((i,j,j+segments,i+segments))
 return mesh_part(builder,'Branch',vertices,faces,material)

def leaf_cloud(builder,centers,count,size,seed,material=1,needle=False):
 rng=random.Random(seed);vertices=[];faces=[];colors=[]
 for i in range(count):
  center,extent=centers[i%len(centers)];theta=rng.random()*math.tau;z=rng.uniform(-1,1);rad=rng.random()**(1/3);horizontal=math.sqrt(1-z*z)
  point=Vector(center)+Vector((math.cos(theta)*horizontal*extent[0],z*extent[1],math.sin(theta)*horizontal*extent[2]))*rad
  angle=rng.random()*math.tau;right=Vector((math.cos(angle),rng.uniform(-.4,.4),math.sin(angle))).normalized();up=Vector((-right.z,rng.uniform(.3,1),right.x)).normalized();length=size*rng.uniform(.65,1.3);width=length*.45
  off=len(vertices)
  if needle:
   width=length*.06;points=[point-up*length*.5-right*width,point-up*length*.5+right*width,point+up*length*.5];faces.append((off,off+1,off+2))
  else:
   points=[point-up*length*.5,point-up*length*.2-right*width,point+up*length*.25-right*width*.8,point+up*length*.5,point+up*length*.25+right*width*.8,point-up*length*.2+right*width]
   faces.extend([(off,off+1,off+2),(off,off+2,off+3),(off,off+3,off+4),(off,off+4,off+5)])
  vertices.extend([tuple(p) for p in points]);tint=rng.uniform(.68,1.12);colors.extend([(tint*.9,tint,tint*.85,1)]*len(points))
 return mesh_part(builder,'Foliage',vertices,faces,material,colors)

def distant_crown(builder,centers,material):
 for center,extent in centers:
  bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=xyz(center));obj=bpy.context.object;obj.name='Crown silhouette';obj.scale=(extent[0],extent[2],extent[1]);bpy.ops.object.transform_apply(location=True,rotation=True,scale=True);obj.data.materials.append(builder.mats[material]);builder.parts.append(obj)
  attr=obj.data.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='POINT')
  for item in attr.data:item.color=(1,1,1,1)

def outdoor_render(builder,target,ortho,camera):
 def aim(o,p):o.rotation_euler=(Vector(p)-o.location).to_track_quat('-Z','Y').to_euler()
 mat=bpy.data.materials.new('Summer ground');mat.diffuse_color=(.19,.26,.08,1);mat.use_nodes=True;n=mat.node_tree.nodes['Principled BSDF'];n.inputs['Base Color'].default_value=mat.diffuse_color;n.inputs['Roughness'].default_value=.95
 bpy.ops.mesh.primitive_plane_add(size=200);bpy.context.object.data.materials.append(mat)
 bpy.ops.object.light_add(type='SUN',location=(0,0,15));sun=bpy.context.object;sun.rotation_euler=(.45,-.5,-.45);sun.data.energy=2.5;sun.data.angle=.12
 bpy.ops.object.light_add(type='AREA',location=(-8,4,12));fill=bpy.context.object;fill.data.energy=1800;fill.data.size=12;aim(fill,(0,0,target))
 bpy.ops.object.camera_add(location=camera);o=bpy.context.object;aim(o,(0,0,target));o.data.type='ORTHO';o.data.ortho_scale=ortho
 s=bpy.context.scene;s.camera=o;s.world.color=(.4,.5,.6);s.render.engine='CYCLES';s.cycles.samples=32;s.cycles.use_denoising=False;s.render.resolution_x=1300;s.render.resolution_y=1100;s.render.resolution_percentage=100
 output=builder.project/'Previews';output.mkdir(exist_ok=True);s.render.filepath=str(output/(builder.name+'.png'));bpy.ops.render.render(write_still=True)

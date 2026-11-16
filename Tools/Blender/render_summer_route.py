"""blender -b --python render_summer_route.py -- --project /UnityProject
Assembly preview from actual exported models, not a Unity screenshot.
"""
import bpy,sys,argparse,math,bisect
from pathlib import Path
from mathutils import Vector,Matrix
p=argparse.ArgumentParser();p.add_argument('--project',required=True);a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []);project=Path(a.project).resolve()
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def material(name,c,metal=0):
 m=bpy.data.materials.new(name);m.diffuse_color=(*c,1);m.use_nodes=True;n=m.node_tree.nodes['Principled BSDF'];n.inputs['Base Color'].default_value=(*c,1);n.inputs['Metallic'].default_value=metal;n.inputs['Roughness'].default_value=.8;return m
cache={}
for name in ['SummerBirch','SummerPine','OvergrownHouse','SummerRailcar']:
 before=set(bpy.context.scene.objects);bpy.ops.import_scene.fbx(filepath=str(project/'Assets/Models'/(name+'.fbx')));objects=[o for o in bpy.context.scene.objects if o not in before]
 cache[name]=[(o.data,list(o.data.materials),o.matrix_world.copy()) for o in objects if o.type=='MESH' and o.name.startswith('LOD0_')]
 for o in objects:bpy.data.objects.remove(o,do_unlink=True)
def instance(name,x,y,z,yaw=0,scale=1):
 transform=Matrix.Translation(Vector((x,-z,y)))@Matrix.Rotation(yaw,4,'Z')@Matrix.Scale(scale,4)
 for mesh,mats,local in cache[name]:
  obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);obj.matrix_world=transform@local
points=[(24*(1-math.cos(z/150)),z) for z in range(0,2201,2)];lengths=[0]
for left,right in zip(points,points[1:]):lengths.append(lengths[-1]+math.dist(left,right))
def pose(distance):
 i=max(0,min(len(points)-2,bisect.bisect_right(lengths,distance)-1));t=(distance-lengths[i])/(lengths[i+1]-lengths[i]);x=points[i][0]*(1-t)+points[i+1][0]*t;z=points[i][1]*(1-t)+points[i+1][1]*t;return x,z,math.atan2(24/150*math.sin(z/150),1)
def mix(v):
 v&=0xffffffff;v^=v>>16;v=v*0x7feb352d&0xffffffff;v^=v>>15;v=v*0x846ca68b&0xffffffff;return (v^(v>>16))&0xffffffff
def unit(tile,slot,salt):return (mix((tile*73856093&0xffffffff)^(slot*19349663&0xffffffff)^salt)&0xffffff)/16777216
stops=[160,640,1200,1800]
for tile in [3,4,5]:
 for i in range(28):
  s=tile*80+unit(tile,i,1)*80;sign=-1 if i%2==0 else 1;offset=sign*(14+unit(tile,i,2)*52)
  if sign>0 and offset<27 and any(abs(s-stop)<32 for stop in stops):offset=32+unit(tile,i,3)*30
  ruin_offset=36 if unit(tile,0,51)>.5 else -36
  if tile==4 and abs(s-360)<8 and abs(offset-ruin_offset)<8:offset=sign*(abs(ruin_offset)+11+unit(tile,i,7)*10)
  x,z,yaw=pose(s);instance('SummerPine' if unit(tile,i,6)>.48 else 'SummerBirch',x+math.cos(yaw)*offset,0,z-math.sin(yaw)*offset,yaw+unit(tile,i,4)*math.tau,.82+unit(tile,i,5)*.36)
x,z,yaw=pose(360);offset=36 if unit(4,0,51)>.5 else -36;instance('OvergrownHouse',x+math.cos(yaw)*offset,0,z-math.sin(yaw)*offset,yaw+(math.pi/2 if offset>0 else -math.pi/2))
x,z,yaw=pose(338);instance('SummerRailcar',x,0,z,yaw)
grass=material('Grass',(.19,.28,.07));wood=material('Sleeper',(.22,.14,.08));steel=material('Rails',(.2,.23,.22),.7)
def box(pos,size,mat,yaw=0):
 bpy.ops.mesh.primitive_cube_add(size=1,location=(pos[0],-pos[2],pos[1]));obj=bpy.context.object;obj.dimensions=(size[0],size[2],size[1]);obj.rotation_euler.z=yaw;obj.data.materials.append(mat)
centerX,centerZ,_=pose(360);box((centerX,-.2,centerZ),(240,.4,260),grass)
for distance in range(240,481,2):
 x,z,yaw=pose(distance);box((x,.08,z),(2.8,.16,.25),wood,yaw)
 nx,nz,_=pose(distance+2)
 for side in [-.8,.8]:box(((x+nx)/2+math.cos(yaw)*side,.22,(z+nz)/2-math.sin(yaw)*side),(.12,.16,2.04),steel,yaw)
def aim(o,p):o.rotation_euler=(Vector(p)-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.light_add(type='SUN',location=(0,0,15));sun=bpy.context.object;sun.rotation_euler=(.4,-.5,-.6);sun.data.energy=2.7;sun.data.angle=.1
bpy.ops.object.light_add(type='AREA',location=(centerX,-centerZ,35));bpy.context.object.data.energy=2500;bpy.context.object.data.size=35
bpy.ops.object.camera_add(location=(centerX+52,-(centerZ-42),16));cam=bpy.context.object;aim(cam,(centerX+8,-(centerZ-4),3));cam.data.lens=30
s=bpy.context.scene;s.camera=cam;s.world.use_nodes=True;sky=s.world.node_tree.nodes.new('ShaderNodeTexSky');sky.sky_type='NISHITA';sky.sun_elevation=.65;sky.sun_rotation=.8;sky.sun_disc=False;sky.air_density=.7;sky.dust_density=.4;background=s.world.node_tree.nodes.get('Background');s.world.node_tree.links.new(sky.outputs['Color'],background.inputs['Color']);background.inputs['Strength'].default_value=.08;s.render.engine='CYCLES';s.cycles.samples=48;s.cycles.use_denoising=False;s.render.resolution_x=1500;s.render.resolution_y=1000;s.render.resolution_percentage=100;s.render.filepath=str(project/'Previews/SummerRouteEnvironment.png');bpy.ops.render.render(write_still=True)

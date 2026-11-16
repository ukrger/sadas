"""blender -b --python validate_environment.py -- --project /UnityProject"""
import bpy,json,argparse,sys
from pathlib import Path
from mathutils.bvhtree import BVHTree
p=argparse.ArgumentParser();p.add_argument('--project',required=True);a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []);base=Path(a.project).resolve()/'Assets/Models'
for name,height in [('SummerBirch',9.6),('SummerPine',11.2),('OvergrownHouse',4.5)]:
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);bpy.ops.import_scene.fbx(filepath=str(base/(name+'.fbx')))
 objects=list(bpy.context.scene.objects);expected=json.loads((base/(name+'_stats.json')).read_text());origin=next(o for o in objects if o.name==name).matrix_world.translation
 axes=[next(o for o in objects if o.name==key).matrix_world.translation-origin for key in ['AxisRight','AxisUp','AxisForward']];assert all(abs(v.length-1)<.001 for v in axes)
 vertices=[];faces=[];counts=[];wind=False
 for lod in range(3):
  meshes=[o for o in objects if o.type=='MESH' and o.name.startswith('LOD'+str(lod)+'_')];triangles=sum(sum(len(f.vertices)-2 for f in o.data.polygons) for o in meshes);counts.append(triangles)
  assert len(meshes)==expected['LOD'+str(lod)]['meshes'] and triangles==expected['LOD'+str(lod)]['triangles']
  for obj in meshes:
   if 'EnvLeaf' in obj.name or 'EnvIvy' in obj.name:
    assert len(obj.data.color_attributes)>0,'Missing vertex wind mask';assert max(v.color[3] for v in obj.data.color_attributes[0].data)>.9;wind=True
   if lod==0:
    offset=len(vertices);vertices.extend([obj.matrix_world@v.co for v in obj.data.vertices]);faces.extend([tuple(offset+i for i in f.vertices) for f in obj.data.polygons])
 assert wind and counts[0]>counts[1]>counts[2];heights=[(v-origin).dot(axes[1]) for v in vertices];assert min(heights)>-.03 and max(heights)<height,(name,min(heights),max(heights))
 if name=='OvergrownHouse':
  bvh=BVHTree.FromPolygons(vertices,faces)
  for x in [-.3,0,.3]:
   for y in [.4,1.3,2.2]:assert bvh.ray_cast(origin+axes[0]*x+axes[1]*y-axes[2]*4,axes[2],3)[0] is None,'Doorway obstructed'
  hit=bvh.ray_cast(origin+axes[0]*1.6+axes[1]*6-axes[2]*1.3,-axes[1],7);assert hit[0] is not None and (hit[0]-origin).dot(axes[1])<.4,'Broken roof quadrant should be open'
 print('PASS',name,'LOD triangles',counts,'wind masks; height',round(max(heights),3))

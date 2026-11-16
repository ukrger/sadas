"""blender -b --python validate_survival_supplies.py -- --project /UnityProject"""
import bpy,json,argparse,sys
from pathlib import Path
from mathutils.bvhtree import BVHTree
p=argparse.ArgumentParser();p.add_argument('--project',required=True);a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
base=Path(a.project).resolve()/'Assets/Models'
for name,height in [('WaterBottle',.28),('CannedFood',.13),('RepairKit',.19)]:
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);bpy.ops.import_scene.fbx(filepath=str(base/(name+'.fbx')))
 objects=list(bpy.context.scene.objects);expected=json.loads((base/(name+'_stats.json')).read_text());origin=next(o for o in objects if o.name==name).matrix_world.translation
 axes=[next(o for o in objects if o.name==key).matrix_world.translation-origin for key in ['AxisRight','AxisUp','AxisForward']]
 assert all(abs(v.length-1)<.001 for v in axes)
 vertices=[];faces=[];counts=[]
 for level in range(3):
  meshes=[o for o in objects if o.type=='MESH' and o.name.startswith('LOD'+str(level)+'_')]
  triangles=sum(sum(len(f.vertices)-2 for f in o.data.polygons) for o in meshes);counts.append(triangles)
  assert len(meshes)==expected['LOD'+str(level)]['meshes'];assert triangles==expected['LOD'+str(level)]['triangles']
  if level==0:
   for obj in meshes:
    offset=len(vertices);vertices.extend([obj.matrix_world@v.co for v in obj.data.vertices]);faces.extend([tuple(offset+i for i in f.vertices) for f in obj.data.polygons])
 assert counts[0]>counts[1]>counts[2]
 ys=[(v-origin).dot(axes[1]) for v in vertices];assert min(ys)>=-.001 and max(ys)<=height,(name,min(ys),max(ys))
 bvh=BVHTree.FromPolygons(vertices,faces);assert bvh.ray_cast(origin+axes[1],-axes[1],1.1)[0] is not None
 print('PASS',name,'LOD triangles',counts,'height',max(ys),'unit markers and solid upper surface')

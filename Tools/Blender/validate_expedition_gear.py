"""blender -b --python validate_expedition_gear.py -- --project /UnityProject"""
import bpy,json,argparse,sys
from pathlib import Path
from mathutils import Vector
from mathutils.bvhtree import BVHTree
p=argparse.ArgumentParser();p.add_argument('--project',required=True);a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
base=Path(a.project).resolve()/'Assets/Models'
for name,maxHeight,probe in [('FieldBackpack',.72,(0,.3)),('FieldMedkit',.39,(0,.15)),('SurvivalAxe',.78,(-.12,.65))]:
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);bpy.ops.import_scene.fbx(filepath=str(base/(name+'.fbx')))
 objects=list(bpy.context.scene.objects);expected=json.loads((base/(name+'_stats.json')).read_text())
 for key in ['AxisRight','AxisUp','AxisForward','LOD0','LOD1','LOD2']:assert any(o.name==key for o in objects),(name,key)
 origin=next(o for o in objects if o.name==name).matrix_world.translation
 axes=[next(o for o in objects if o.name==key).matrix_world.translation-origin for key in ['AxisRight','AxisUp','AxisForward']]
 assert all(abs(v.length-1)<.001 for v in axes)
 assert all(abs(axes[i].dot(axes[j]))<.001 for i,j in [(0,1),(0,2),(1,2)])
 vertices=[];faces=[];counts=[]
 for level in range(3):
  meshes=[o for o in objects if o.type=='MESH' and o.parent.name=='LOD'+str(level)]
  triangles=sum(sum(len(f.vertices)-2 for f in o.data.polygons) for o in meshes);counts.append(triangles)
  assert len(meshes)==expected['LOD'+str(level)]['meshes'];assert triangles==expected['LOD'+str(level)]['triangles']
  if level==0:
   for o in meshes:
    offset=len(vertices);vertices.extend([o.matrix_world@v.co for v in o.data.vertices]);faces.extend([tuple(offset+i for i in face.vertices) for face in o.data.polygons])
 assert counts[0]>counts[1]>counts[2]
 heights=[(v-origin).dot(axes[1]) for v in vertices];assert min(heights)>=-.001 and max(heights)<=maxHeight,(name,min(heights),max(heights))
 bvh=BVHTree.FromPolygons(vertices,faces);x,y=probe
 hit=bvh.ray_cast(origin+axes[0]*x+axes[1]*y-axes[2],axes[2],2)
 assert hit[0] is not None,('Missing key surface',name)
 print('PASS',name,'LOD triangles',counts,'height',round(max(heights),3),'unit axes and surface probe')

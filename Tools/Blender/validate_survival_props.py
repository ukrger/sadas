"""blender -b --python validate_survival_props.py -- --project /UnityProject"""
import argparse,sys,json,bpy
from pathlib import Path
from mathutils import Vector
from mathutils.bvhtree import BVHTree
p=argparse.ArgumentParser();p.add_argument('--project',required=True);a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
base=Path(a.project).resolve()/'Assets/Models'
for name,expectedMeshes,maxHeight in [('SupplyCrate',11,.76),('RepairBench',10,1.76)]:
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);bpy.ops.import_scene.fbx(filepath=str(base/(name+'.fbx')))
 objects=list(bpy.context.scene.objects)
 for key in ['AxisRight','AxisUp','AxisForward','LOD0','LOD1','LOD2']:assert any(o.name==key for o in objects),(name,key)
 assert len([o for o in objects if o.type=='MESH'])==expectedMeshes
 origin=next(o for o in objects if o.name==name).matrix_world.translation
 axes=[next(o for o in objects if o.name==key).matrix_world.translation-origin for key in ['AxisRight','AxisUp','AxisForward']]
 assert all(abs(v.length-1)<.001 for v in axes)
 vertices=[];faces=[];stats=[]
 for level in range(3):
  tris=0
  for o in objects:
   if o.type=='MESH' and o.parent.name=='LOD'+str(level):
    tris+=sum(len(f.vertices)-2 for f in o.data.polygons)
    if level==0:
     offset=len(vertices);vertices.extend([o.matrix_world@v.co for v in o.data.vertices]);faces.extend([tuple(offset+i for i in face.vertices) for face in o.data.polygons])
  stats.append(tris)
 assert stats[0]>stats[1]>stats[2],stats
 heights=[(v-origin).dot(axes[1]) for v in vertices];assert min(heights)>-.001 and max(heights)<=maxHeight,(name,min(heights),max(heights))
 bvh=BVHTree.FromPolygons(vertices,faces)
 x,z=(0,0) if name=='SupplyCrate' else (0,-.1)
 hit=bvh.ray_cast(origin+axes[0]*x+axes[1]*2+axes[2]*z,-axes[1],2)
 assert hit[0] is not None,('Missing top surface',name)
 print('PASS',name,'FBX roundtrip;',expectedMeshes,'meshes; triangles',stats,'height',max(heights))

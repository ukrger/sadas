import bpy, math,json
from mathutils import Vector
from pathlib import Path
import argparse, sys
parser=argparse.ArgumentParser();parser.add_argument('--project',required=True)
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
path=Path(parser.parse_args(args).project).resolve()/'Assets/Models/SummerRailcar.fbx'
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(path))
objects=list(bpy.context.scene.objects)
for name in ['AxisRight','AxisForward','AxisUp','LOD0','LOD1','LOD2']:
 assert any(o.name==name for o in objects),name
assert len([o for o in objects if o.type=='MESH'])==16
# Test semantic coordinates after FBX reimport, independent of exporter orientation.
origin=next(o for o in objects if o.name=='SummerRailcar').matrix_world.translation
axes=[next(o for o in objects if o.name==n).matrix_world.translation-origin for n in ['AxisRight','AxisUp','AxisForward']]
assert all(abs(v.length-1)<.001 for v in axes),axes
verts=[]
for obj in objects:
 if obj.type=='MESH' and obj.parent.name=='LOD0':
  verts.extend([Vector([(obj.matrix_world@v.co-origin).dot(axis) for axis in axes]) for v in obj.data.vertices])
mins=[min(v[i] for v in verts) for i in range(3)];maxs=[max(v[i] for v in verts) for i in range(3)]
assert abs(mins[2]+6.95)<.05 and abs(maxs[2]-6.95)<.05,(mins,maxs)
assert 3.9<maxs[1]<4.2
assert 2.8<maxs[0]<3.0
# Exit should not be blocked by side-wall polygons. A visual aperture is essential.
from mathutils.bvhtree import BVHTree
faces=[];vertices=[]
for obj in objects:
 if obj.type=='MESH' and obj.parent.name=='LOD0':
  offset=len(vertices);vertices.extend([obj.matrix_world@v.co for v in obj.data.vertices]);faces.extend([tuple(offset+i for i in p.vertices) for p in obj.data.polygons])
bvh=BVHTree.FromPolygons(vertices,faces)
for height in [1.3,2.0,2.8]:
 start=origin+axes[0]*2+axes[1]*height+axes[2]*-4.5
 hit=bvh.ray_cast(start,-axes[0],1.1)
 assert hit[0] is None,('Exit blocked',height,hit[0])
print('PASS FBX roundtrip: 16 meshes, 3 LODs, unit axes, expected bounds, clear doorway',mins,maxs)

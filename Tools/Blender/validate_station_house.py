"""blender -b --python validate_station_house.py -- --project /UnityProject"""
import bpy,argparse,sys
from pathlib import Path
from mathutils import Vector
from mathutils.bvhtree import BVHTree
p=argparse.ArgumentParser();p.add_argument('--project',required=True);args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [];a=p.parse_args(args)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(Path(a.project).resolve()/'Assets/Models/StationHouse.fbx'))
objects=list(bpy.context.scene.objects)
for name in ['AxisRight','AxisForward','AxisUp','LOD0','LOD1','LOD2']:assert any(o.name==name for o in objects),name
assert len([o for o in objects if o.type=='MESH'])==15
origin=next(o for o in objects if o.name=='StationHouse').matrix_world.translation
axes=[next(o for o in objects if o.name==n).matrix_world.translation-origin for n in ['AxisRight','AxisUp','AxisForward']]
assert all(abs(v.length-1)<.001 for v in axes)
vertices=[];faces=[]
for o in objects:
 if o.type=='MESH' and o.parent.name=='LOD0':
  offset=len(vertices);vertices.extend([o.matrix_world@v.co for v in o.data.vertices]);faces.extend([tuple(offset+i for i in face.vertices) for face in o.data.polygons])
bvh=BVHTree.FromPolygons(vertices,faces)
for x in [-.3,0,.3]:
 for height in [.5,1.2,2.0]:
  start=origin+axes[0]*x+axes[1]*height+axes[2]*-4.5
  hit=bvh.ray_cast(start,axes[2],3.5)
  assert hit[0] is None,('Doorway obstructed',x,height,hit)
# Table must survive exporting; check downward ray on the worktop.
hit=bvh.ray_cast(origin+axes[0]*1.6+axes[1]*1.5+axes[2]*.8,-axes[1],.6)
assert hit[0] is not None,'Missing table surface'
print('PASS StationHouse FBX: 15 meshes, 3 LODs, unit axes, doorway clear, table present')

"""blender -b --python validate_combat_models.py -- --project /UnityProject"""
import bpy,json,argparse,sys
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--project',required=True);a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
base=Path(a.project).resolve()/'Assets/Models'
for name in ['ServicePistol','AmmoBox','ForestMutant','StationZombie']:
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 # Import just one asset, and ignore orphaned actions from previous imports.
 previous=set(bpy.data.actions);bpy.ops.import_scene.fbx(filepath=str(base/(name+'.fbx')))
 objects=list(bpy.context.scene.objects);expected=json.loads((base/(name+'_stats.json')).read_text())
 for marker in ['AxisRight','AxisUp','AxisForward','LOD0','LOD1','LOD2']:assert any(o.name==marker for o in objects),(name,marker)
 origin=next(o for o in objects if o.name==name).matrix_world.translation
 for marker in ['AxisRight','AxisUp','AxisForward']:assert abs((next(o for o in objects if o.name==marker).matrix_world.translation-origin).length-1)<.001
 counts=[]
 for level in range(3):
  meshes=[o for o in objects if o.type=='MESH' and o.name.startswith('LOD'+str(level)+'_')];triangles=sum(sum(len(f.vertices)-2 for f in o.data.polygons) for o in meshes)
  assert len(meshes)==expected['LOD'+str(level)]['meshes'];assert triangles==expected['LOD'+str(level)]['triangles'];counts.append(triangles)
 assert counts[0]>counts[1]>counts[2]
 if name in ['ForestMutant','StationZombie']:
  rigs=[o for o in objects if o.type=='ARMATURE'];assert len(rigs)==1 and len(rigs[0].data.bones)>=11
  assert len([action for action in bpy.data.actions if action not in previous])>=2,'Missing baked idle/walk actions'
  for obj in objects:
   if obj.type=='MESH':
    assert len(obj.vertex_groups)>0 and any(m.type=='ARMATURE' for m in obj.modifiers),'Unbound mesh'
  print('RIG PASS',len(rigs[0].data.bones),'bones; actions',[x.name for x in bpy.data.actions if x not in previous])
 print('PASS',name,'LOD triangles',counts)

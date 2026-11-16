"""Reimport and validate the v0.9 FBX batch. Run in Blender background mode."""
import bpy,argparse,json,sys
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--project',required=True);a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
base=Path(a.project)/'Assets/Models'
for name,extent_axis,lo,hi in [('PumpShotgun',2,1.1,1.25),('PlayerHands',2,.7,1.1),('FieldRadio',1,.78,.85),('AutomaticRifle',2,1.1,1.25),('RailSignal',1,3.3,3.5),('FuelCan',1,.667,.668)]:
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);bpy.ops.import_scene.fbx(filepath=str(base/(name+'.fbx')))
 objs=list(bpy.context.scene.objects);origin=next(o for o in objs if o.name==name).matrix_world.translation
 axes=[next(o for o in objs if o.name==key).matrix_world.translation-origin for key in ['AxisRight','AxisUp','AxisForward']]
 assert all(abs(axis.length-1)<.001 for axis in axes);assert all(abs(axes[i].dot(axes[j]))<.001 for i in range(3) for j in range(i))
 stats=json.loads((base/(name+'_stats.json')).read_text());counts=[];points=[]
 for lod in range(3):
  meshes=[o for o in objs if o.type=='MESH' and o.name.startswith('LOD'+str(lod)+'_')]
  count=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in meshes);counts.append(count)
  assert count==stats['LOD'+str(lod)]['triangles'];assert len(meshes)==stats['LOD'+str(lod)]['meshes']
  assert all(o.data.materials and len(o.data.vertices)>0 for o in meshes)
  if lod==0:points=[o.matrix_world@v.co for o in meshes for v in o.data.vertices]
 assert counts[0]>counts[1]>counts[2]
 values=[(v-origin).dot(axes[extent_axis]) for v in points];extent=max(values)-min(values);assert lo<extent<hi,(name,extent)
 if name=='PlayerHands':assert any('HandRightGlove' in o.name for o in objs) and any('HandLeftGlove' in o.name for o in objs)
 print('PASS',name,'LOD',counts,'semantic extent',round(extent,4),'unit orthogonal axes and materials')

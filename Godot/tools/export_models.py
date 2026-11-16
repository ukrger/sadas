"""Blender batch exporter for our own original .blend files; no Unity dependency.
blender -b --python export_models.py -- --source /TrainSummer/Tools/Blender/Source --output /TrainSummerGodot/assets/models
"""
import bpy,sys,argparse,json
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--source',required=True);p.add_argument('--output',required=True);a=p.parse_args(sys.argv[sys.argv.index('--')+1:]);out=Path(a.output);out.mkdir(parents=True,exist_ok=True);manifest={}
for source in sorted(Path(a.source).glob('*.blend')):
 bpy.ops.wm.open_mainfile(filepath=str(source));name=source.stem
 for o in list(bpy.context.scene.objects):
  if o.type in ['CAMERA','LIGHT']:bpy.data.objects.remove(o,do_unlink=True)
 for o in bpy.context.scene.objects:o.hide_set(False);o.hide_render=False;o.select_set(True)
 bpy.ops.export_scene.gltf(filepath=str(out/(name+'.glb')),export_format='GLB',use_selection=True,export_animations=True,export_extras=False,export_yup=True)
 manifest[name]={'bytes':(out/(name+'.glb')).stat().st_size,'lod_meshes':{str(i):len([o for o in bpy.context.scene.objects if o.type=='MESH' and o.name.startswith('LOD'+str(i)+'_')]) for i in range(3)}}
 print('EXPORTED',name,flush=True)
(out/'manifest.json').write_text(json.dumps(manifest,indent=2));print('ALL MODELS',len(manifest),flush=True)

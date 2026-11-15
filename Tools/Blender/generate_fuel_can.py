"""Blender 3.6+/4.x: blender -b --python generate_fuel_can.py -- --project D:/MyUnityGame"""
import bpy, argparse, sys
from pathlib import Path
args = sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
parser=argparse.ArgumentParser();parser.add_argument('--project',required=True);opts=parser.parse_args(args)
out=Path(opts.project).expanduser().resolve()/'Assets'/'Models';out.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.context.scene.unit_settings.system='METRIC'
mat=bpy.data.materials.new('PaintedOlive');mat.diffuse_color=(.19,.24,.10,1);mat.use_nodes=True
bsdf=mat.node_tree.nodes.get('Principled BSDF');bsdf.inputs['Base Color'].default_value=mat.diffuse_color;bsdf.inputs['Metallic'].default_value=.65;bsdf.inputs['Roughness'].default_value=.48
parts=[]
def box(name,location,scale,bevel=.018):
 bpy.ops.mesh.primitive_cube_add(size=1,location=location);o=bpy.context.object;o.name=name;o.dimensions=scale
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(mat)
 b=o.modifiers.new('Edge bevel','BEVEL');b.width=bevel;b.segments=3
 o.modifiers.new('Weighted normals','WEIGHTED_NORMAL');parts.append(o);return o
box('CanBody',(0,0,.28),(.34,.16,.53),.025)
box('HandleLeft',(-.10,0,.60),(.035,.055,.13),.009)
box('HandleRight',(.10,0,.60),(.035,.055,.13),.009)
box('HandleTop',(0,0,.66),(.23,.055,.035),.009)
box('Cap',(.105,0,.565),(.075,.09,.035),.009)
for side in [-1,1]:
 for angle in [-.65,.65]:
  rib=box('PressedRib',(0,side*.083,.29),(.025,.012,.32),.005);rib.rotation_euler[1]=angle
bpy.ops.object.select_all(action='DESELECT')
for o in parts:o.select_set(True)
bpy.context.view_layer.objects.active=parts[0]
bpy.ops.export_scene.fbx(filepath=str(out/'FuelCan.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,use_mesh_modifiers=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'FuelCan.blend'))
print('Exported',out/'FuelCan.fbx')

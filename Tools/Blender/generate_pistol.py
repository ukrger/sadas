"""blender -b --python generate_pistol.py -- --project /UnityProject [--render]"""
import sys,argparse
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from prop_pipeline import PropBuilder
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
b=PropBuilder(a.project,'ServicePistol',[('GunSteel',(.12,.14,.15),.8),('GunGrip',(.12,.095,.065),0),('GearSteel',(.22,.23,.20),.7)])
# Grip origin at bottom; barrel points semantic +Z for the first-person camera.
for lod in range(3):
 b.begin(lod)
 b.box('Frame',(0,.145,.025),(.043,.05,.17),0,.006)
 b.box('Slide',(0,.19,.05),(.045,.046,.205),0,.005)
 b.box('Grip',(0,.075,-.035),(.04,.115,.058),1,.01)
 b.cylinder('Barrel',(0,.188,.166),.013,.048,2,'z')
 if lod<2:
  b.box('Front sight',(0,.22,.127),(.008,.012,.012),0,.001)
  for side in [-1,1]:b.box('Rear sight',(side*.012,.22,-.035),(.008,.012,.014),0,.001)
  b.box('Trigger guard front',(0,.105,.052),(.025,.055,.012),0,.003)
  b.box('Trigger guard bottom',(0,.082,.02),(.025,.012,.075),0,.003)
  b.box('Trigger',(0,.123,.02),(.012,.025,.009),2,.001)
  b.box('Magazine floor',(0,.013,-.035),(.045,.012,.064),0,.002)
 if lod==0:
  for z in [-.034,-.023,-.012]:
   for side in [-1,1]:b.box('Slide serration',(side*.023,.19,z),(.003,.032,.003),2,.0005)
  for y in [.04,.06,.08,.1]:
   for side in [-1,1]:b.box('Grip rib',(side*.021,y,-.035),(.003,.003,.048),0,.001)
  b.box('Ejection port',(.023,.199,.05),(.003,.018,.035),2,.001)
 b.finish()
b.export()
if a.render:b.render(.115,.46)

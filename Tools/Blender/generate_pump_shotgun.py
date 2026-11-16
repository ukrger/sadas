"""Original game-art shotgun. Blender 4.x, no external assets. Not a manufacturing model."""
import sys,argparse,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from prop_pipeline import PropBuilder
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
b=PropBuilder(a.project,'PumpShotgun',[('GunSteel',(.12,.14,.15),.8),('ShotgunWood',(.26,.12,.045),0),('GunGrip',(.075,.065,.05),0),('GearMark',(.70,.64,.45),.2)])
for lod in range(3):
 b.begin(lod)
 b.box('Receiver',(0,.12,.20),(.065,.105,.28),0,.012)
 b.cylinder('Barrel',(0,.165,.57),.022,.56,0,'z');b.cylinder('Magazine tube',(0,.108,.55),.019,.48,0,'z')
 b.cylinder('Muzzle ring',(0,.165,.855),.025,.02,0,'z')
 b.box('Pump fore-end',(0,.095,.49),(.074,.075,.22),1,.015)
 b.box('Stock',(0,.055,-.13),(.075,.145,.31),1,.018)
 b.box('Butt pad',(0,.055,-.29),(.082,.15,.025),2,.006)
 grip=b.box('Grip',(0,-.005,.045),(.062,.16,.07),1,.012);grip.rotation_euler.x=-.22
 for side in [-1,1]:b.box('Trigger guard side',(side*.022,-.015,.15),(.008,.065,.115),0,.004)
 b.box('Trigger guard base',(0,-.047,.15),(.05,.008,.115),0,.003)
 for side in [-1,1]:b.box('Rear notch',(side*.016,.204,.08),(.008,.018,.025),0,.002)
 b.box('Front sight',(0,.195,.79),(.012,.025,.02),0,.003)
 if lod<2:
  b.box('Ejection recess',(.033,.13,.235),(.003,.045,.095),2,.001)
  for z in [.41,.44,.47,.50,.53,.56]:b.box('Pump grooves',(0,.095,z),(.078,.077,.006),2,.001)
  for z in [.14,.26]:b.cylinder('Receiver pin',(.035,.12,z),.006,.004,3,'x')
 if lod==0:
  for z in [.04,.09,.14]:b.box('Sight rail',(0,.18,z),(.024,.009,.02),0,.001)
 b.finish()
b.export()
if a.render:b.render(.12,1.30,(1.1,1.3,.65))

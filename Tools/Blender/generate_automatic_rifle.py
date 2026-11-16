"""Original non-branded game-art rifle; decorative geometry, no functional internals."""
import sys,argparse,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from prop_pipeline import PropBuilder
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
b=PropBuilder(a.project,'AutomaticRifle',[('GunSteel',(.12,.14,.15),.8),('RifleWood',(.29,.14,.045),0),('GunGrip',(.08,.065,.045),0),('GearMark',(.77,.71,.5),.2)])
for lod in range(3):
 b.begin(lod)
 b.box('Receiver',(0,.13,.19),(.07,.10,.29),0,.009)
 b.box('Handguard',(0,.13,.43),(.075,.085,.20),1,.012)
 b.cylinder('Barrel',(0,.155,.635),.016,.25,0,'z');b.cylinder('Muzzle',(0,.155,.78),.023,.06,0,'z')
 b.cylinder('Gas silhouette',(0,.20,.58),.012,.19,0,'z')
 b.box('Stock',(0,.105,-.14),(.065,.125,.34),1,.018);b.box('Butt plate',(0,.105,-.317),(.075,.14,.022),0,.003)
 grip=b.box('Grip',(0,-.005,.08),(.05,.15,.075),2,.01);grip.rotation_euler.x=-.24
 b.box('Magazine upper',(0,.01,.25),(.045,.16,.085),0,.008)
 mag=b.box('Magazine curve',(0,-.105,.28),(.045,.15,.085),0,.008);mag.rotation_euler.x=.28
 for side in [-1,1]:b.box('Trigger guard',(side*.024,.005,.16),(.008,.052,.10),0,.003)
 b.box('Guard base',(0,-.02,.16),(.052,.008,.10),0,.003)
 b.box('Front sight',(0,.214,.70),(.018,.065,.025),0,.003);b.box('Rear sight base',(0,.225,.19),(.042,.012,.035),0,.002)
 for side in [-1,1]:b.box('Rear notch',(side*.016,.239,.19),(.009,.020,.025),0,.002)
 if lod<2:
  b.cylinder('Bolt handle',(.052,.15,.25),.008,.045,0,'x')
  for z in [.40,.44,.48]:b.box('Handguard ribs',(0,.13,z),(.079,.089,.008),2,.002)
 if lod==0:
  for z in [.07,.17,.27]:b.cylinder('Receiver pin',(.038,.13,z),.005,.006,3,'x')
  for x in [-.026,.026]:
   for z in [.23,.25,.27]:b.box('Magazine seam',(x,-.07,z),(.003,.16,.003),2,.001)
 b.finish()
b.export()
if a.render:b.render(.10,1.35,(1.1,1.4,.7),floor=-.22)

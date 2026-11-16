"""Standalone original radio prop, with three LODs and automatic FBX export."""
import sys,argparse
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from prop_pipeline import PropBuilder
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
b=PropBuilder(a.project,'FieldRadio',[('RadioPaint',(.22,.29,.15),.3),('RadioDark',(.035,.045,.035),.1),('GearSteel',(.22,.23,.20),.7),('GearMark',(.77,.71,.50),0)])
for lod in range(3):
 b.begin(lod)
 b.box('Case',(0,.16,0),(.43,.29,.19),0,.016)
 for x in [-.17,.17]:b.box('Foot',(x,.015,0),(.045,.03,.14),1,.004)
 b.box('Speaker recess',(-.105,.17,.098),(.16,.19,.012),1,.004)
 if lod<2:
  for y in [.10,.125,.15,.175,.20,.225,.25]:b.box('Speaker slat',(-.105,y,.108),(.15,.006,.006),2,.001)
 b.box('Tuning window',(.095,.23,.10),(.15,.045,.012),1,.003)
 b.box('Frequency stripe',(.06,.23,.109),(.003,.035,.003),3,.001)
 for x in [.04,.14]:b.cylinder('Knob',(x,.13,.11),.024,.025,1,'z')
 for x in [-.12,.12]:b.box('Handle arm',(x,.34,0),(.018,.08,.026),2,.004)
 b.box('Carry handle',(0,.38,0),(.26,.025,.03),1,.006)
 b.cylinder('Antenna',(-.16,.56,-.06),.005,.51,2)
 b.box('Module slot',(.217,.17,0),(.01,.12,.12),1,.002)
 b.box('Relay cartridge',(.228,.17,0),(.02,.09,.08),3,.002)
 if lod==0:
  for x in [-.19,.19]:
   for y in [.06,.28]:b.cylinder('Screw',(x,y,.103),.005,.006,2,'z')
 b.finish()
b.export()
if a.render:b.render(.35,1.15,(1.1,-1.4,.85))

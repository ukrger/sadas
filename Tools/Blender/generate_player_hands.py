"""Two original gloved first-person arms in an approximate two-handed grip.
Static geometry, no skinning: procedural weapon pivots animate the entire set.
"""
import sys,argparse,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from prop_pipeline import PropBuilder
from environment_pipeline import branch
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--render',action='store_true');a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
b=PropBuilder(a.project,'PlayerHands',[('HandRightSleeve',(.20,.25,.13),0),('HandRightGlove',(.16,.10,.055),0),('HandLeftSleeve',(.20,.25,.13),0),('HandLeftGlove',(.16,.10,.055),0),('HandRightStitch',(.38,.31,.19),0),('HandLeftStitch',(.38,.31,.19),0)])
for lod in range(3):
 b.begin(lod)
 # Coordinates relative to weapon grip. Fingers wrap into the model's grip region.
 for left in [False,True]:
  x=-.045 if left else .035;y=.075 if left else -.025;z=.49 if left else .05;mi=2 if left else 0
  start=(x+(-.19 if left else .13),y-.13,z-.42)
  branch(b,start,(x,y-.02,z-.055),.064,.047,12 if lod==0 else 6,mi)
  b.box('Cuff',(x,y-.025,z-.07),(.104,.095,.07),mi,.012)
  b.box('Palm',(x,y,z),(.077,.09,.09),mi+1,.018)
  if lod<2:
   for finger in range(4):
    fz=z-.025+finger*.019
    # Two short segments keep the glove from looking like an unbroken block.
    branch(b,(x-.026,y+.034,fz),(x-.054,y+.015,fz+.004),.012,.011,8 if lod==0 else 5,mi+1)
    branch(b,(x-.054,y+.015,fz+.004),(x-.045,y-.015,fz+.008),.011,.009,8 if lod==0 else 5,mi+1)
   branch(b,(x+.028,y+.015,z+.025),(x+.043,y+.032,z-.016),.016,.012,8 if lod==0 else 5,mi+1)
  else:b.box('Finger silhouette',(x-.035,y+.004,z),(.035,.065,.085),mi+1,0)
  if lod==0:
   for q in [-.025,0,.025]:b.box('Glove seams',(x+q,y+.047,z),(.002,.002,.066),4 if not left else 5,.0005)
 b.finish()
b.export()
if a.render:b.render(.01,1.0,(1.0,-1.15,.8),floor=-.26)

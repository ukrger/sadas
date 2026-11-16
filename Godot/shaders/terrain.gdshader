shader_type spatial;
render_mode diffuse_burley;
varying vec3 world;
void vertex(){world=(MODEL_MATRIX*vec4(VERTEX,1.0)).xyz;}
float hash(vec2 p){return fract(sin(dot(p,vec2(127.1,311.7)))*43758.5453);}
float noise(vec2 p){vec2 i=floor(p),f=fract(p);f=f*f*(3.0-2.0*f);return mix(mix(hash(i),hash(i+vec2(1,0)),f.x),mix(hash(i+vec2(0,1)),hash(i+1.0),f.x),f.y);}
void fragment(){float n=noise(world.xz*.3)*.65+noise(world.xz*4.0)*.25+noise(world.xz*32.0)*.10;ALBEDO=mix(vec3(.045,.095,.018),vec3(.18,.25,.065),n);ROUGHNESS=.96;}

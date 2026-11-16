shader_type spatial;
render_mode cull_disabled, diffuse_burley;
uniform vec4 tint:source_color=vec4(.24,.39,.09,1.0);
uniform float amplitude=.035;
void vertex(){vec3 p=(MODEL_MATRIX*vec4(VERTEX,1.0)).xyz;float mask=COLOR.a;VERTEX.x+=sin(TIME*.8+p.x*.6+p.z*.4)*amplitude*mask;}
void fragment(){ALBEDO=tint.rgb*mix(vec3(1),COLOR.rgb,.5);ROUGHNESS=.88;}

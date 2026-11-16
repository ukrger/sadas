class_name TSWorld
extends Node3D

var game
var train:Node3D
var train_floor:StaticBody3D
var train_bodies:Array=[]
var stations:Array = []
var pickups:Dictionary = {}
var boards:Dictionary = {}
var enemies:Dictionary = {}
var pool:Array = []
var active:Dictionary = {}
var terrain_material:ShaderMaterial
var parts:Dictionary = {}
var grass_mesh:ArrayMesh
var grass_material:ShaderMaterial
var ground_material:StandardMaterial3D
var update_clock:float = 0
const TILE = 80.0
const TREES_PER_SPECIES = 32

func route(distance:float) -> Transform3D:
	var z = distance
	var x = 24.0*(1.0-cos(z/150.0))
	var slope = 24.0/150.0*sin(z/150.0)
	return Transform3D(Basis(Vector3.UP,atan(slope)),Vector3(x,0,z))

func ground_height(x:float,z:float) -> float:
	var offset = absf(x-route(z).origin.x)
	var weight = smoothstep(18,45,offset)
	for stop in game.stops:
		if absf(z-stop)<55: weight*=smoothstep(25,55,absf(z-stop))
	return -.08+weight*(sin(x*.05)*1.3+sin(z*.033+x*.025)*1.1+.8)

func setup(owner_game) -> void:
	game = owner_game
	ground_material = TSAssets.surface("Wood",Color(.36,.22,.11))
	terrain_material = ShaderMaterial.new()
	terrain_material.shader = load("res://shaders/terrain.gdshader")
	grass_material = ShaderMaterial.new()
	grass_material.shader = load("res://shaders/foliage.gdshader")
	grass_material.set_shader_parameter("tint",Color(.28,.43,.10))
	grass_mesh = _grass_mesh()
	for species in ["SummerBirch","SummerPine"]:
		for lod in range(3): parts[species+str(lod)] = TSAssets.plant_parts(species,lod)
	var backdrop=MeshInstance3D.new()
	var plane=PlaneMesh.new()
	plane.size=Vector2(3000,40000)
	backdrop.mesh=plane
	backdrop.material_override=terrain_material
	backdrop.position=Vector3(0,-3,12000)
	add_child(backdrop)
	_build_train()
	train_bodies=train.find_children("*","CollisionObject3D",true,false)
	for index in range(4): _build_station(index)
	for index in range(10):
		var tile = _make_tile()
		pool.append(tile)
	refresh(true)

func _build_train() -> void:
	train = Node3D.new()
	train.name = "MovingTrain"
	add_child(train)
	train.add_child(TSAssets.model("SummerRailcar"))
	train_floor=TSAssets.collider(train,Vector3(0,1,0),Vector3(3,.3,12))
	TSAssets.collider(train,Vector3(0,4,0),Vector3(3.2,.2,12))
	for side in [-1,1]:
		var start = -6.0 if side<0 else -3.4
		for row in [[1.65,1.0],[3.65,.55]]:
			TSAssets.collider(train,Vector3(side*1.5,row[0],(start+6)/2),Vector3(.12,row[1],6-start))
		var z = start
		while z<=6:
			TSAssets.collider(train,Vector3(side*1.5,2.75,z),Vector3(.15,1.2,.1))
			z+=1.8
	for z in [-6,6]:
		TSAssets.collider(train,Vector3(0,1.7,z),Vector3(3,1.1,.12))
		TSAssets.collider(train,Vector3(0,3.65,z),Vector3(3,.6,.12))
	for i in range(4): TSAssets.collider(train,Vector3(2.7-i*.3,.15+i*.25,-4.5),Vector3(.4,.3,2))
	for z in [-1.7,.1,1.9]:
		for side in [-1,1]: TSAssets.collider(train,Vector3(side*1.04,1.65,z),Vector3(.66,.15,1.4))
	var cab = TSAssets.collider(train,Vector3(0,1.6,4.5),Vector3(1.8,.8,.6))
	cab.set_meta("interaction",{"kind":"cab","id":"cab"})
	var bench = Node3D.new()
	bench.position = Vector3(-1.05,1.15,-4.5)
	bench.rotation.y = -PI/2
	train.add_child(bench)
	bench.add_child(TSAssets.model("RepairBench"))
	var bench_body = TSAssets.collider(bench,Vector3(0,.5,0),Vector3(1.8,1,.7))
	bench_body.set_meta("interaction",{"kind":"bench","id":"bench"})
	pickup("train/axe", "axe","SurvivalAxe",bench,Vector3(.65,1.02,-.15),Vector3(.4,.8,.15))
	pickup("train/pistol","pistol","ServicePistol",bench,Vector3(.15,1.02,-.05),Vector3(.12,.3,.3))
	pickup("train/shotgun","shotgun","PumpShotgun",train,Vector3(-1.1,1.5,-2.7),Vector3(.18,.5,1.2)).rotation.x=-PI/2
	pickup("train/rifle","rifle","AutomaticRifle",train,Vector3(-1.1,1.5,-3.2),Vector3(.18,.5,1.2)).rotation.x=-PI/2
	pickup("train/ammo","pistol_ammo","AmmoBox",bench,Vector3(-.1,1.02,-.15),Vector3(.15,.15,.15),24)
	pickup("train/shells","shells","AmmoBox",bench,Vector3(.4,1.02,.1),Vector3(.15,.15,.15),12)
	pickup("train/rifle_ammo","rifle_ammo","AmmoBox",bench,Vector3(-.4,1.02,.1),Vector3(.15,.15,.15),60)
	for i in range(3): pickup("train/supply"+str(i),["water","food","kit"][i],["WaterBottle","CannedFood","RepairKit"][i],train,Vector3(.5+i*.25,1.16,-2.9),Vector3(.2,.3,.2))
	var pack = TSAssets.collider(train,Vector3(-.85,1.48,-2.9),Vector3(.6,.7,.4))
	var pack_visual = TSAssets.model("FieldBackpack")
	pack_visual.position.y = -.33
	pack.add_child(pack_visual)
	pack.set_meta("interaction",{"kind":"backpack","id":"backpack"})

func pickup(id:String,kind:String,model_name:String,parent:Node3D,at:Vector3,size:Vector3,amount:int=1) -> Node3D:
	var root = Node3D.new()
	root.name = id.replace("/","_")
	root.position = at
	parent.add_child(root)
	root.add_child(TSAssets.model(model_name))
	var body = TSAssets.collider(root,Vector3(0,size.y*.5,0),size)
	body.set_meta("interaction",{"id":id,"kind":kind,"amount":amount})
	pickups[id] = root
	return root

func _occluder(parent:Node3D,at:Vector3,size:Vector3) -> void:
	var node=OccluderInstance3D.new()
	var shape=BoxOccluder3D.new()
	shape.size=size
	node.occluder=shape
	node.position=at
	parent.add_child(node)

func _build_station(index:int) -> void:
	var station = NavigationRegion3D.new()
	station.name = "Station"+str(index+1)
	station.transform = route(game.stops[index])
	add_child(station)
	stations.append(station)
	var concrete = TSAssets.surface("Plaster",Color(.55,.53,.45))
	TSAssets.collider(station,Vector3(0,-.2,0),Vector3(90,.25,90))
	TSAssets.box(station,Vector3(6,.35,0),Vector3(7,.7,26),concrete)
	for i in range(3): TSAssets.box(station,Vector3(9.65+i*.3,.3-i*.1,0),Vector3(.4,.6-i*.2,2),concrete)
	var house = Node3D.new()
	house.position = Vector3(14,0,0)
	house.rotation.y = PI/2
	station.add_child(house)
	house.add_child(TSAssets.model("StationHouse"))
	# Conservative occluders only in opaque lower walls and solid rear wall; keep windows/door open.
	for side in [-1,1]:
		_occluder(house,Vector3(side*3,.7,0),Vector3(.12,1,5))
		_occluder(house,Vector3(side*1.85,.7,-2.5),Vector3(2.3,1,.12))
	_occluder(house,Vector3(0,1.6,2.5),Vector3(6,2.8,.12))
	TSAssets.collider(house,Vector3(0,.1,0),Vector3(6,.2,5))
	TSAssets.collider(house,Vector3(0,.24,0),Vector3(5.8,.08,4.8))
	TSAssets.collider(house,Vector3(0,.14,-3),Vector3(6,.16,1.2))
	for side in [-1,1]:
		TSAssets.collider(house,Vector3(side*3,.7,0),Vector3(.16,1,5))
		TSAssets.collider(house,Vector3(side*3,2.72,0),Vector3(.16,.56,5))
		for z in [-2.4,-1.4,0,1.4,2.4]: TSAssets.collider(house,Vector3(side*3,1.82,z),Vector3(.17,1.25,.15))
		TSAssets.collider(house,Vector3(side*1.85,.7,-2.5),Vector3(2.3,1,.16))
		TSAssets.collider(house,Vector3(side*1.85,2.72,-2.5),Vector3(2.3,.56,.16))
		for x in [side*.7,side*3]: TSAssets.collider(house,Vector3(x,1.82,-2.5),Vector3(.16,1.25,.18))
	TSAssets.collider(house,Vector3(0,2.72,-2.5),Vector3(1.4,.56,.18))
	TSAssets.collider(house,Vector3(0,1.6,2.5),Vector3(6,2.8,.16))
	TSAssets.collider(house,Vector3(1.6,1.05,.8),Vector3(1.6,.12,.8))
	var barrier = TSAssets.box(house,Vector3(0,1,-2.66),Vector3(1.35,1.45,.10),TSAssets.surface("Wood",Color(.32,.21,.1)))
	barrier.set_meta("damage_id","boards/"+str(index))
	boards["boards/"+str(index)] = {"node":barrier,"health":60.0}
	pickup("station%d/fuel"%index,"fuel","FuelCan",house,Vector3(1.6,1.12,.8),Vector3(.36,.7,.2),80 if game.long_route else 20)
	pickup("station%d/parts"%index,"crate","SupplyCrate",house,Vector3(-1.8,.28,-.5),Vector3(1.1,.8,.8))
	pickup("station%d/medkit"%index,"medkit","FieldMedkit",house,Vector3(-2.4,.3,1.5),Vector3(.36,.4,.24))
	for i in range(5):
		pickup("station%d/supply%d"%[index,i],["water","food","pistol_ammo","shells","rifle_ammo"][i],["WaterBottle","CannedFood","AmmoBox","AmmoBox","AmmoBox"][i],house,Vector3(-.5+i*.4,.28,1.7),Vector3(.2,.3,.2),[1,1,24,12,45][i])
	var radio = TSAssets.collider(house,Vector3(1.05,1.45,.75),Vector3(.45,.75,.22))
	var visual = TSAssets.model("FieldRadio")
	visual.position.y=-.33
	radio.add_child(visual)
	radio.set_meta("interaction",{"kind":"relay","index":index,"id":"relay"+str(index)})
	var signal_visual = TSAssets.model("RailSignal")
	signal_visual.position = Vector3(-3.8,0,4)
	station.add_child(signal_visual)
	for i in range(2):
		var enemy = TSEnemy.new()
		enemy.game = game
		enemy.enemy_id = "station%d/enemy%d"%[index,i]
		enemy.mutant = i==1
		enemy.position = Vector3(-8,1,7) if i==0 else Vector3(18,1,8)
		station.add_child(enemy)
		enemies[enemy.enemy_id] = enemy
	var nav = NavigationMesh.new()
	nav.agent_radius = .5
	nav.agent_height = 2.0
	nav.agent_max_climb = .5
	nav.cell_size = .25
	nav.cell_height = .25
	nav.geometry_collision_mask=1
	nav.geometry_parsed_geometry_type = NavigationMesh.PARSED_GEOMETRY_STATIC_COLLIDERS
	station.navigation_mesh = nav
	station.bake_navigation_mesh.call_deferred(false)

func _make_tile() -> Dictionary:
	var root = Node3D.new()
	add_child(root)
	var terrain = MeshInstance3D.new()
	terrain.material_override = terrain_material
	root.add_child(terrain)
	var body = StaticBody3D.new()
	var shape = CollisionShape3D.new()
	body.add_child(shape)
	root.add_child(body)
	var rail = MeshInstance3D.new()
	rail.material_override = TSAssets.surface("Steel",Color(.32,.34,.32))
	root.add_child(rail)
	var sleepers = MeshInstance3D.new()
	sleepers.material_override = ground_material
	root.add_child(sleepers)
	var trunks=[]
	for i in range(TREES_PER_SPECIES*2):
		var trunk_body=StaticBody3D.new()
		var trunk_collider=CollisionShape3D.new()
		var trunk_shape=CylinderShape3D.new()
		trunk_shape.radius=.17
		trunk_shape.height=5.2
		trunk_collider.shape=trunk_shape
		trunk_collider.position.y=2.6
		trunk_body.add_child(trunk_collider)
		root.add_child(trunk_body)
		trunks.append(trunk_body)
	var plants = []
	for species in ["SummerBirch","SummerPine"]:
		for lod in range(3):
			for part in parts[species+str(lod)]:
				var node = MultiMeshInstance3D.new()
				var multi = MultiMesh.new()
				multi.transform_format = MultiMesh.TRANSFORM_3D
				multi.mesh = part.mesh
				multi.instance_count = TREES_PER_SPECIES
				node.multimesh = multi
				node.visibility_range_begin = [0.0,70.0,160.0][lod]
				node.visibility_range_end = [70.0,160.0,390.0][lod]
				node.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF if lod==2 else GeometryInstance3D.SHADOW_CASTING_SETTING_ON
				if part.material:
					node.material_override = part.material
					if "Leaf" in str(part.material.resource_name):
						var mat = ShaderMaterial.new()
						mat.shader = load("res://shaders/foliage.gdshader")
						mat.set_shader_parameter("tint",Color(.15,.29,.07) if species=="SummerPine" else Color(.26,.43,.10))
						node.material_override=mat
				root.add_child(node)
				plants.append({"node":node,"species":species,"part":part.transform})
	var ruin=Node3D.new()
	root.add_child(ruin)
	ruin.add_child(TSAssets.model("OvergrownHouse"))
	TSAssets.collider(ruin,Vector3(0,.1,0),Vector3(6.6,.2,5.6))
	TSAssets.collider(ruin,Vector3(0,.23,0),Vector3(6.1,.06,5.1))
	for side in [-1,1]:
		TSAssets.collider(ruin,Vector3(side*3.15,.73,0),Vector3(.22,1,5.4))
		TSAssets.collider(ruin,Vector3(side*3.15,2.83,0),Vector3(.22,.45,5.4))
		for z in [-2.65,-1.8,0,1.8,2.65]: TSAssets.collider(ruin,Vector3(side*3.15,1.915,z),Vector3(.23,1.39,.22))
		TSAssets.collider(ruin,Vector3(side*2,.73,-2.7),Vector3(2.3,1,.22))
		TSAssets.collider(ruin,Vector3(side*2,2.83,-2.7),Vector3(2.3,.45,.22))
		for x in [side*.9,side*3.1]: TSAssets.collider(ruin,Vector3(x,1.915,-2.7),Vector3(.23,1.39,.24))
	TSAssets.collider(ruin,Vector3(0,1.63,2.7),Vector3(6.4,2.8,.22))
	var grass = MultiMeshInstance3D.new()
	grass.multimesh = MultiMesh.new()
	grass.multimesh.transform_format = MultiMesh.TRANSFORM_3D
	grass.multimesh.mesh = grass_mesh
	grass.multimesh.instance_count = 1400
	grass.material_override = grass_material
	grass.visibility_range_end = 65
	grass.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	root.add_child(grass)
	return {"root":root,"terrain":terrain,"shape":shape,"rails":rail,"sleepers":sleepers,"plants":plants,"grass":grass,"trunks":trunks,"ruin":ruin}

func _grass_mesh() -> ArrayMesh:
	var tool = SurfaceTool.new()
	tool.begin(Mesh.PRIMITIVE_TRIANGLES)
	for i in range(8):
		var angle = i*2.4
		var center = Vector3(cos(angle)*.3,0,sin(angle)*.3)
		var right = Vector3(cos(angle),0,sin(angle))*.035
		var tip = center+Vector3.UP*(.35+(i%3)*.1)
		for v in [center-right,tip,center+right]:
			tool.set_color(Color(1,1,1,1 if v==tip else 0))
			tool.set_normal(Vector3.UP)
			tool.add_vertex(v)
	return tool.commit()

func _fill_tile(tile:Dictionary,index:int) -> void:
	tile.root.visible = true
	tile.root.process_mode = Node.PROCESS_MODE_INHERIT
	var origin = route(index*TILE).origin
	tile.root.position = origin
	var surface = SurfaceTool.new()
	surface.begin(Mesh.PRIMITIVE_TRIANGLES)
	var vertices = PackedVector3Array()
	for z in range(16):
		for x in range(32):
			var points = []
			for p in [Vector2(x,z),Vector2(x,z+1),Vector2(x+1,z),Vector2(x+1,z+1)]:
				var wz = index*TILE+p.y*5
				var wx = route(wz).origin.x+(p.x-16)*12
				points.append(Vector3(wx,ground_height(wx,wz),wz)-origin)
			for v in [points[0],points[2],points[1],points[2],points[3],points[1]]:
				var at=v+origin
				var dx=ground_height(at.x+1,at.z)-ground_height(at.x-1,at.z)
				var dz=ground_height(at.x,at.z+1)-ground_height(at.x,at.z-1)
				surface.set_normal(Vector3(-dx,2,-dz).normalized())
				surface.add_vertex(v)
				vertices.append(v)
	tile.terrain.mesh = surface.commit()
	var shape = ConcavePolygonShape3D.new()
	shape.set_faces(vertices)
	tile.shape.shape = shape
	var rails = SurfaceTool.new()
	var sleepers = SurfaceTool.new()
	rails.begin(Mesh.PRIMITIVE_TRIANGLES)
	sleepers.begin(Mesh.PRIMITIVE_TRIANGLES)
	for segment in range(40):
		var pose = route(index*TILE+segment*2+1)
		pose.origin-=origin
		for side in [-.8,.8]:
			var t=pose
			t.origin+=pose.basis*Vector3(side,.22,0)
			_append_box(rails,t,Vector3(.12,.16,2.04))
		var t=pose
		t.origin+=Vector3.UP*.05
		_append_box(sleepers,t,Vector3(2.8,.16,.26))
	tile.rails.mesh=rails.commit()
	tile.sleepers.mesh=sleepers.commit()
	var ruin_distance=index*TILE+40
	var ruin_active=index>0 and index%3==1
	for stop in game.stops:
		if absf(ruin_distance-stop)<65: ruin_active=false
	var ruin_offset=36.0 if index%2==0 else -36.0
	tile.ruin.visible=ruin_active
	tile.ruin.process_mode=Node.PROCESS_MODE_INHERIT if ruin_active else Node.PROCESS_MODE_DISABLED
	var ruin_pose=route(ruin_distance)
	ruin_pose.origin+=ruin_pose.basis*Vector3(ruin_offset,0,0)
	ruin_pose.origin.y=ground_height(ruin_pose.origin.x,ruin_pose.origin.z)
	ruin_pose.origin-=origin
	ruin_pose.basis*=Basis(Vector3.UP,PI/2 if ruin_offset>0 else -PI/2)
	tile.ruin.transform=ruin_pose
	var rng=RandomNumberGenerator.new()
	rng.seed=abs(index*73856093)+9
	var trees={"SummerBirch":[],"SummerPine":[]}
	for species in trees:
		for i in range(TREES_PER_SPECIES):
			var distance=index*TILE+rng.randf_range(0,TILE)
			var offset=(1 if i%2==0 else -1)*rng.randf_range(17,68)
			for stop in game.stops:
				if absf(distance-stop)<55: offset=sign(offset)*rng.randf_range(48,68)
			if ruin_active and absf(distance-ruin_distance)<9 and absf(offset-ruin_offset)<8: offset=sign(offset)*(absf(ruin_offset)+12+rng.randf_range(0,8))
			var t=route(distance)
			t.origin+=t.basis*Vector3(offset,0,0)
			t.origin.y=ground_height(t.origin.x,t.origin.z)
			t.origin-=origin
			t.basis=Basis(Vector3.UP,rng.randf_range(0,TAU)).scaled(Vector3.ONE*rng.randf_range(.8,1.2))
			trees[species].append(t)
	var trunk_index=0
	for species in trees:
		for t in trees[species]:
			tile.trunks[trunk_index].transform=t
			trunk_index+=1
	for batch in tile.plants:
		for i in range(TREES_PER_SPECIES): batch.node.multimesh.set_instance_transform(i,trees[batch.species][i]*batch.part)
	for i in range(1400):
		var distance=index*TILE+rng.randf_range(0,TILE)
		var t=route(distance)
		var offset=(1 if i%2==0 else -1)*rng.randf_range(3,30)
		t.origin+=t.basis*Vector3(offset,0,0)
		t.origin.y=ground_height(t.origin.x,t.origin.z)
		t.origin-=origin
		t.basis=Basis(Vector3.UP,rng.randf_range(0,TAU))
		tile.grass.multimesh.set_instance_transform(i,t)

func _append_box(tool:SurfaceTool,t:Transform3D,size:Vector3) -> void:
	var p=[]
	for v in [Vector3(-1,-1,-1),Vector3(1,-1,-1),Vector3(1,1,-1),Vector3(-1,1,-1),Vector3(-1,-1,1),Vector3(1,-1,1),Vector3(1,1,1),Vector3(-1,1,1)]: p.append(t*(v*size*.5))
	for face in [[0,3,2,1],[4,5,6,7],[0,4,7,3],[1,2,6,5],[3,7,6,2],[0,1,5,4]]:
		var normal=(p[face[1]]-p[face[0]]).cross(p[face[2]]-p[face[0]]).normalized()
		for i in [0,2,1,0,3,2]:
			tool.set_normal(normal)
			tool.set_uv(Vector2(i%2,i/2))
			tool.add_vertex(p[face[i]])

func refresh(force:bool=false) -> void:
	var player_position=game.player.global_position if game.player else train.global_position
	var wanted=TSCore.chunk_window(floori(game.distance/TILE),floori(player_position.z/TILE))
	for index in active.keys():
		if index not in wanted:
			active[index].root.visible=false
			active[index].root.process_mode=Node.PROCESS_MODE_DISABLED
			pool.append(active[index])
			active.erase(index)
	for index in wanted:
		if not active.has(index):
			var tile=pool.pop_back()
			_fill_tile(tile,index)
			active[index]=tile
	for i in range(stations.size()):
		var station=stations[i]
		var d=minf(absf(game.distance-game.stops[i]),absf(player_position.z-game.stops[i]))
		var enabled=d<(500 if station.visible else 420)
		station.visible=enabled
		station.process_mode=Node.PROCESS_MODE_INHERIT if enabled else Node.PROCESS_MODE_DISABLED
		station.enabled=enabled

func _process(dt:float) -> void:
	update_clock+=dt
	if update_clock>.3:
		update_clock=0
		refresh()

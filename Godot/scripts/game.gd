class_name TSGame
extends Node3D

var world:TSWorld
var player:TSPlayer
var hud:TSHud
var projectiles:TSProjectiles
var environment:WorldEnvironment
var sun:DirectionalLight3D
var moon:DirectionalLight3D
var sky_material:ShaderMaterial
var settings:Dictionary={"english":false,"quality":1,"fps":false,"vsync":true,"limit":60,"sensitivity":.002,"fullscreen":false}
var bag:Dictionary=TSCore.new_inventory()
var stops:Array=TSCore.SHORT_STOPS.duplicate()
var long_route:bool=false
var distance:float=0
var speed:float=0
var fuel:float=30
var condition:float=60
var stop_index:int=0
var parked:bool=false
var brake:bool=false
var relay_mask:int=0
var remaining:float=0
var automatic:bool=true
var health:float=70
var hunger:float=90
var thirst:float=85
var time_of_day:float=.4
var daylight:float=1
var collected:Dictionary={}
var menu:String="start"
var playing:bool=false
var bag_open:bool=false
var status:String=""
var auto_clock:float=0
var sky_clock:float=0
var initial_enemy:Dictionary={}
var rail_audio:AudioStreamPlayer
var foot_audio:AudioStreamPlayer
var shot_audio:AudioStreamPlayer
var save_path:String="user://save_short.json"
const DWELL=180.0

func text(ru:String,en:String) -> String:
	return en if settings.english else ru

func _ready() -> void:
	process_mode=Node.PROCESS_MODE_ALWAYS
	long_route=bool(get_tree().get_meta("long_route",false)) or "--long-route" in OS.get_cmdline_user_args()
	stops=TSCore.LONG_STOPS.duplicate() if long_route else TSCore.SHORT_STOPS.duplicate()
	save_path="user://save_long.json" if long_route else "user://save_short.json"
	fuel=75 if long_route else 30
	_load_settings()
	_lighting()
	world=TSWorld.new()
	world.process_mode=Node.PROCESS_MODE_PAUSABLE
	add_child(world)
	world.setup(self)
	player=TSPlayer.new()
	player.process_mode=Node.PROCESS_MODE_PAUSABLE
	player.game=self
	player.position=Vector3(0,1.16,-2.7)
	player.rotation.y=PI
	add_child(player)
	projectiles=TSProjectiles.new()
	projectiles.process_mode=Node.PROCESS_MODE_PAUSABLE
	add_child(projectiles)
	projectiles.setup(self)
	for id in world.enemies: initial_enemy[id]={"position":world.enemies[id].global_position,"health":world.enemies[id].health}
	hud=TSHud.new()
	hud.game=self
	add_child(hud)
	_audio()
	apply_settings()
	set_menu("start")
	if get_tree().get_meta("auto_start",false):
		get_tree().remove_meta("auto_start")
		start_game.call_deferred()

func _unhandled_input(event:InputEvent) -> void:
	if event is InputEventKey and event.pressed and not event.echo:
		if event.physical_keycode==KEY_ESCAPE:
			if menu=="": set_menu("pause")
			elif menu=="pause": set_menu("")
			elif menu=="settings": set_menu("pause" if playing else "start")
			get_viewport().set_input_as_handled()
		if event.physical_keycode==KEY_F5 and menu=="": save_game(false)
		if event.physical_keycode==KEY_F9: load_game(false)
		if event.physical_keycode==KEY_F2:
			settings.fps=not settings.fps
			hud.update_hud()

func set_menu(value:String) -> void:
	menu=value
	if player:
		player.reset_aim()
		if value!="": player.bag_view.render_target_update_mode=SubViewport.UPDATE_DISABLED
	get_tree().paused=value!=""
	Input.mouse_mode=Input.MOUSE_MODE_VISIBLE if value!="" or bag_open else Input.MOUSE_MODE_CAPTURED
	if hud: hud.rebuild()

func choose_route(use_long:bool) -> void:
	if use_long==long_route: start_game();return
	get_tree().set_meta("long_route",use_long)
	get_tree().set_meta("auto_start",true)
	get_tree().reload_current_scene()

func start_game() -> void:
	bag=TSCore.new_inventory()
	distance=0
	speed=0
	fuel=75 if long_route else 30
	condition=60
	stop_index=0
	parked=false
	brake=false
	relay_mask=0
	remaining=0
	health=70
	hunger=90
	thirst=85
	time_of_day=.4
	collected.clear()
	bag_open=false
	world.train.transform=world.route(0)
	for id in world.pickups: _set_pickup(id,true)
	for id in world.boards: _restore_board(id,60)
	for id in world.enemies: _restore_enemy(id,initial_enemy[id].health,initial_enemy[id].position)
	player.global_position=world.train.to_global(Vector3(0,1.16,-2.7))
	player.rotation=Vector3(0,PI,0)
	player.pitch=0
	player.camera.rotation=Vector3.ZERO
	player.support=true
	player.aboard=true
	player.support_point=Vector3(0,1.16,-2.7)
	player.last_train=world.train.global_transform
	player.velocity=Vector3.ZERO
	projectiles.clear()
	auto_clock=0
	playing=true
	world.refresh(true)
	set_menu("")

func _physics_process(dt:float) -> void:
	if not playing or menu!="": return
	if fuel<=0 or condition<=0: brake=true
	var endpoint=15600.0 if long_route else 2200.0
	var next=float(stops[stop_index]) if stop_index<4 else endpoint
	var target=0.0 if brake or parked or fuel<=0 or condition<=0 else minf(8,sqrt(3*maxf(0,next-distance)))
	speed=move_toward(speed,target,1.5*dt)
	var step=speed*dt
	if not parked and distance+step>=next:
		step=maxf(0,next-distance)
		parked=true
		speed=0
		remaining=DWELL if stop_index<4 else 0
	distance+=step
	fuel=maxf(0,fuel-step*.015)
	condition=maxf(0,condition-step*.006)
	world.train.transform=world.route(distance)
	for body in world.train_bodies: body.force_update_transform()
	if parked and stop_index<4:
		remaining=maxf(0,remaining-dt)
		if automatic and remaining<=0 and player.aboard: depart()
	hunger=maxf(0,hunger-dt*.025)
	thirst=maxf(0,thirst-dt*.045)
	if hunger<=0 or thirst<=0: damage(dt*1.5)
	if health<=0: return
	time_of_day=fposmod(time_of_day+dt/1200,1)
	auto_clock+=dt
	if auto_clock>=120:
		auto_clock=0
		save_game(true)
	if distance>=endpoint and relay_mask==15:
		save_game(true)
		set_menu("victory")

func _process(dt:float) -> void:
	if menu=="":
		sky_clock+=dt
		if sky_clock>.25:
			sky_clock=0
			_update_lighting()
		if rail_audio:
			rail_audio.volume_db=linear_to_db(maxf(.001,speed/8*.25))
			rail_audio.pitch_scale=lerpf(.65,1.15,speed/8)

func depart() -> bool:
	if not player.aboard or fuel<=0 or condition<=0: return false
	if parked:
		if stop_index>=4 or (relay_mask & (1<<stop_index))==0: return false
		stop_index+=1
	parked=false
	brake=false
	save_game(true)
	return true

func damage(amount:float) -> void:
	if health<=0: return
	health=maxf(0,health-amount)
	if health<=0:
		bag_open=false
		player.bag_root.visible=false
		set_menu("death")

func hit(body:Node,amount:float) -> void:
	if body.has_meta("enemy"):
		body.damage(amount)
	elif body.has_meta("damage_id"):
		var id=body.get_meta("damage_id")
		_restore_board(id,maxf(0,world.boards[id].health-amount))
		if world.boards[id].health<=0:
			var index=int(id.get_slice("/",1))
			world.stations[index].bake_navigation_mesh.call_deferred(false)

func noise(point:Vector3,radius:float) -> void:
	if radius>=50 and shot_audio: shot_audio.play()
	elif radius<=6 and foot_audio: foot_audio.play()
	for enemy in world.enemies.values():
		if enemy.is_inside_tree() and enemy.health>0 and enemy.process_mode!=Node.PROCESS_MODE_DISABLED: enemy.hear(point,radius)

func interact(data:Dictionary) -> void:
	if data.is_empty(): return
	var kind=data.kind
	if kind=="cab":
		if parked:
			if not depart(): status=text("Забери модуль, вернись на борт и проверь топливо/ремонт.","Recover the relay, board the train and check fuel/repairs.")
		else: brake=not brake
		return
	if kind=="bench":
		use_item("kit")
		return
	if kind=="backpack": toggle_bag();return
	if kind=="relay":
		var mask=1<<int(data.index)
		if relay_mask & mask: status=text("Модуль уже забран.","Relay already recovered.")
		else:
			relay_mask|=mask
			status=text("Радиомодуль найден: ","Radio relay recovered: ")+str(_relays())+" / 4"
		return
	var id=data.id
	if collected.has(id): return
	var success=false
	if kind in TSCore.WEAPONS: success=TSCore.take_weapon(bag,kind)
	elif kind=="fuel": fuel+=int(data.amount);success=true
	elif kind=="crate":
		if bag.parts<=995 and bag.cloth<=997:
			bag.parts+=4;bag.cloth+=2;success=true
	else: success=TSCore.add_item(bag,kind,int(data.amount))
	if success:
		collected[id]=true
		_set_pickup(id,false)
		status=text("Предмет подобран.","Item collected.")
	else: status=text("Нет места или оружие уже есть.","No space, or weapon already owned.")

func use_item(kind:String) -> bool:
	if kind=="kit":
		if not player.aboard or speed>.1 or condition>=100:
			status=text("Останови поезд и вернись на борт для ремонта.","Stop and board the train to repair it.")
			return false
		if bag.kit<=0 and not TSCore.craft(bag,"kit"): return false
		bag.kit-=1
		condition=minf(100,condition+25)
		return true
	if kind not in ["water","food","medkit"] or bag[kind]<=0: return false
	if kind=="water" and thirst<100: thirst=minf(100,thirst+45)
	elif kind=="food" and hunger<100: hunger=minf(100,hunger+35)
	elif kind=="medkit" and health<100: health=minf(100,health+35)
	else: return false
	bag[kind]-=1
	return true

func toggle_bag() -> void:
	bag_open=not bag_open
	player.reset_aim()
	player.bag_root.visible=bag_open
	if not bag_open: player.camera.rotation.x=player.pitch
	Input.mouse_mode=Input.MOUSE_MODE_VISIBLE if bag_open else Input.MOUSE_MODE_CAPTURED
	hud.rebuild()

func item_name(kind:String) -> String:
	var names={"axe":["Топор","Axe"],"pistol":["Пистолет","Pistol"],"shotgun":["Дробовик","Shotgun"],"rifle":["Автомат","Rifle"],"water":["Вода","Water"],"food":["Еда","Food"],"medkit":["Аптечка","Medkit"],"kit":["Ремнабор","Repair kit"],"parts":["Детали","Parts"],"cloth":["Ткань","Cloth"],"pistol_ammo":["Пистолетные патроны","Pistol ammo"],"rifle_ammo":["Патроны автомата","Rifle ammo"],"shells":["Дробь","Shells"],"crate":["Ящик припасов","Supply crate"],"fuel":["Топливо","Fuel"],"relay":["Радиомодуль","Radio relay"],"cab":["Пульт поезда","Train controls"],"bench":["Верстак","Workbench"],"backpack":["Рюкзак","Backpack"]}
	return names.get(kind,[kind,kind])[1 if settings.english else 0]

func _relays() -> int:
	var count=0
	for i in range(4):
		if relay_mask & (1<<i): count+=1
	return count

func _set_pickup(id:String,active:bool) -> void:
	var node=world.pickups[id]
	node.visible=active
	node.process_mode=Node.PROCESS_MODE_INHERIT if active else Node.PROCESS_MODE_DISABLED
	for body in node.find_children("*","StaticBody3D",true,false): body.collision_layer=1 if active else 0

func _restore_board(id:String,value:float) -> void:
	world.boards[id].health=value
	var node=world.boards[id].node
	node.visible=value>0
	node.collision_layer=1 if value>0 else 0

func _restore_enemy(id:String,value:float,at:Vector3) -> void:
	var enemy=world.enemies[id]
	enemy.health=value
	enemy.global_position=at
	enemy.visible=value>0
	enemy.process_mode=Node.PROCESS_MODE_INHERIT if value>0 else Node.PROCESS_MODE_DISABLED
	enemy.collision_layer=4 if value>0 else 0
	enemy.alert=0
	enemy.velocity=Vector3.ZERO

func capture() -> Dictionary:
	var enemy_data={}
	var board_data={}
	for id in world.enemies: enemy_data[id]={"health":world.enemies[id].health,"position":TSCore.array(world.enemies[id].global_position)}
	for id in world.boards: board_data[id]=world.boards[id].health
	return {"schema":1,"long_route":long_route,"distance":distance,"speed":speed,"fuel":fuel,"condition":condition,"stop":stop_index,"parked":parked,"brake":brake,"relays":relay_mask,"remaining":remaining,"automatic":automatic,"health":health,"hunger":hunger,"thirst":thirst,"time":time_of_day,"aboard":player.aboard,"position":TSCore.array(world.train.to_local(player.global_position) if player.aboard else player.global_position),"yaw":player.rotation.y,"pitch":player.pitch,"flashlight":player.lamp.visible,"bag":bag.duplicate(true),"collected":collected.duplicate(),"enemies":enemy_data,"boards":board_data}

func validate(data) -> bool:
	if not data is Dictionary or data.get("schema")!=1 or data.get("long_route")!=long_route: return false
	for key in ["parked","brake","automatic","aboard","flashlight"]:
		if not data.get(key) is bool: return false
	for key in ["health","hunger","thirst","condition"]:
		if not TSCore.number(data.get(key),0,100): return false
	if not TSCore.number(data.get("distance"),0,15600 if long_route else 2200) or not TSCore.number(data.get("speed"),0,8.001) or not TSCore.number(data.get("fuel"),0,100000): return false
	if not TSCore.integer(data.get("stop"),0,4) or not TSCore.integer(data.get("relays"),0,15) or not TSCore.number(data.get("remaining"),0,DWELL): return false
	var stop=int(data.stop)
	var mask=int(data.relays)
	if (mask & ((1<<stop)-1))!=((1<<stop)-1): return false
	var next=float(stops[stop]) if stop<4 else (15600.0 if long_route else 2200.0)
	if data.distance>next+.02 or (stop>0 and data.distance<stops[stop-1]-.02): return false
	if data.parked and (absf(data.distance-next)>.02 or data.speed>.01): return false
	if not TSCore.number(data.get("time"),0,1) or not TSCore.number(data.get("pitch"),-1.39,1.39) or not TSCore.number(data.get("yaw"),-100000,100000): return false
	if not TSCore.position_array(data.get("position")) or not TSCore.valid_inventory(data.get("bag")): return false
	if data.aboard and (absf(float(data.position[0]))>3.2 or absf(float(data.position[2]))>6.5 or not TSCore.number(data.position[1],-.1,6)): return false
	if not data.get("collected") is Dictionary or not data.get("enemies") is Dictionary or not data.get("boards") is Dictionary: return false
	for id in data.collected:
		if not world.pickups.has(id) or data.collected[id]!=true: return false
	if data.enemies.size()!=world.enemies.size() or data.boards.size()!=world.boards.size(): return false
	for id in data.enemies:
		if not world.enemies.has(id) or not data.enemies[id] is Dictionary or not TSCore.number(data.enemies[id].get("health"),0,100) or not TSCore.position_array(data.enemies[id].get("position")): return false
	for id in data.boards:
		if not world.boards.has(id) or not TSCore.number(data.boards[id],0,60): return false
	return true

func apply(data:Dictionary) -> void:
	distance=float(data.distance);speed=float(data.speed);fuel=float(data.fuel);condition=float(data.condition)
	stop_index=int(data.stop);parked=data.parked;brake=data.brake;relay_mask=int(data.relays);remaining=float(data.remaining);automatic=data.automatic
	health=float(data.health);hunger=float(data.hunger);thirst=float(data.thirst);time_of_day=float(data.time)
	bag=data.bag.duplicate(true);collected=data.collected.duplicate();bag_open=false
	world.train.transform=world.route(distance)
	for body in world.train_bodies: body.force_update_transform()
	player.global_position=world.train.to_global(TSCore.vector(data.position)) if data.aboard else TSCore.vector(data.position)
	player.rotation.y=data.yaw;player.pitch=data.pitch;player.camera.rotation.x=data.pitch;player.lamp.visible=data.flashlight
	player.aboard=data.aboard;player.support=data.aboard;player.support_point=world.train.to_local(player.global_position);player.last_train=world.train.global_transform
	player.velocity=Vector3.ZERO;player.inherited=Vector3.ZERO;player.reset_aim();player.bag_root.visible=false
	for id in world.pickups: _set_pickup(id,not collected.has(id))
	for id in data.enemies: _restore_enemy(id,float(data.enemies[id].health),TSCore.vector(data.enemies[id].position))
	for id in data.boards: _restore_board(id,float(data.boards[id]))
	for station in world.stations: station.bake_navigation_mesh.call_deferred(false)
	projectiles.clear();world.refresh(true);_update_lighting();auto_clock=0

func save_game(auto:bool=false) -> bool:
	if not playing: return false
	var data=capture()
	if not validate(data): status=text("Состояние не прошло проверку сохранения.","State validation failed.");return false
	var payload=JSON.stringify(data)
	var envelope=JSON.stringify({"payload":payload,"sha256":payload.sha256_text()})
	var path=save_path.replace("save_","autosave_") if auto else save_path
	var file=FileAccess.open(path+".tmp",FileAccess.WRITE)
	if file==null: return false
	file.store_string(envelope);file.flush();file.close()
	if FileAccess.file_exists(path):
		if FileAccess.file_exists(path+".bak"): DirAccess.remove_absolute(path+".bak")
		if DirAccess.rename_absolute(path,path+".bak")!=OK: return false
	if DirAccess.rename_absolute(path+".tmp",path)!=OK:
		if FileAccess.file_exists(path+".bak"): DirAccess.rename_absolute(path+".bak",path)
		return false
	status=text("Прогресс сохранён.","Progress saved.")
	return true

func _read_save(path:String):
	if not FileAccess.file_exists(path): return null
	var parser=JSON.new()
	if parser.parse(FileAccess.get_file_as_string(path))!=OK: return null
	var envelope=parser.data
	if not envelope is Dictionary or not envelope.get("payload") is String or not envelope.get("sha256") is String: return null
	if envelope.payload.sha256_text()!=envelope.sha256: return null
	if parser.parse(envelope.payload)!=OK: return null
	var data=parser.data
	return data if validate(data) else null

func load_game(auto:bool=false) -> bool:
	var path=save_path.replace("save_","autosave_") if auto else save_path
	if not auto and not FileAccess.file_exists(path) and not FileAccess.file_exists(path+".bak"): path=save_path.replace("save_","autosave_")
	var data=_read_save(path)
	if data==null: data=_read_save(path+".bak")
	if data==null:
		status=text("Нет совместимого сохранения или резервной копии.","No compatible save or backup.")
		hud.update_hud();return false
	apply(data)
	playing=true
	set_menu("" if health>0 else "death")
	status=text("Прогресс загружен.","Progress loaded.")
	return true

func _lighting() -> void:
	environment=WorldEnvironment.new()
	var env=Environment.new()
	env.background_mode=Environment.BG_SKY
	var sky=Sky.new()
	sky_material=ShaderMaterial.new()
	sky_material.shader=load("res://shaders/sky.gdshader")
	sky.sky_material=sky_material
	env.sky=sky
	env.ambient_light_source=Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color=Color(.55,.64,.76)
	env.ambient_light_energy=.65
	env.tonemap_mode=Environment.TONE_MAPPER_ACES
	env.fog_enabled=true
	env.fog_light_color=Color(.67,.78,.87)
	env.fog_density=.002
	env.volumetric_fog_density=.003
	environment.environment=env
	add_child(environment)
	sun=DirectionalLight3D.new()
	sun.shadow_enabled=true
	sun.directional_shadow_max_distance=80
	sun.directional_shadow_mode=DirectionalLight3D.SHADOW_PARALLEL_2_SPLITS
	add_child(sun)
	moon=DirectionalLight3D.new()
	moon.light_color=Color(.55,.65,1)
	add_child(moon)
	_update_lighting()

func _update_lighting() -> void:
	var angle=time_of_day*TAU-PI*.5
	var height=sin(angle)
	daylight=smoothstep(-.08,.3,height)
	sun.rotation=Vector3(-angle,deg_to_rad(145),0)
	sun.light_energy=1.15*smoothstep(-.02,.25,height)
	sun.light_color=Color(1,.53,.28).lerp(Color(1,.96,.85),clampf(height*3,0,1))
	moon.rotation=Vector3(-angle-PI,deg_to_rad(145),0)
	moon.light_energy=.13*(1-daylight)
	sky_material.set_shader_parameter("daylight",daylight)
	sky_material.set_shader_parameter("sun_dir",sun.global_basis.z)
	environment.environment.ambient_light_color=Color(.025,.04,.08).lerp(Color(.48,.57,.66),daylight)
	environment.environment.ambient_light_energy=lerpf(.3,.45,daylight)
	environment.environment.fog_light_color=Color(.025,.04,.07).lerp(Color(.67,.78,.87),daylight)

func apply_settings() -> void:
	var quality=int(settings.quality)
	player.camera.far=[220,320,390][quality]
	sun.directional_shadow_max_distance=[45,80,110][quality]
	get_viewport().msaa_3d=Viewport.MSAA_DISABLED if quality==0 else Viewport.MSAA_2X if quality==1 else Viewport.MSAA_4X
	var forward=RenderingServer.get_current_rendering_method()=="forward_plus"
	get_viewport().use_occlusion_culling=true
	get_viewport().scaling_3d_scale=.85 if quality==0 else 1.0
	environment.environment.ssao_enabled=quality==2 and forward
	environment.environment.volumetric_fog_enabled=quality==2 and forward
	environment.environment.glow_enabled=quality>0
	for tile in world.active.values()+world.pool:
		tile.grass.multimesh.visible_instance_count=[500,1000,1400][quality]
		tile.grass.visibility_range_end=[45,65,80][quality]
	Engine.max_fps=int(settings.limit)
	if DisplayServer.get_name()!="headless":
		DisplayServer.window_set_vsync_mode(DisplayServer.VSYNC_ENABLED if settings.vsync else DisplayServer.VSYNC_DISABLED)
		DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_FULLSCREEN if settings.fullscreen else DisplayServer.WINDOW_MODE_WINDOWED)

func _load_settings() -> void:
	if not FileAccess.file_exists("user://settings.json"): return
	var data=JSON.parse_string(FileAccess.get_file_as_string("user://settings.json"))
	if not data is Dictionary: return
	for key in ["english","fps","vsync","fullscreen"]:
		if data.get(key) is bool: settings[key]=data[key]
	if TSCore.integer(data.get("quality"),0,2): settings.quality=int(data.quality)
	if data.get("limit") in [0,30,60,120,144]: settings.limit=int(data.limit)
	if TSCore.number(data.get("sensitivity"),.0005,.006): settings.sensitivity=float(data.sensitivity)

func save_settings() -> void:
	apply_settings()
	var file=FileAccess.open("user://settings.json",FileAccess.WRITE)
	if file: file.store_string(JSON.stringify(settings));file.close()

func _audio() -> void:
	if DisplayServer.get_name()=="headless": return
	if not ResourceLoader.exists("res://assets/train.wav"): return
	rail_audio=AudioStreamPlayer.new()
	rail_audio.process_mode=Node.PROCESS_MODE_PAUSABLE
	rail_audio.stream=load("res://assets/train.wav")
	if rail_audio.stream is AudioStreamWAV:
		rail_audio.stream.loop_mode=AudioStreamWAV.LOOP_FORWARD
		rail_audio.stream.loop_end=int(rail_audio.stream.get_length()*rail_audio.stream.mix_rate)
	add_child(rail_audio)
	rail_audio.volume_db=-60
	rail_audio.play()
	var air=AudioStreamPlayer.new()
	air.process_mode=Node.PROCESS_MODE_PAUSABLE
	air.stream=load("res://assets/summer.wav")
	air.stream.loop_mode=AudioStreamWAV.LOOP_FORWARD
	air.stream.loop_end=int(air.stream.get_length()*air.stream.mix_rate)
	air.volume_db=-8
	add_child(air)
	air.play()
	shot_audio=AudioStreamPlayer.new()
	shot_audio.stream=load("res://assets/shot.wav")
	shot_audio.max_polyphony=6
	shot_audio.volume_db=-12
	shot_audio.process_mode=Node.PROCESS_MODE_PAUSABLE
	add_child(shot_audio)
	foot_audio=AudioStreamPlayer.new()
	foot_audio.stream=load("res://assets/foot.wav")
	foot_audio.volume_db=-12
	foot_audio.process_mode=Node.PROCESS_MODE_PAUSABLE
	add_child(foot_audio)

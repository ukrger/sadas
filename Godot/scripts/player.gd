class_name TSPlayer
extends CharacterBody3D

var game
var camera:Camera3D
var lamp:SpotLight3D
var weapon:Node3D
var hands:Node3D
var current_weapon:String=""
var pitch:float=0
var aim:float=0
var recoil:float=0
var shot_at:float=0
var reload_at:float=0
var reloading:bool=false
var aboard:bool=true
var support:bool=true
var support_point:Vector3=Vector3(0,1.16,-2.7)
var last_train:Transform3D
var inherited:Vector3=Vector3.ZERO
var target:Dictionary={}
var noise_clock:float=0
var gun_nodes:Dictionary={}
var bag_root:Node3D
var bag_view:SubViewport
var bag_camera:Camera3D
var bag_ids:Dictionary={}
var footsteps_clock:float=0
var look_block_frames:int=0

func _ready() -> void:
	collision_layer=2
	collision_mask=1|4
	platform_floor_layers=0
	platform_on_leave=CharacterBody3D.PLATFORM_ON_LEAVE_DO_NOTHING
	var collider=CollisionShape3D.new()
	var shape=CapsuleShape3D.new()
	shape.radius=.28
	shape.height=1.8
	collider.shape=shape
	collider.position.y=.9
	add_child(collider)
	camera=Camera3D.new()
	camera.position.y=1.65
	camera.fov=75
	camera.near=.04
	camera.far=320
	camera.current=true
	add_child(camera)
	lamp=SpotLight3D.new()
	lamp.spot_range=25
	lamp.spot_angle=25
	lamp.light_energy=3
	lamp.visible=false
	camera.add_child(lamp)
	for kind in ["axe","pistol","shotgun","rifle"]:
		var pivot=Node3D.new()
		var visual=TSAssets.model({"axe":"SurvivalAxe","pistol":"ServicePistol","shotgun":"PumpShotgun","rifle":"AutomaticRifle"}[kind])
		visual.rotation.y=PI
		pivot.add_child(visual)
		if kind!="axe":
			var arms=TSAssets.model("PlayerHands")
			arms.rotation.y=PI
			if kind=="pistol":
				arms.position=Vector3(0,.07,.08)
				for mesh in arms.find_children("*","MeshInstance3D",true,false):
					if "HandLeft" in str(mesh.name): mesh.visible=false
			pivot.add_child(arms)
		for mesh in pivot.find_children("*","MeshInstance3D",true,false): mesh.cast_shadow=GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
		camera.add_child(pivot)
		pivot.visible=false
		gun_nodes[kind]=pivot
	_make_bag()
	last_train=game.world.train.global_transform

func _unhandled_input(event:InputEvent) -> void:
	if not game.playing or game.menu!="": return
	if event is InputEventMouseMotion and not game.bag_open and look_block_frames<=0:
		var sensitivity=game.settings.sensitivity*(1-.5*aim)
		rotate_y(-event.relative.x*sensitivity)
		pitch=clampf(pitch-event.relative.y*sensitivity,-1.39,1.39)
		camera.rotation.x=pitch
	if event is InputEventKey and event.pressed and not event.echo:
		if event.physical_keycode==KEY_TAB: game.toggle_bag()
		if event.physical_keycode==KEY_F and not game.bag_open: lamp.visible=not lamp.visible
		if event.physical_keycode==KEY_E and not game.bag_open: game.interact(target)
		if event.physical_keycode==KEY_R and not game.bag_open: start_reload()
		if event.physical_keycode in [KEY_1,KEY_2,KEY_3,KEY_4] and not game.bag_open:
			var key=TSCore.WEAPONS[event.physical_keycode-KEY_1]
			if key in game.bag.owned:
				game.bag.selected="" if game.bag.selected==key else key
				reset_aim()
	if event is InputEventMouseButton and event.button_index==MOUSE_BUTTON_LEFT and event.pressed:
		if game.bag_open: _pick_bag(event.position)
		else: fire()

func _physics_process(dt:float) -> void:
	if not game.playing or game.menu!="": return
	var train=game.world.train
	var train_pose=train.global_transform
	var had_support=support
	var movement=Vector2.ZERO
	if not game.bag_open:
		movement=Vector2(float(Input.is_physical_key_pressed(KEY_D))-float(Input.is_physical_key_pressed(KEY_A)),float(Input.is_physical_key_pressed(KEY_S))-float(Input.is_physical_key_pressed(KEY_W))).limit_length()
	var jumping=not game.bag_open and Input.is_physical_key_pressed(KEY_SPACE) and (support or is_on_floor())
	var platform_velocity=Vector3.ZERO
	if support:
		var carried=train_pose*support_point
		platform_velocity=(carried-global_position)/maxf(dt,.001)
		rotate_y(train_pose.basis.get_euler().y-last_train.basis.get_euler().y)
		if not jumping:
			# Flat train decks use a local attachment plus swept horizontal collision.
			# This avoids CharacterBody recovery fighting a teleported railcar floor.
			global_position=carried
			var horizontal=(global_basis*Vector3(movement.x,0,movement.y))*4*(1-.3*aim)*dt
			_try_step(horizontal)
			if horizontal.length_squared()>.000001:
				var collision=move_and_collide(horizontal)
				if collision:
					var slide=collision.get_remainder().slide(collision.get_normal())
					if slide.length_squared()>.000001: move_and_collide(slide)
			var floor_hit=_floor_hit()
			support=_is_train_floor(floor_hit)
			aboard=support
			if support:
				global_position.y=floor_hit.position.y+.002
				support_point=train.to_local(global_position)
				inherited=Vector3.ZERO
				velocity=Vector3.ZERO
			else:
				inherited=Vector3(platform_velocity.x,0,platform_velocity.z)
				velocity.y=-.5
			last_train=train_pose
			_footsteps(dt,movement.length()>.2)
			return
		inherited=Vector3(platform_velocity.x,0,platform_velocity.z)
		support=false
	var flat=global_basis*Vector3(movement.x,0,movement.y)*4*(1-.3*aim)
	if is_on_floor() and velocity.y<0: velocity.y=-.5
	if jumping: velocity.y=5.0
	velocity.y-=18*dt
	velocity.x=flat.x+inherited.x
	velocity.z=flat.z+inherited.z
	if is_on_floor() and not jumping: _try_step(Vector3(velocity.x,0,velocity.z)*dt)
	var falling=velocity.y
	var was_grounded=is_on_floor()
	move_and_slide()
	if not was_grounded and is_on_floor() and falling< -10: game.damage((-falling-10)*8)
	var hit=_floor_hit()
	support=velocity.y<=0 and _is_train_floor(hit) and (is_on_floor() or global_position.y-hit.position.y<.06)
	if support:
		support_point=train.to_local(global_position)
		inherited=Vector3.ZERO
	elif is_on_floor(): inherited=Vector3.ZERO
	elif had_support and not jumping: inherited=Vector3(platform_velocity.x,0,platform_velocity.z)
	aboard=support
	last_train=train_pose
	_footsteps(dt,movement.length()>.2 and is_on_floor())
	if global_position.y< -15:
		global_position=train.to_global(Vector3(0,1.16,-2.7))
		velocity=Vector3.ZERO
		support=true
		support_point=Vector3(0,1.16,-2.7)

func _floor_hit() -> Dictionary:
	var ray=PhysicsRayQueryParameters3D.create(global_position+Vector3.UP*.12,global_position-Vector3.UP*.35,1,[get_rid()])
	return get_world_3d().direct_space_state.intersect_ray(ray)

func _is_train_floor(hit:Dictionary) -> bool:
	if hit.is_empty(): return false
	var node=hit.collider
	while node!=null:
		if node==game.world.train: return true
		node=node.get_parent()
	return false

func _footsteps(dt:float,moving:bool) -> void:
	footsteps_clock-=dt
	if moving and footsteps_clock<=0:
		game.noise(global_position,6)
		footsteps_clock=.5

func _process(dt:float) -> void:
	if not game.playing or game.menu!="": return
	look_block_frames=maxi(0,look_block_frames-1)
	var selected=game.bag.selected
	if selected!=current_weapon:
		current_weapon=selected
		reset_aim()
	for key in gun_nodes: gun_nodes[key].visible=key==selected and not game.bag_open
	if game.bag_open:
		camera.rotation.x=lerp_angle(camera.rotation.x,deg_to_rad(-58),minf(1,dt*8))
		_refresh_bag()
		return
	camera.rotation.x=pitch
	bag_root.visible=false
	bag_view.render_target_update_mode=SubViewport.UPDATE_DISABLED
	var now=Time.get_ticks_msec()/1000.0
	if reloading and now>=reload_at:
		TSCore.reload_weapon(game.bag,current_weapon)
		if current_weapon=="shotgun" and game.bag.magazines.shotgun<5 and game.bag.shells>0: reload_at=now+.65
		else: reloading=false
	var aiming=selected in TSCore.CAPACITY and Input.is_mouse_button_pressed(MOUSE_BUTTON_RIGHT) and not reloading
	aim=TSCore.aim_step(aim,dt,aiming)
	camera.fov=lerpf(75,58,aim)
	if selected in gun_nodes:
		var pivot=gun_nodes[selected]
		var rest=Vector3(.24,-.3,-.55)
		if selected=="axe": rest=Vector3(.3,-.62,-.64)
		if selected in ["shotgun","rifle"]: rest=Vector3(.17,-.25,-.4)
		var marker={"pistol":Vector3(0,.226,.035),"shotgun":Vector3(0,.2075,-.08),"rifle":Vector3(0,.2465,-.19),"axe":Vector3.ZERO}[selected]
		var aimed=Vector3(0,0,-.23)-marker
		recoil=move_toward(recoil,0,dt*6)
		pivot.position=rest.lerp(aimed,aim)+Vector3(0,recoil*.02,recoil*.07)*lerpf(1,.45,aim)+Vector3.DOWN*(.15 if reloading else 0)
		pivot.rotation=Vector3(deg_to_rad(recoil*10),0,deg_to_rad(18 if reloading else 0))
	if selected=="rifle" and Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT): fire()
	var query=PhysicsRayQueryParameters3D.create(camera.global_position,camera.global_position-camera.global_basis.z*3.2,1,[get_rid()])
	var hit=get_world_3d().direct_space_state.intersect_ray(query)
	target=hit.collider.get_meta("interaction",{}) if not hit.is_empty() else {}
	game.hud.update_hud()

func _try_step(horizontal:Vector3) -> void:
	if horizontal.length_squared()<.000001 or not test_move(global_transform,horizontal): return
	var raised=global_transform
	raised.origin+=Vector3.UP*.32
	if test_move(global_transform,Vector3.UP*.32) or test_move(raised,horizontal): return
	var top=raised.origin+horizontal+horizontal.normalized()*.32+Vector3.UP*.02
	var query=PhysicsRayQueryParameters3D.create(top,top-Vector3.UP*.38,1,[get_rid()])
	var hit=get_world_3d().direct_space_state.intersect_ray(query)
	if not hit.is_empty() and hit.normal.y>.7 and hit.position.y>global_position.y+.02:
		global_position.y=hit.position.y+.01

func reset_aim() -> void:
	look_block_frames=2
	aim=0
	recoil=0
	reloading=false
	if camera: camera.fov=75
	shot_at=Time.get_ticks_msec()/1000.0+.15

func start_reload() -> void:
	if current_weapon not in TSCore.CAPACITY or reloading: return
	if game.bag.magazines[current_weapon]>=TSCore.CAPACITY[current_weapon] or game.bag[TSCore.AMMO[current_weapon]]<=0: return
	reloading=true
	reload_at=Time.get_ticks_msec()/1000.0+(.65 if current_weapon=="shotgun" else 1.4 if current_weapon=="pistol" else 2.1)

func fire() -> void:
	if not game.playing or game.menu!="" or game.bag_open: return
	var now=Time.get_ticks_msec()/1000.0
	if now<shot_at: return
	var forward=-camera.global_basis.z
	if current_weapon=="axe":
		var hit=get_world_3d().direct_space_state.intersect_ray(PhysicsRayQueryParameters3D.create(camera.global_position,camera.global_position+forward*1.9,1|4,[get_rid()]))
		if not hit.is_empty(): game.hit(hit.collider,25)
		shot_at=now+.65
		recoil=1
		game.noise(global_position,9)
		return
	if current_weapon not in TSCore.CAPACITY or (reloading and current_weapon!="shotgun") or game.bag.magazines[current_weapon]<=0: return
	var count=8 if current_weapon=="shotgun" else 1
	if game.projectiles.free_slots()<count: return
	var spread=lerpf(3.2,2.0,aim) if current_weapon=="shotgun" else lerpf(1.15,.18,aim) if current_weapon=="rifle" else lerpf(.65,.12,aim)
	for pellet in range(count):
		var disk=Vector2.from_angle(randf()*TAU)*sqrt(randf())*tan(deg_to_rad(spread))
		var direction=(forward+camera.global_basis.x*disk.x+camera.global_basis.y*disk.y).normalized()
		game.projectiles.fire(camera.global_position+forward*.10,direction,230 if current_weapon=="shotgun" else 430 if current_weapon=="rifle" else 280,12 if current_weapon=="shotgun" else 26 if current_weapon=="rifle" else 30)
	TSCore.spend_round(game.bag,current_weapon)
	reloading=false
	shot_at=now+(.95 if current_weapon=="shotgun" else .11 if current_weapon=="rifle" else .22)
	recoil=1
	pitch=clampf(pitch+deg_to_rad(2.2 if current_weapon=="shotgun" else .55 if current_weapon=="rifle" else .8)*lerpf(1,.75,aim),-1.39,1.39)
	game.noise(global_position,80)

func _make_bag() -> void:
	bag_view=SubViewport.new()
	bag_view.own_world_3d=true
	bag_view.transparent_bg=true
	bag_view.size=Vector2i(1280,720)
	bag_view.render_target_update_mode=SubViewport.UPDATE_DISABLED
	add_child(bag_view)
	bag_camera=Camera3D.new()
	bag_camera.position=Vector3(0,1.59,.95)
	bag_camera.rotation.x=deg_to_rad(-58)
	bag_camera.fov=75
	bag_camera.near=.02
	bag_camera.far=6
	bag_camera.current=true
	bag_view.add_child(bag_camera)
	var lighting=WorldEnvironment.new()
	var env=Environment.new()
	env.background_mode=Environment.BG_CLEAR_COLOR
	env.ambient_light_source=Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color=Color(.7,.75,.8)
	env.ambient_light_energy=.6
	env.tonemap_mode=Environment.TONE_MAPPER_ACES
	lighting.environment=env
	bag_view.add_child(lighting)
	var light=DirectionalLight3D.new()
	light.rotation=Vector3(-.9,-.6,0)
	light.light_energy=1.1
	bag_view.add_child(light)
	bag_root=Node3D.new()
	bag_view.add_child(bag_root)
	TSAssets.box(bag_root,Vector3.ZERO,Vector3(2.6,.025,1.8),TSAssets.surface("Fabric",Color(.29,.23,.15)),false)
	var keys=["water","food","medkit","kit","pistol_ammo","shells","rifle_ammo","parts","cloth","axe","pistol","shotgun","rifle"]
	var names=["WaterBottle","CannedFood","FieldMedkit","RepairKit","AmmoBox","AmmoBox","AmmoBox","","","SurvivalAxe","ServicePistol","PumpShotgun","AutomaticRifle"]
	for i in range(keys.size()):
		var key=keys[i]
		var root=Node3D.new()
		root.position=Vector3(-1.05+(i%7)*.35,.02,-.60)
		var hit_size=Vector3(.35,.4,.28)
		var hit_center=Vector3(0,.20,0)
		if key in ["shotgun","rifle"]:
			root.scale=Vector3.ONE*.9
			root.rotation.y=PI/2
			root.position=Vector3(-.9,.06,.27 if key=="shotgun" else -.1)
			hit_size=Vector3(.14,.30,1.2)
			hit_center=Vector3(0,.08,.25)
		elif key=="axe":
			root.rotation.x=-PI/2
			root.position=Vector3(.8,.08,.42)
			hit_size=Vector3(.45,.85,.15)
			hit_center=Vector3(-.1,.4,0)
		elif key=="pistol":
			root.rotation.z=PI/2
			root.position=Vector3(.3,.09,.20)
			hit_size=Vector3(.08,.26,.28)
			hit_center=Vector3(0,.12,.04)
		elif key in ["parts","cloth"]:
			root.position=Vector3(.05 if key=="parts" else .48,.03,-.12)
			hit_size=Vector3(.26,.1,.24)
			hit_center=Vector3(0,.04,0)
		bag_root.add_child(root)
		if names[i]!="": root.add_child(TSAssets.model(names[i]))
		else: TSAssets.box(root,Vector3(0,.02,0),Vector3(.22,.04,.20),TSAssets.surface("Steel" if key=="parts" else "Fabric",Color(.4,.4,.34) if key=="parts" else Color(.42,.31,.19)),false)
		var area=Area3D.new()
		area.name="BagPickArea"
		area.collision_layer=8
		area.collision_mask=0
		area.set_meta("bag_key",key)
		var collider=CollisionShape3D.new()
		var shape=BoxShape3D.new()
		shape.size=hit_size
		collider.shape=shape
		collider.position=hit_center
		area.add_child(collider)
		root.add_child(area)
		bag_ids[key]=root
	bag_root.visible=false

func _refresh_bag() -> void:
	bag_root.visible=true
	bag_view.size=Vector2i(get_viewport().get_visible_rect().size)
	bag_view.render_target_update_mode=SubViewport.UPDATE_ALWAYS
	for key in bag_ids:
		bag_ids[key].visible=key in game.bag.owned if key in TSCore.WEAPONS else game.bag.get(key,0)>0
		bag_ids[key].get_node("BagPickArea").collision_layer=8 if bag_ids[key].visible else 0

func _pick_bag(screen:Vector2) -> void:
	var origin=bag_camera.project_ray_origin(screen)
	var direction=bag_camera.project_ray_normal(screen)
	var query=PhysicsRayQueryParameters3D.create(origin,origin+direction*5,8)
	query.collide_with_areas=true
	query.collide_with_bodies=false
	var hit=bag_camera.get_world_3d().direct_space_state.intersect_ray(query)
	if hit.is_empty(): return
	var selected=hit.collider.get_meta("bag_key","")
	if selected in TSCore.WEAPONS:
		game.bag.selected=selected
	elif selected!="": game.use_item(selected)

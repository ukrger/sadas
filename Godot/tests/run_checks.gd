extends SceneTree
var passed:int=0
func check(condition:bool,label:String) -> void:
	if not condition:
		push_error("CHECK FAILED: "+label)
		quit(1)
		assert(condition,label)
	passed+=1
	print("PASS "+label)
func _initialize() -> void:
	create_timer(60,true).timeout.connect(func():push_error("Test watchdog timeout");quit(1))
	_run.call_deferred()
func _run() -> void:
	var bag=TSCore.new_inventory()
	check(TSCore.valid_inventory(bag),"empty inventory valid")
	check(not TSCore.craft(bag,"kit") and bag.parts==0,"craft failure is atomic")
	check(TSCore.add_item(bag,"parts",10) and TSCore.add_item(bag,"cloth",10),"resource pickups")
	check(TSCore.craft(bag,"kit") and bag.parts==7 and bag.cloth==9 and bag.kit==1,"repair recipe exact costs")
	check(TSCore.craft(bag,"medkit") and bag.parts==6 and bag.cloth==7,"medkit recipe exact costs")
	check(not TSCore.add_item(bag,"parts",-1) and not TSCore.add_item(bag,"unknown",1),"invalid pickups rejected")
	for gun in TSCore.CAPACITY:
		check(TSCore.take_weapon(bag,gun) and not TSCore.take_weapon(bag,gun),gun+" ownership idempotent")
		TSCore.add_item(bag,TSCore.AMMO[gun],40)
		var total=bag.magazines[gun]+bag[TSCore.AMMO[gun]]
		TSCore.spend_round(bag,gun)
		check(TSCore.reload_weapon(bag,gun)==1 and bag.magazines[gun]+bag[TSCore.AMMO[gun]]==total-1,gun+" reload conserves ammo")
		for i in range(TSCore.CAPACITY[gun]): TSCore.spend_round(bag,gun)
		check(not TSCore.spend_round(bag,gun),gun+" empty cannot fire")
	check(TSCore.valid_inventory(bag),"armed inventory round trip valid")
	var invalid=bag.duplicate(true)
	invalid.magazines.rifle=31
	check(not TSCore.valid_inventory(invalid),"invalid magazine rejected")
	invalid=bag.duplicate(true)
	invalid.selected="not-owned"
	check(not TSCore.valid_inventory(invalid),"unowned selection rejected")
	check(TSCore.chunk_window(0,0).size()==7,"normal seven-tile window")
	check(TSCore.chunk_window(0,-40).size()==10 and -40 in TSCore.chunk_window(0,-40),"stranded player protected")
	var bounded=true
	for i in range(-100,100): bounded=bounded and TSCore.chunk_window(i,-i).size()<=10
	check(bounded,"chunk pool bounded across teleports")
	var one=TSCore.aim_step(0,.3,true)
	var many=0.0
	for i in range(30): many=TSCore.aim_step(many,.01,true)
	check(absf(one-many)<.000001 and one>.99,"aim frame subdivision independence")
	check(TSCore.aim_step(0,0,true)==0 and TSCore.aim_step(.5,-1,true)==.5,"pause and invalid aim time")
	var scene=load("res://scenes/main.tscn").instantiate()
	root.add_child(scene)
	await process_frame
	check(paused and scene.menu=="start","start screen pauses tree")
	check(scene.world.stations.size()==4 and scene.world.enemies.size()==8,"four stations and eight enemies instantiated")
	check(scene.world.active.size()==7 and scene.world.pool.size()==3,"ten prewarmed chunks")
	check(scene.world.parts.size()==6,"both tree species with three LODs imported")
	for name in ["SummerRailcar","ServicePistol","PumpShotgun","AutomaticRifle","PlayerHands","StationZombie"]:
		var model=TSAssets.model(name)
		var count=0
		for node in model.find_children("*","MeshInstance3D",true,false):
			if node.visible: count+=1
		check(count>0,name+" visible LOD0 loaded")
		model.free()
	scene.save_path="user://test_"+str(Time.get_ticks_usec())+".json"
	scene.start_game()
	check(not paused and scene.playing,"new game resumes simulation")
	check(scene.validate(scene.capture()),"initial scene snapshot validates")
	for frame in range(30): await physics_frame
	check(scene.player.global_position.y>1.0 and scene.player.global_position.y<1.4,"player supported by train floor")
	check(scene.player.aboard,"moving train support detected")
	check(scene.distance>0,"train advances under physics")
	var local_before=scene.world.train.to_local(scene.player.global_position)
	for frame in range(360): await physics_frame
	var local_after=scene.world.train.to_local(scene.player.global_position)
	print("CARRY MEASUREMENT local displacement=",local_before.distance_to(local_after)," m after 6 seconds; speed=",scene.speed)
	check(Vector2(local_before.x-local_after.x,local_before.z-local_after.z).length()<.05 and scene.speed>7.9,"player remains on curved train at cruise speed")
	var jump_key=InputEventKey.new()
	jump_key.physical_keycode=KEY_SPACE
	jump_key.pressed=true
	Input.parse_input_event(jump_key)
	for frame in range(2): await physics_frame
	jump_key.pressed=false
	Input.parse_input_event(jump_key)
	check(scene.player.velocity.y>0 and scene.player.velocity.z>6.5,"jump inherits train launch velocity")
	for frame in range(50): await physics_frame
	check(scene.player.aboard,"jump lands back on moving train")
	var ground_at=scene.world.route(300).origin+Vector3(5,4,0)
	var ground_hit=scene.player.get_world_3d().direct_space_state.intersect_ray(PhysicsRayQueryParameters3D.create(ground_at,ground_at-Vector3.UP*8,1))
	check(not ground_hit.is_empty() and ground_hit.normal.y>.7,"terrain has upward-facing physical surface")
	check(scene.world.stations[0].navigation_mesh.get_polygon_count()>0,"station NavMesh actually baked")
	var board=scene.world.boards["boards/0"].node
	var origin=board.global_position-board.global_basis.z*2
	var direction=board.global_basis.z
	for i in range(8): scene.projectiles.fire(origin,direction,280,5)
	for frame in range(5): await physics_frame
	check(scene.world.boards["boards/0"].health==20,"eight actual pooled pellets apply damage")
	scene._restore_board("boards/0",60)
	var wall=TSAssets.collider(scene,Vector3.ZERO,Vector3(3,3,.2))
	wall.global_transform=Transform3D(board.global_basis,board.global_position-board.global_basis.z)
	await physics_frame
	scene.projectiles.fire(origin,direction,280,25)
	for frame in range(5): await physics_frame
	check(scene.world.boards["boards/0"].health==60,"solid wall blocks actual bullet damage")
	wall.queue_free()
	await physics_frame
	var snapshot=scene.capture()
	check(scene.save_game(false),"actual JSON save writes and validates")
	scene.bag.parts=99
	scene.health=13
	check(scene.load_game(false) and scene.bag.parts==snapshot.bag.parts and scene.health==snapshot.health,"file load restores real scene state")
	TSCore.take_weapon(scene.bag,"rifle")
	var aim_button=InputEventMouseButton.new()
	aim_button.button_index=MOUSE_BUTTON_RIGHT
	aim_button.pressed=true
	Input.parse_input_event(aim_button)
	for frame in range(24): await process_frame
	check(scene.player.aim>.98 and scene.player.camera.fov<59,"actual input enters ADS and narrows camera FOV")
	var sight=scene.player.gun_nodes.rifle.transform*Vector3(0,.2465,-.19)
	check(sight.distance_to(Vector3(0,0,-.23))<.01,"rifle sight aligns with camera center")
	for gun in ["pistol","shotgun"]:
		TSCore.take_weapon(scene.bag,gun)
		for frame in range(24): await process_frame
		var marker=Vector3(0,.226,.035) if gun=="pistol" else Vector3(0,.2075,-.08)
		var point=scene.player.gun_nodes[gun].transform*marker
		check(scene.player.aim>.98 and point.distance_to(Vector3(0,0,-.23))<.01,gun+" sight aligns after weapon switch")
	scene.bag.selected="rifle"
	for frame in range(24): await process_frame
	aim_button.pressed=false
	Input.parse_input_event(aim_button)
	for frame in range(24): await process_frame
	check(scene.player.aim<.01 and scene.player.camera.fov>74.8,"releasing RMB restores hip view")
	TSCore.spend_round(scene.bag,"rifle")
	TSCore.add_item(scene.bag,"rifle_ammo",10)
	scene.player.start_reload()
	aim_button.pressed=true
	Input.parse_input_event(aim_button)
	for frame in range(12): await process_frame
	check(scene.player.reloading and scene.player.aim<.01,"reload blocks ADS")
	aim_button.pressed=false
	Input.parse_input_event(aim_button)
	# Restore the known saved state after moving the live train far away.
	scene.distance=1000
	scene.world.train.transform=scene.world.route(1000)
	scene.player.global_position=scene.world.train.to_global(Vector3(0,1.16,-2.7))
	check(scene.load_game(false),"save loads after a large train teleport")
	for frame in range(30): await physics_frame
	var restored_local=scene.world.train.to_local(scene.player.global_position)
	check(restored_local.distance_to(TSCore.vector(snapshot.position))<.05 and scene.player.aboard,"teleport load retains passenger support without launch impulse")
	check(scene.save_game(false),"second save rotates backup")
	var damaged=FileAccess.open(scene.save_path,FileAccess.WRITE)
	damaged.store_string("broken")
	damaged.close()
	check(scene.load_game(false),"corrupt primary falls back to backup")
	var before=scene.capture()
	var bad=before.duplicate(true)
	bad.bag.magazines.rifle=999
	check(not scene.validate(bad) and scene.health==before.health,"invalid restore rejected before scene mutation")
	bad=before.duplicate(true)
	bad.stop=1
	bad.relays=0
	check(not scene.validate(bad),"departed station must retain recovered relay")
	scene.distance=160
	scene.speed=0
	scene.parked=true
	scene.remaining=0
	scene.player.aboard=true
	check(not scene.depart(),"departure gated by station relay")
	scene.interact({"kind":"relay","index":0,"id":"relay0"})
	scene.interact({"kind":"relay","index":0,"id":"relay0"})
	check(scene.relay_mask==1 and scene._relays()==1,"radio collection idempotent")
	scene.player.aboard=false
	check(not scene.depart(),"train waits for player")
	scene.player.aboard=true
	check(scene.depart() and scene.stop_index==1,"relay and player permit departure")
	scene.toggle_bag()
	check(scene.bag_open and not paused and scene.player.bag_root.visible,"3D backpack opens without pausing world")
	check(scene.player.bag_camera.get_world_3d()!=scene.player.camera.get_world_3d(),"inventory uses an isolated world to prevent clipping into train furniture")
	scene.bag.water=1
	scene.thirst=50
	for frame in range(3): await physics_frame
	var bottle=scene.player.bag_ids.water
	var screen=scene.player.bag_camera.unproject_position(bottle.to_global(Vector3(0,.15,0)))
	var thirst_before=scene.thirst
	scene.player._pick_bag(screen)
	print("BAG PICK water=",scene.bag.water," thirst before=",thirst_before," after=",scene.thirst)
	check(scene.bag.water==0 and absf(scene.thirst-minf(100,thirst_before+45))<.0001,"clicking a 3D bag object uses the actual item")
	scene.toggle_bag()
	check(not scene.bag_open and scene.player.camera.fov==75,"closing backpack resets aim and FOV")
	scene.player.global_position=Vector3(0,1,-2000)
	scene.world.refresh(true)
	check(scene.world.active.size()==10 and scene.world.pool.is_empty(),"player far from train receives reserve chunks")
	scene.projectiles.clear()
	for i in range(57): scene.projectiles.fire(Vector3.ZERO,Vector3.FORWARD,230,12)
	check(scene.projectiles.free_slots()==7,"projectile pool reports capacity for whole shotgun volley")
	scene.projectiles.clear()
	var target=scene.world.boards["boards/0"].node
	scene.hit(target,60)
	check(scene.world.boards["boards/0"].health==0 and target.collision_layer==0,"destroyed boards stop blocking collisions")
	# Walk onto a 0.30 m step on the real terrain without jumping.
	scene.player.global_position=scene.world.route(300).origin+Vector3(5,.02,-2)
	scene.player.rotation.y=PI
	scene.player.velocity=Vector3.ZERO
	scene.player.support=false
	scene.player.aboard=false
	scene.world.refresh(true)
	var step=TSAssets.collider(scene,scene.world.route(300).origin+Vector3(5,.07,1),Vector3(2,.30,2))
	for frame in range(10): await physics_frame
	var key=InputEventKey.new()
	key.physical_keycode=KEY_W
	key.pressed=true
	Input.parse_input_event(key)
	for frame in range(48): await physics_frame
	key.pressed=false
	Input.parse_input_event(key)
	check(scene.player.global_position.z>300 and scene.player.global_position.y>.18,"walk onto a step without jumping")
	step.queue_free()
	scene.set_menu("pause")
	var paused_distance=scene.distance
	for frame in range(8): await physics_frame
	check(scene.distance==paused_distance,"pause freezes train simulation")
	check(paused and scene.player.aim==0,"pause resets ADS")
	for path in [scene.save_path,scene.save_path+".bak",scene.save_path+".tmp"]:
		if FileAccess.file_exists(path): DirAccess.remove_absolute(path)
	var auto_path=scene.save_path.replace("save_","autosave_")
	for path in [auto_path,auto_path+".bak"]:
		if FileAccess.file_exists(path): DirAccess.remove_absolute(path)
	print("RESULT: ",passed," checks passed in Godot ",Engine.get_version_info().string,". Headless tests do not verify GPU performance.")
	paused=false
	for node in scene.find_children("*","AudioStreamPlayer",true,false): node.stop()
	scene.queue_free()
	TSAssets.scenes.clear()
	TSAssets.surfaces.clear()
	for frame in range(8): await process_frame
	quit(0)

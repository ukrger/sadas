extends SceneTree
var passed:int=0
func check(value:bool,label:String) -> void:
	if not value: push_error("FAILED "+label);quit(1);assert(value,label)
	passed+=1
	print("PASS "+label)
func _initialize() -> void:
	_run.call_deferred()
func _run() -> void:
	var game=load("res://scenes/main.tscn").instantiate()
	root.add_child(game)
	await process_frame
	check(game.long_route and game.stops[0]==2800 and game.save_path=="user://save_long.json","long route selects separate profile and station distances")
	check(game.world.stations[0].navigation_mesh.get_polygon_count()>0,"distant long-route station NavMesh baked")
	game.save_path="user://long_check_"+str(Time.get_ticks_usec())+".json"
	game.start_game()
	var simulated=0.0
	while not game.parked and simulated<400:
		game._physics_process(.1)
		simulated+=.1
	check(game.parked and absf(game.distance-2800)<.001 and game.speed==0 and simulated>345 and simulated<365,"real train tick reaches first long stop in approximately six minutes")
	check(absf(game.fuel-33)<.02 and absf(game.condition-43.2)<.02,"long first leg consumes distance-based fuel and wear")
	game.player.global_position=game.world.train.to_global(Vector3(0,1.152,-2.7))
	game.player.support_point=Vector3(0,1.152,-2.7)
	game.player.last_train=game.world.train.global_transform
	game.player.aboard=true
	check(not game.depart(),"long route also waits for its radio module")
	game.interact({"kind":"relay","index":0,"id":"relay0"})
	check(game.depart() and game.stop_index==1 and game.validate(game.capture()),"long-route departure and save contract valid")
	for path in [game.save_path,game.save_path+".bak"]:
		if FileAccess.file_exists(path):DirAccess.remove_absolute(path)
	print("RESULT: ",passed," long-route checks passed; schedule tick simulation, not a full real-time playthrough.")
	paused=false
	game.queue_free()
	TSAssets.scenes.clear()
	TSAssets.surfaces.clear()
	for frame in range(4):await process_frame
	quit(0)

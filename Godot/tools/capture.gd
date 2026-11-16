extends SceneTree
var game
func _initialize() -> void:
	create_timer(240,true).timeout.connect(func():push_error("Capture watchdog timeout");quit(1))
	_run.call_deferred()
func _run() -> void:
	game=load("res://scenes/main.tscn").instantiate()
	root.add_child(game)
	for i in range(6): await process_frame
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://previews/GodotMenu.png")
	game.start_game()
	game.distance=160
	game.speed=0
	game.parked=true
	game.remaining=180
	game.world.train.transform=game.world.route(160)
	game.player.global_position=game.world.train.to_global(Vector3(0,1.16,-2.7))
	game.player.support_point=Vector3(0,1.16,-2.7)
	game.player.last_train=game.world.train.global_transform
	game.world.refresh(true)
	game.player.rotation.y=-PI*.5
	for i in range(6): await process_frame
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://previews/GodotTrainView.png")
	# A diagnostic overview from the actual world, using the real scene meshes/materials/shaders.
	game.player.process_mode=Node.PROCESS_MODE_DISABLED
	game.player.camera.global_position=game.world.route(160)*Vector3(25,13,-24)
	game.player.camera.look_at(game.world.route(160)*Vector3(7,1,0))
	for i in range(4): await process_frame
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://previews/GodotOverview.png")
	game.player.process_mode=Node.PROCESS_MODE_PAUSABLE
	game.player.position=game.world.route(160)*Vector3(3.2,.75,-4.5)
	game.player.rotation=Vector3(0,-PI/2,0)
	game.player.camera.position=Vector3(0,1.65,0)
	game.player.camera.rotation=Vector3.ZERO
	game.player.pitch=0
	game.player.support=false
	game.player.velocity=Vector3.ZERO
	TSCore.take_weapon(game.bag,"rifle")
	var button=InputEventMouseButton.new()
	button.button_index=MOUSE_BUTTON_RIGHT
	button.pressed=true
	Input.parse_input_event(button)
	for i in range(12): await process_frame
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://previews/GodotAim.png")
	button.pressed=false
	Input.parse_input_event(button)
	for kind in ["axe","pistol","shotgun"]: TSCore.take_weapon(game.bag,kind)
	for kind in TSCore.ITEMS: game.bag[kind]=4
	game.toggle_bag()
	for i in range(12): await process_frame
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://previews/GodotBackpack.png")
	game.toggle_bag()
	game.player.process_mode=Node.PROCESS_MODE_DISABLED
	game.player.camera.global_position=game.world.route(160)*Vector3(25,13,-24)
	game.player.camera.look_at(game.world.route(160)*Vector3(7,1,0))
	game.settings.quality=2
	game.apply_settings()
	game.time_of_day=0
	game._update_lighting()
	for i in range(4): await process_frame
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://previews/GodotNightHigh.png")
	print("CAPTURE COMPLETE: actual Godot renderer ",RenderingServer.get_current_rendering_method()," / ",RenderingServer.get_video_adapter_name())
	game.playing=false
	for audio in game.find_children("*","AudioStreamPlayer",true,false):
		audio.stop()
		audio.stream=null
	for frame in range(4): await process_frame
	game.queue_free()
	TSAssets.scenes.clear()
	TSAssets.surfaces.clear()
	for frame in range(4): await process_frame
	quit()

class_name TSEnemy
extends CharacterBody3D
var game
var enemy_id:String
var mutant:bool=false
var health:float=100
var home:Vector3
var agent:NavigationAgent3D
var investigate:Vector3
var alert:float=0
var attack_clock:float=0
var think_clock:float=0
var destination:Vector3
var visual:Node3D
var phase:float=0
var animation_player:AnimationPlayer
var walk_clip:String=""
var idle_clip:String=""
func _ready() -> void:
	collision_layer=4
	collision_mask=1|2
	var shape=CapsuleShape3D.new()
	shape.radius=.3
	shape.height=1.8
	var collider=CollisionShape3D.new()
	collider.shape=shape
	collider.position.y=.9
	add_child(collider)
	visual=TSAssets.model("ForestMutant" if mutant else "StationZombie")
	add_child(visual)
	var players=visual.find_children("*","AnimationPlayer",true,false)
	if not players.is_empty():
		animation_player=players[0]
		for clip in animation_player.get_animation_list():
			if "Walk" in clip: walk_clip=clip
			if "Idle" in clip: idle_clip=clip
			animation_player.get_animation(clip).loop_mode=Animation.LOOP_LINEAR
	agent=NavigationAgent3D.new()
	agent.path_desired_distance=.5
	agent.target_desired_distance=1.2
	add_child(agent)
	health=100 if mutant else 85
	home=global_position
	destination=home
	set_meta("enemy",true)
func hear(point:Vector3,radius:float) -> void:
	if global_position.distance_to(point)<minf(radius,40 if not mutant else 30):
		investigate=point
		alert=8
func damage(amount:float) -> void:
	health=maxf(0,health-amount)
	if health<=0:
		visible=false
		process_mode=Node.PROCESS_MODE_DISABLED
	else: hear(game.player.global_position,100)
func _physics_process(dt:float) -> void:
	if not game.playing or health<=0: return
	attack_clock-=dt
	think_clock-=dt
	alert=maxf(0,alert-dt)
	if think_clock<=0:
		think_clock=.2
		var delta=game.player.global_position-global_position
		var distance=delta.length()
		var to_player=delta.normalized()
		var view=(global_basis.z.dot(to_player)>.35 and distance<20*lerpf(.55,1,game.daylight))
		if game.player.lamp.visible and distance<25:
			view=view or (-game.player.camera.global_basis.z).dot(-to_player)>.90
		var ray=PhysicsRayQueryParameters3D.create(global_position+Vector3.UP*1.5,game.player.global_position+Vector3.UP*.9,1|2,[get_rid()])
		var hit=get_world_3d().direct_space_state.intersect_ray(ray)
		var clear=not hit.is_empty() and hit.collider==game.player
		if view and clear:
			alert=6
			investigate=game.player.global_position
		if distance<1.65 and view and clear:
			if attack_clock<=0:
				game.damage(14 if mutant else 8)
				attack_clock=1.3
			destination=global_position
		else:
			destination=investigate if alert>0 else home+Vector3(sin(Time.get_ticks_msec()*.00008+home.x)*5,0,cos(Time.get_ticks_msec()*.00008+home.z)*5)
			agent.target_position=destination
	var next=global_position
	if NavigationServer3D.map_get_iteration_id(agent.get_navigation_map())>0 and not agent.is_navigation_finished(): next=agent.get_next_path_position()
	var direction=next-global_position
	direction.y=0
	if direction.length()>.15:
		direction=direction.normalized()
		rotation.y=lerp_angle(rotation.y,atan2(direction.x,direction.z),dt*6)
	velocity.x=direction.x*(2.2 if mutant else 1.5)
	velocity.z=direction.z*(2.2 if mutant else 1.5)
	velocity.y-=18*dt
	if is_on_floor(): _try_step(Vector3(velocity.x,0,velocity.z)*dt)
	move_and_slide()
	if animation_player:
		var wanted=walk_clip if Vector2(velocity.x,velocity.z).length()>.1 else idle_clip
		if wanted!="" and animation_player.current_animation!=wanted: animation_player.play(wanted)

func _try_step(horizontal:Vector3) -> void:
	if horizontal.length_squared()<.000001 or not test_move(global_transform,horizontal): return
	var raised=global_transform
	raised.origin+=Vector3.UP*.32
	if test_move(global_transform,Vector3.UP*.32) or test_move(raised,horizontal): return
	var top=raised.origin+horizontal+horizontal.normalized()*.34+Vector3.UP*.02
	var query=PhysicsRayQueryParameters3D.create(top,top-Vector3.UP*.38,1,[get_rid()])
	var hit=get_world_3d().direct_space_state.intersect_ray(query)
	if not hit.is_empty() and hit.normal.y>.7 and hit.position.y>global_position.y+.02: global_position.y=hit.position.y+.01

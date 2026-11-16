class_name TSProjectiles
extends Node3D
var game
var bullets:Array=[]
func setup(owner_game) -> void:
	game=owner_game
	var material=StandardMaterial3D.new()
	material.shading_mode=BaseMaterial3D.SHADING_MODE_UNSHADED
	material.albedo_color=Color(1,.67,.24)
	for i in range(64):
		var visual=MeshInstance3D.new()
		var mesh=BoxMesh.new()
		mesh.size=Vector3(.006,.006,1)
		visual.mesh=mesh
		visual.material_override=material
		visual.cast_shadow=GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
		add_child(visual)
		visual.visible=false
		bullets.append({"active":false,"position":Vector3.ZERO,"velocity":Vector3.ZERO,"age":0.0,"damage":0.0,"visual":visual})
func free_slots() -> int:
	var count=0
	for bullet in bullets:
		if not bullet.active: count+=1
	return count
func fire(origin:Vector3,direction:Vector3,speed:float,damage:float) -> bool:
	for bullet in bullets:
		if not bullet.active:
			bullet.active=true
			bullet.position=origin
			bullet.velocity=direction*speed
			bullet.age=0.0
			bullet.damage=damage
			return true
	return false
func clear() -> void:
	for bullet in bullets:
		bullet.active=false
		bullet.visual.visible=false
func _physics_process(dt:float) -> void:
	if not game.playing: return
	for bullet in bullets:
		if not bullet.active: continue
		var previous=bullet.position
		var next=previous+bullet.velocity*dt+Vector3.DOWN*4.9*dt*dt
		var query=PhysicsRayQueryParameters3D.create(previous,next,1|4,[game.player.get_rid()])
		var hit=get_world_3d().direct_space_state.intersect_ray(query)
		if not hit.is_empty():
			game.hit(hit.collider,bullet.damage)
			bullet.active=false
		else: bullet.position=next
		bullet.velocity+=Vector3.DOWN*9.8*dt
		bullet.age+=dt
		if bullet.age>3: bullet.active=false
		bullet.visual.visible=bullet.active
		if bullet.active:
			var delta=next-previous
			bullet.visual.global_position=(previous+next)*.5
			if delta.length_squared()>.00001: bullet.visual.look_at(next,Vector3.UP)
			bullet.visual.scale.z=maxf(.01,delta.length())

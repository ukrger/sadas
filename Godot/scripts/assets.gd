class_name TSAssets
extends RefCounted

static var scenes:Dictionary = {}
static var surfaces:Dictionary = {}

static func material(color:Color, roughness:float=.8) -> StandardMaterial3D:
	var m = StandardMaterial3D.new()
	m.albedo_color = color
	m.roughness = roughness
	return m

static func surface(style:String, color:Color) -> StandardMaterial3D:
	var key = style + str(color)
	if surfaces.has(key): return surfaces[key]
	var m = material(color,.42 if style=="Steel" else .8)
	m.albedo_texture = load("res://assets/textures/"+style+"_Albedo.png")
	m.normal_enabled = true
	m.normal_texture = load("res://assets/textures/"+style+"_Normal.png")
	m.normal_scale = .45
	m.metallic_texture = load("res://assets/textures/"+style+"_MetalSmooth.png")
	m.metallic_texture_channel = BaseMaterial3D.TEXTURE_CHANNEL_RED
	# Imported alpha stores smoothness; Godot wants roughness, so retain authored roughness here.
	m.metallic = 1 if style == "Steel" else .0
	surfaces[key] = m
	return m

static func model(model_name:String, lod:int=0) -> Node3D:
	var path = "res://assets/models/"+model_name+".glb"
	if not scenes.has(path): scenes[path] = load(path)
	if scenes[path] == null: return Node3D.new()
	var root = scenes[path].instantiate()
	for child in root.find_children("*", "MeshInstance3D", true, false):
		child.visible = child.name.begins_with("LOD"+str(lod)+"_")
		var original=child.get_active_material(0)
		if original is StandardMaterial3D:
			var key=str(original.resource_name)
			var style=""
			if "Wood" in key or "HouseTrim" in key: style="Wood"
			elif "Steel" in key or "Roof" in key: style="Steel"
			elif "Paint" in key: style="Paint"
			elif "Canvas" in key or "Cloth" in key or "Sleeve" in key: style="Fabric"
			elif "Leather" in key or "Glove" in key or "Grip" in key: style="Leather"
			elif "Plaster" in key or "Stone" in key: style="Plaster"
			elif "Brick" in key: style="Brick"
			if style!="": child.material_override=surface(style,original.albedo_color)
	return root

static func box(parent:Node3D, at:Vector3, size:Vector3, mat:Material, solid:bool=true) -> Node3D:
	var root:Node3D = StaticBody3D.new() if solid else Node3D.new()
	root.position = at
	var mesh = MeshInstance3D.new()
	var cube = BoxMesh.new()
	cube.size = size
	mesh.mesh = cube
	mesh.material_override = mat
	root.add_child(mesh)
	if solid:
		var collider = CollisionShape3D.new()
		var shape = BoxShape3D.new()
		shape.size = size
		collider.shape = shape
		root.add_child(collider)
	parent.add_child(root)
	return root

static func collider(parent:Node3D, at:Vector3, size:Vector3, moving:bool=false) -> StaticBody3D:
	var root = AnimatableBody3D.new() if moving else StaticBody3D.new()
	if moving: root.sync_to_physics=false
	root.position = at
	var collider = CollisionShape3D.new()
	var shape = BoxShape3D.new()
	shape.size = size
	collider.shape = shape
	root.add_child(collider)
	parent.add_child(root)
	return root

static func plant_parts(model_name:String, lod:int) -> Array:
	var root = model(model_name,lod)
	var parts = []
	for mesh in root.find_children("*","MeshInstance3D",true,false):
		if not mesh.visible: continue
		var relative = Transform3D.IDENTITY
		var node:Node = mesh
		while node != root:
			relative = node.transform * relative
			node = node.get_parent()
		parts.append({"mesh":mesh.mesh,"transform":relative,"material":mesh.get_active_material(0)})
	root.free()
	return parts

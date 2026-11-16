class_name TSCore
extends RefCounted

const SHORT_STOPS = [160.0, 640.0, 1200.0, 1800.0]
const LONG_STOPS = [2800.0, 6000.0, 9200.0, 12400.0]
const ITEMS = ["parts", "cloth", "medkit", "food", "water", "kit", "pistol_ammo", "shells", "rifle_ammo"]
const WEAPONS = ["axe", "pistol", "shotgun", "rifle"]
const CAPACITY = {"pistol":12, "shotgun":5, "rifle":30}
const AMMO = {"pistol":"pistol_ammo", "shotgun":"shells", "rifle":"rifle_ammo"}

static func new_inventory() -> Dictionary:
	var result = {}
	for key in ITEMS: result[key] = 0
	result["owned"] = []
	result["selected"] = ""
	result["magazines"] = {"pistol":0, "shotgun":0, "rifle":0}
	return result

static func add_item(bag:Dictionary, key:String, amount:int) -> bool:
	if key not in ITEMS or amount <= 0 or bag[key] + amount > 999: return false
	bag[key] += amount
	return true

static func take_weapon(bag:Dictionary, key:String) -> bool:
	if key not in WEAPONS or key in bag.owned: return false
	bag.owned.append(key)
	bag.selected = key
	if key in CAPACITY: bag.magazines[key] = CAPACITY[key]
	return true

static func craft(bag:Dictionary, recipe:String) -> bool:
	if recipe not in ["kit", "medkit"]: return false
	var cost = [3,1] if recipe == "kit" else [1,2]
	if bag.parts < cost[0] or bag.cloth < cost[1] or bag[recipe] >= 999: return false
	bag.parts -= cost[0]
	bag.cloth -= cost[1]
	bag[recipe] += 1
	return true

static func reload_weapon(bag:Dictionary, weapon:String) -> int:
	if weapon not in CAPACITY or weapon not in bag.owned: return 0
	var reserve = AMMO[weapon]
	var count = mini(CAPACITY[weapon] - bag.magazines[weapon], bag[reserve])
	if weapon == "shotgun": count = mini(1,count)
	bag.magazines[weapon] += count
	bag[reserve] -= count
	return count

static func spend_round(bag:Dictionary, weapon:String) -> bool:
	if weapon not in CAPACITY or weapon not in bag.owned or bag.magazines[weapon] <= 0: return false
	bag.magazines[weapon] -= 1
	return true

static func aim_step(current:float, dt:float, held:bool) -> float:
	if not is_finite(dt) or dt < 0: return current
	return lerpf(current, 1.0 if held else 0.0, 1.0-exp(-18.0*dt))

static func chunk_window(train_tile:int, player_tile:int) -> Array:
	var result = []
	for index in range(train_tile-2,train_tile+5): result.append(index)
	for index in range(player_tile-1,player_tile+2):
		if index not in result: result.append(index)
	result.sort()
	return result

static func number(value, low:float, high:float) -> bool:
	return (value is float or value is int) and is_finite(float(value)) and float(value) >= low and float(value) <= high

static func integer(value, low:int, high:int) -> bool:
	return number(value,low,high) and float(value) == floor(float(value))

static func valid_inventory(bag) -> bool:
	if not bag is Dictionary: return false
	for key in ITEMS:
		if not bag.has(key) or not integer(bag[key],0,999): return false
	if not bag.get("owned") is Array or not bag.get("magazines") is Dictionary: return false
	var seen = []
	for key in bag.owned:
		if key not in WEAPONS or key in seen: return false
		seen.append(key)
	if bag.get("selected",null) != "" and bag.get("selected") not in bag.owned: return false
	for key in CAPACITY:
		if not integer(bag.magazines.get(key,-1),0,CAPACITY[key]): return false
		if key not in bag.owned and bag.magazines[key] != 0: return false
	return true

static func position_array(value) -> bool:
	if not value is Array or value.size()!=3: return false
	for n in value:
		if not number(n,-50000,50000): return false
	return true

static func vector(value:Array) -> Vector3:
	return Vector3(float(value[0]),float(value[1]),float(value[2]))

static func array(value:Vector3) -> Array:
	return [value.x,value.y,value.z]

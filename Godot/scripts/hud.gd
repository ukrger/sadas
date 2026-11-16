class_name TSHud
extends CanvasLayer
var game
var root:Control
var panel:PanelContainer
var information:Label
var prompt:Label
var cross:Label
var fps_label:Label
var menu_message:Label
var stats_clock:float=0
func _ready() -> void:
	process_mode=Node.PROCESS_MODE_ALWAYS
	rebuild()
func _label(message:String,size:int=18) -> Label:
	var label=Label.new()
	label.text=message
	label.add_theme_font_size_override("font_size",size)
	label.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART
	return label
func _button(parent:Node,title:String,action:Callable) -> void:
	var button=Button.new()
	button.text=title
	button.custom_minimum_size.y=38
	button.pressed.connect(action)
	parent.add_child(button)
func rebuild() -> void:
	if root: root.queue_free()
	root=Control.new()
	root.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	root.mouse_filter=Control.MOUSE_FILTER_IGNORE
	add_child(root)
	information=_label("",17)
	information.position=Vector2(20,16)
	information.size=Vector2(600,140)
	root.add_child(information)
	fps_label=_label("",16)
	fps_label.set_anchors_and_offsets_preset(Control.PRESET_TOP_RIGHT)
	fps_label.position=Vector2(-150,16)
	fps_label.size=Vector2(140,30)
	root.add_child(fps_label)
	prompt=_label("",18)
	prompt.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_WIDE)
	prompt.offset_top=-90
	prompt.offset_bottom=-12
	prompt.offset_left=20
	prompt.offset_right=-20
	prompt.horizontal_alignment=HORIZONTAL_ALIGNMENT_CENTER
	root.add_child(prompt)
	cross=_label("+",22)
	cross.set_anchors_and_offsets_preset(Control.PRESET_CENTER)
	cross.position=Vector2(-8,-16)
	root.add_child(cross)
	if game.menu!="": _menu()
	elif game.bag_open: _bag()
	update_hud()
func _menu() -> void:
	var shade=ColorRect.new()
	shade.color=Color(.025,.04,.025,.86)
	shade.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	root.add_child(shade)
	panel=PanelContainer.new()
	panel.set_anchors_and_offsets_preset(Control.PRESET_CENTER)
	panel.position=Vector2(-320,-310)
	panel.size=Vector2(640,620)
	root.add_child(panel)
	var margin=MarginContainer.new()
	for side in ["left","right","top","bottom"]: margin.add_theme_constant_override("margin_"+side,22)
	panel.add_child(margin)
	var scroll=ScrollContainer.new()
	scroll.horizontal_scroll_mode=ScrollContainer.SCROLL_MODE_DISABLED
	margin.add_child(scroll)
	var column=VBoxContainer.new()
	column.size_flags_horizontal=Control.SIZE_EXPAND_FILL
	column.add_theme_constant_override("separation",10)
	scroll.add_child(column)
	var title="TRAIN SUMMER"
	if game.menu=="death": title=game.text("ВЫ НЕ ВЕРНУЛИСЬ","YOU DID NOT RETURN")
	if game.menu=="victory": title=game.text("СВЯЗЬ ВОССТАНОВЛЕНА","RADIO LINK RESTORED")
	if game.menu=="settings": title=game.text("НАСТРОЙКИ","SETTINGS")
	column.add_child(_label(title,32))
	if game.menu=="settings": _settings(column);return
	column.add_child(_label(game.text("Ясное лето. Четыре станции. Последний поезд.\nЗабери модуль из каждой станционной рации и доведи поезд до конца маршрута.","Clear summer skies. Four stations. The last train.\nRecover each station radio relay and reach the route's end.")))
	if game.playing and game.health>0 and game.menu!="victory": _button(column,game.text("Продолжить","Resume"),func():game.set_menu(""))
	_button(column,game.text("Начать маршрут заново","Start route")+(game.text(" · длинный"," · long") if game.long_route else game.text(" · короткий"," · short")),game.start_game)
	if game.menu=="start": _button(column,game.text("Короткий проверочный маршрут","Short test route") if game.long_route else game.text("Длинный маршрут · 5–10 минут между станциями","Long route · 5–10 minutes between stations"),func():game.choose_route(not game.long_route))
	_button(column,game.text("Загрузить сохранение","Load save"),func():game.load_game(false);rebuild())
	_button(column,game.text("Загрузить автосохранение","Load autosave"),func():game.load_game(true);rebuild())
	if game.menu=="pause": _button(column,game.text("Сохранить","Save"),func():game.save_game(false);rebuild())
	_button(column,game.text("Настройки","Settings"),func():game.set_menu("settings"))
	_button(column,"English" if not game.settings.english else "Русский",func():game.settings.english=not game.settings.english;game.save_settings();rebuild())
	_button(column,game.text("Выход","Quit"),func():get_tree().quit())
	menu_message=_label(game.status,16)
	column.add_child(menu_message)
	column.add_child(_label(game.text("WASD — ходьба · E — взаимодействие · Tab — рюкзак\nЛКМ — атака · ПКМ — прицел · 1/2/3/4 — оружие\nR — зарядка · F — фонарь · Esc — пауза", "WASD move · E interact · Tab backpack\nLMB attack · RMB aim · 1/2/3/4 weapons\nR reload · F flashlight · Esc pause"),15))
func _settings(column:VBoxContainer) -> void:
	_button(column,game.text("Назад","Back"),func():game.set_menu("pause" if game.playing else "start"))
	_button(column,game.text("Язык: ","Language: ")+("English" if game.settings.english else "Русский"),func():game.settings.english=not game.settings.english;game.save_settings();rebuild())
	var levels=[game.text("Низкое","Low"),game.text("Среднее","Medium"),game.text("Высокое","High")]
	_button(column,game.text("Качество: ","Quality: ")+levels[game.settings.quality],func():game.settings.quality=(game.settings.quality+1)%3;game.apply_settings();rebuild())
	for key in ["fps","vsync","fullscreen"]:
		var check=CheckButton.new()
		check.text={"fps":game.text("Показывать FPS","Show FPS"),"vsync":"V-Sync","fullscreen":game.text("Полный экран","Fullscreen")}[key]
		check.button_pressed=game.settings[key]
		check.toggled.connect(func(value):game.settings[key]=value;game.apply_settings())
		column.add_child(check)
	var automatic=CheckButton.new()
	automatic.text=game.text("Автоотправление (с игроком на борту)","Automatic departure (with player aboard)")
	automatic.button_pressed=game.automatic
	automatic.toggled.connect(func(value):game.automatic=value)
	column.add_child(automatic)
	_button(column,game.text("Лимит FPS: ","FPS limit: ")+str(game.settings.limit),func():var values=[0,30,60,120,144];game.settings.limit=values[(values.find(game.settings.limit)+1)%5];game.apply_settings();rebuild())
	column.add_child(_label(game.text("Чувствительность мыши","Mouse sensitivity"),16))
	var slider=HSlider.new()
	slider.min_value=.0005
	slider.max_value=.006
	slider.step=.0001
	slider.value=game.settings.sensitivity
	slider.value_changed.connect(func(value):game.settings.sensitivity=value)
	column.add_child(slider)
	_button(column,game.text("Применить и сохранить","Apply and save"),func():game.save_settings();game.status=game.text("Настройки сохранены.","Settings saved."))
	column.add_child(_label(game.text("Высокое качество включает SSAO и объёмный туман Forward+. Начни со среднего. Целевой FPS проверяется на твоём ПК.","High enables SSAO and Forward+ volumetric fog. Start with Medium. Target FPS requires testing on your PC."),15))
func _bag() -> void:
	var overlay=TextureRect.new()
	overlay.texture=game.player.bag_view.get_texture()
	overlay.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	overlay.mouse_filter=Control.MOUSE_FILTER_IGNORE
	overlay.expand_mode=TextureRect.EXPAND_IGNORE_SIZE
	root.add_child(overlay)
	root.move_child(overlay,0)
	panel=PanelContainer.new()
	panel.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_WIDE)
	panel.offset_left=120
	panel.offset_right=-120
	panel.offset_top=-160
	panel.offset_bottom=-10
	root.add_child(panel)
	var column=VBoxContainer.new()
	panel.add_child(column)
	column.add_child(_label(game.text("РЮКЗАК · клик по модели — использовать / выбрать · Tab — закрыть", "BACKPACK · click a model to use/equip · Tab to close"),17))
	var row=HBoxContainer.new()
	column.add_child(row)
	_button(row,game.text("Ремнабор: 3 детали + 1 ткань","Repair kit: 3 parts + 1 cloth"),func():TSCore.craft(game.bag,"kit"))
	_button(row,game.text("Аптечка: 1 деталь + 2 ткани","Medkit: 1 part + 2 cloth"),func():TSCore.craft(game.bag,"medkit"))
	for key in ["water","food","medkit","kit"]:
		_button(row,{"water":game.text("Пить","Drink"),"food":game.text("Есть","Eat"),"medkit":game.text("Лечиться","Heal"),"kit":game.text("Ремонт","Repair")}[key],func():game.use_item(key))
func update_hud() -> void:
	if not information: return
	information.visible=game.menu==""
	prompt.visible=game.menu==""
	cross.visible=game.menu=="" and not game.bag_open and game.player.aim<.5
	fps_label.visible=game.settings.fps
	fps_label.text=str(Engine.get_frames_per_second())+" FPS"
	var lines=game.text("Здоровье: ","Health: ")+"%d · "%game.health+game.text("Модули: ","Relays: ")+str(game._relays())+" / 4"
	if game.player.aboard: lines+="\n%.1f m/s · %.1f "%[game.speed,game.fuel]+game.text("топливо · ","fuel · ")+"%d%%"%game.condition
	if game.bag.selected in TSCore.CAPACITY: lines+="\n"+game.item_name(game.bag.selected)+": %d / %d"%[game.bag.magazines[game.bag.selected],game.bag[TSCore.AMMO[game.bag.selected]]]
	if game.hunger<30 or game.thirst<30 or game.bag_open: lines+="\n"+game.text("Сытость / вода: ","Food / water: ")+"%d / %d"%[game.hunger,game.thirst]
	if game.bag_open:
		for key in TSCore.ITEMS: lines+="\n"+game.item_name(key)+": "+str(game.bag[key])
	if game.parked and game.stop_index<4:
		lines+="\n"+game.text("Забери модуль из рации в станционном доме.","Recover the relay from the station house radio.")
		lines+="\n"+(game.text("Отправление: ","Departure: ")+"%02d:%02d"%[int(ceil(game.remaining))/60,int(ceil(game.remaining))%60] if game.automatic else game.text("Ручное отправление из кабины.","Manual departure from the cab."))
		if game.remaining<=0: lines+=" · "+game.text("ожидание модуля / игрока / обслуживания","waiting for relay / player / service")
	information.text=lines
	var action=""
	if not game.player.target.is_empty():
		var kind=game.player.target.kind
		action=game.text("E — взаимодействовать: ","E — interact: ")+game.item_name(kind)
	prompt.text=game.status+"\n"+action+"\n"+game.text("Tab — рюкзак · Esc — пауза · F5/F9 — сохранить/загрузить", "Tab backpack · Esc pause · F5/F9 save/load")

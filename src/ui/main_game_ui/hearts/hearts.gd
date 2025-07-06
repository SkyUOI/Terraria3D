extends Control

@export var heart_fill_red: Texture
@export var heart_fill_yellow: Texture
@export var heart_left: Texture
@export var heart_middle: Texture
@export var heart_right: Texture
@export var heart_right_fancy: Texture
@export var heart_single_fancy: Texture
@export var player: Node

@onready var label = $Label

var heart_num: float = 100
var heart_num_max: int = 100
var x_min = 1000
var x_max = 0
var y_min = 1000
var y_max = 0

func set_hp(hp: int):
	heart_num = float(hp) / 20.0
	draw_hearts()

func set_hp_max(hp_max: int):
	heart_num_max = hp_max / 20
	draw_frame()

func mouse_in_area():
	var pos = get_global_mouse_position()
	var x = pos.x
	var y = pos.y
	return (y_min <= y && y <= y_max) && (x_min <= x && x <= x_max)

func _process(_delta):
	clear_hearts()
	draw_hearts()
	
	if (mouse_in_area()):
		set_label_text()
		label.global_position = get_viewport().get_mouse_position() + Vector2(-30, -30)
		label.visible = true
	else:
		label.visible = false
	
func set_label_text():
	label.text = str(int(roundf(heart_num * 20))) + "/" + str(heart_num_max * 20)

func _ready():
	draw_frame()
	
func clear_frame():
	for c in $Frame.get_children():
		c.queue_free()

func clear_hearts():
	for c in $HeartRed.get_children():
		c.queue_free()
	for c in $HeartYellow.get_children():
		c.queue_free()
	x_min = 1000
	x_max = 0
	y_min = 1000
	y_max = 0
	
func get_frame(frame_num: int) -> Array[Sprite2D]:
	if (frame_num == 1):
		var frame = Sprite2D.new()
		frame.texture = heart_single_fancy
		frame.position = frame.position + Vector2(-2, 0)
		return [frame]
	
	var frames:Array[Sprite2D] = []
	frames.resize(frame_num)
	for i in range(frames.size()):
		frames[i] = Sprite2D.new()
		frames[i].texture = heart_middle
		var row = 0 if i < 10 else 30
		var col = (i % 10) * 24
		frames[i].position = Vector2(col, row)
		
	frames[0].texture = heart_left
	frames[0].position = frames[0].position - Vector2(2, 0)
	
	if (frame_num > 10):
		frames[9].texture = heart_right
		frames[10].texture = heart_left
		frames[10].position = frames[10].position + Vector2(-2, 0)
		
	frames.back().texture = heart_right_fancy
	frames.back().position = frames.back().position + Vector2(-2, 0)
	
	return frames
	
func get_heart(num: float, texture: Texture, heart_scale_delta: float) -> Array[Sprite2D]:
	var heart: Array[Sprite2D] = []
	heart.resize(int(ceilf(num)))
	for i in range(heart.size()):
		heart[i] = Sprite2D.new()
		heart[i].texture = texture
		var row = 0 if i < 10 else 30
		var col = (i % 10) * 24 - 1
		heart[i].position = Vector2(col, row)
		
	var t = num - floorf(num)
	t = 1.0 if t == 0 else t
	var s = clamp(t + heart_scale_delta, 0, 1)
	if (!heart.is_empty()):
			heart.back().scale = Vector2(s, s)
		
	return heart
	
# 绘制血条边框
func draw_frame():
	clear_frame()
	var frame_num = min(heart_num_max, 20)
	var frame: Array[Sprite2D] = get_frame(frame_num)
	
	for f in frame:
		$Frame.add_child(f)

# 根据血量绘制心
func draw_hearts():
	clear_hearts()
	var heart_scale_delta = (sin($Timer.time_left * TAU) + 1) * 0.05
	
	var heart_yellow_max: float = max(0, heart_num_max - 20)
	var heart_red_max: float = min(20, heart_num_max)
		
	var heart_yellow_num: float = heart_yellow_max \
		if   	heart_num > heart_yellow_max * 2 \
		else    heart_num / 2
	var heart_red_num: float = heart_num - heart_yellow_num
	
	var delta_red: float= heart_scale_delta if heart_red_max != heart_red_num else 0
	var delta_yellow: float = heart_scale_delta if heart_yellow_max != heart_yellow_num else 0
	
	var heart_red: Array[Sprite2D] = get_heart(heart_red_num, heart_fill_red\
		, delta_red)
	var heart_yellow: Array[Sprite2D] = get_heart(heart_yellow_num, heart_fill_yellow\
		, delta_yellow)
	
	for hy in heart_yellow:
		$HeartYellow.add_child(hy)
		update_focus_area(hy)
	for hr in heart_red:
		$HeartRed.add_child(hr)
		update_focus_area(hr)
	extend_focus_area()

func update_focus_area(heart: Sprite2D):
	x_min = min(x_min, heart.global_position.x)
	x_max = max(x_max, heart.global_position.x)
	y_min = min(y_min, heart.global_position.y)
	y_max = max(y_max, heart.global_position.y)

func extend_focus_area():
	x_min -= 10
	x_max += 10
	y_min -= 10
	y_max += 10

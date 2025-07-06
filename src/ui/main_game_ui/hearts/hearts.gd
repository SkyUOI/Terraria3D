extends Control

@export var heart_fill_red: Texture
@export var heart_fill_yellow: Texture
@export var heart_left: Texture
@export var heart_middle: Texture
@export var heart_right: Texture
@export var heart_right_fancy: Texture
@export var heart_single_fancy: Texture

@onready var label = $Label

var heart_num: float
var heart_num_max: int


# 血不满时心的周期性大小变化
var heart_scale_delta: float

func mouse_in_area():
	var pos = get_viewport().get_mouse_position()
	var x = pos.x
	var y = pos.y
	return (0 <= y && y <= 80) && (880 <= x && x <= 1150)

func _process(_delta):
	heart_scale_delta = (sin($Timer.time_left * TAU) + 1) * 0.05
	clear_hearts()
	draw_hearts(470)
	
	if (mouse_in_area()):
		print("aaa")
		set_label_text()
		label.global_position = get_viewport().get_mouse_position() + Vector2(-30, -30)
		label.visible = true
	else:
		label.visible = false
	print(get_viewport().get_mouse_position())
	
func set_label_text():
	label.text = str(int(roundf(heart_num * 20))) + "/" + str(heart_num_max * 20)

func _ready():
	draw_frame(540)

	# 设置鼠标悬停提示文本
	tooltip_text = "这是按钮的提示信息"
	
	# 自定义提示样式（可选）
	var tooltip_style = StyleBoxFlat.new()
	tooltip_style.bg_color = Color(0.1, 0.1, 0.1, 0.9)
	tooltip_style.border_width_bottom = 2
	tooltip_style.border_color = Color.GOLD
	tooltip_style.corner_radius_top_left = 5
	tooltip_style.corner_radius_top_right = 5
	tooltip_style.corner_radius_bottom_right = 5
	tooltip_style.corner_radius_bottom_left = 5
	
	# 应用自定义样式
	add_theme_stylebox_override("panel", tooltip_style)
	add_theme_font_size_override("font_size", 14)
	add_theme_color_override("font_color", Color.WHITE)
	
	
func clear_frame():
	for c in $Frame.get_children():
		c.queue_free()

func clear_hearts():
	for c in $HeartRed.get_children():
		c.queue_free()
	for c in $HeartYellow.get_children():
		c.queue_free()
	
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
	
func get_heart(num: float, texture: Texture, floating: bool) -> Array[Sprite2D]:
	var heart: Array[Sprite2D] = []
	heart.resize(int(ceilf(num)))
	for i in range(heart.size()):
		heart[i] = Sprite2D.new()
		heart[i].texture = texture
		var row = 0 if i < 10 else 30
		var col = (i % 10) * 24 - 1
		heart[i].position = Vector2(col, row)
	
	var delta = heart_scale_delta if floating else 0.0
	
	var t = num - floorf(num)
	t = 1.0 if t == 0 else t
	var s = clamp(t + delta, 0, 1)
	if (!heart.is_empty()):
			heart.back().scale = Vector2(s, s)
		
	return heart
	
# 绘制血条边框
func draw_frame(hp_max: int):
	heart_num_max = floor(hp_max / 20.0)
	var frame_num = min(heart_num_max, 20)
	var frame: Array[Sprite2D] = get_frame(frame_num)
	
	for f in frame:
		$Frame.add_child(f)

# 根据血量绘制心
func draw_hearts(hp: float):
	heart_num = hp / 20
	
	var heart_yellow_max: float = max(0, heart_num_max - 20)
	var heart_red_max: float = min(20, heart_num_max)
		
	var heart_yellow_num: float = heart_yellow_max \
		if   	heart_num > heart_yellow_max * 2 \
		else    heart_num / 2
	var heart_red_num: float = heart_num - heart_yellow_num
	
	var heart_red: Array[Sprite2D] = get_heart(heart_red_num, heart_fill_red\
		, heart_red_max != heart_red_num)
	var heart_yellow: Array[Sprite2D] = get_heart(heart_yellow_num, heart_fill_yellow\
		, heart_yellow_max != heart_yellow_num)
	
	for hy in heart_yellow:
		$HeartYellow.add_child(hy)
	for hr in heart_red:
		$HeartRed.add_child(hr)

extends Node2D

@export var heart_fill_red: Texture
@export var heart_fill_yellow: Texture
@export var heart_left: Texture
@export var heart_middle: Texture
@export var heart_right: Texture
@export var heart_right_fancy: Texture
@export var heart_single_fancy: Texture

func _ready():
	draw_hearts(260)
	print("aaa")
	
func get_frame(heart_num: int) -> Array[Sprite2D]:
	if (heart_num == 1):
		var frame = Sprite2D.new()
		frame.texture = heart_single_fancy
		frame.position = frame.position + Vector2(-2, 0)
		return [frame]
	
	var frames:Array[Sprite2D] = []
	frames.resize(heart_num)
	for i in range(frames.size()):
		frames[i] = Sprite2D.new()
		frames[i].texture = heart_middle
		var row = 0 if i < 10 else 30
		var col = (i % 10) * 24
		frames[i].position = Vector2(col, row)
		
	frames[0].texture = heart_left
	frames[0].position = frames[0].position - Vector2(2, 0)
	
	if (heart_num > 10):
		frames[9].texture = heart_right
		frames[10].texture = heart_left
		frames[10].position = frames[10].position + Vector2(-2, 0)
		
	frames.back().texture = heart_right_fancy
	frames.back().position = frames.back().position + Vector2(-2, 0)
	
	return frames
	
func get_heart(heart_num: int, texture: Texture) -> Array[Sprite2D]:
	var heart: Array[Sprite2D] = []
	heart.resize(heart_num)
	for i in range(heart.size()):
		heart[i] = Sprite2D.new()
		heart[i].texture = texture
		var row = 0 if i < 10 else 30
		var col = (i % 10) * 24 - 1
		heart[i].position = Vector2(col, row)
	return heart

# 根据血量绘制血条
func draw_hearts(hp: int):
	var heart_num = hp / 20
	assert(heart_num <= 40, "超过生命值上限")
	assert(hp % 20 == 0, "血量必须是20的倍数")
	var heart_red_num: int = min(20, heart_num)
	var heart_yellow_num: int = max(0, heart_num - 20)
	
	var frame: Array[Sprite2D] = get_frame(heart_red_num)
	var heart_red: Array[Sprite2D] = get_heart(heart_red_num, heart_fill_red)
	var heart_yellow: Array[Sprite2D] = get_heart(heart_yellow_num, heart_fill_yellow)
	
	for f in frame:
		add_child(f)
	for hr in heart_red:
		add_child(hr)
	for hy in heart_yellow:
		add_child(hy)

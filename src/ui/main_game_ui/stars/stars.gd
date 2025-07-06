extends Control

@export var star_single: Texture
@export var star_up: Texture
@export var star_middle: Texture
@export var star_down: Texture
@export var star_fill: Texture

var star_num: float = 20
var star_num_max: int = 20

func set_mp(mp: int):
	star_num = float(mp) / 20.0
	draw_stars()
	
func set_mp_max(mp_max: int):
	star_num_max = mp_max / 20
	draw_frame()


func _process(_delta):
	draw_stars()

func _ready():
	draw_frame()

func clear_frame():
	for c in $Frame.get_children():
		c.queue_free()

func get_frame(frame_num: int) -> Array[Sprite2D]:
	if (frame_num == 1):
		var frame = Sprite2D.new()
		frame.texture = star_single
		frame.position = frame.position + Vector2(0, -2)
		return [frame]
	
	var frames:Array[Sprite2D] = []
	frames.resize(frame_num)
	for i in range(frames.size()):
		frames[i] = Sprite2D.new()
		frames[i].texture = star_middle
		var row = i * 22
		frames[i].position = Vector2(0, row)
		
	frames[0].texture = star_up
	frames[0].position = frames[0].position + Vector2(0, -2)
	
	frames.back().texture = star_down
	frames.back().position = frames.back().position + Vector2(0, 2)
	
	return frames

func draw_frame():
	var frame_num = star_num_max
	var frame: Array[Sprite2D] = get_frame(frame_num)
	
	for f in frame:
		$Frame.add_child(f)
		
func clear_star():
	for c in $Star.get_children():
		c.queue_free()

func get_star(num: float, star_scale_delta: float) -> Array[Sprite2D]:
	var heart: Array[Sprite2D] = []
	heart.resize(int(ceilf(num)))
	for i in range(heart.size()):
		heart[i] = Sprite2D.new()
		heart[i].texture = star_fill
		var row = i * 22 
		heart[i].position = Vector2(0, row)
		
	var t = num - floorf(num)
	t = 1.0 if t == 0 else t
	var s = clamp(t + star_scale_delta, 0, 1)
	if (!heart.is_empty()):
			heart.back().scale = Vector2(s, s)
		
	return heart


func draw_stars():
	var star_scale_delta = (sin($Timer.time_left * TAU) + 1) * 0.05
		
	var star: Array[Sprite2D] = get_star(star_num, star_scale_delta)

	for s in star:
		$Star.add_child(s)

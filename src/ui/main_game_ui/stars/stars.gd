extends Control

@export var star_single: Texture
@export var star_up: Texture
@export var star_middle: Texture
@export var star_down: Texture
@export var star_fill: Texture

@onready var label = $Label

var star_num: float = 20
var star_num_max: int = 20
var x_min = 10000
var x_max = 0
var y_min = 10000
var y_max = 0

func set_mp(mp: int):
    var star_num_new = float(mp) / 20.0
    if star_num_new != star_num:
        star_num = star_num_new
        draw_stars()
    
func set_mp_max(mp_max: int):
    var star_num_max_new = mp_max / 20.0
    if star_num_max_new != star_num_max:
        star_num_max = int(star_num_max_new)
        draw_frame()
    
func mouse_in_area():
    var pos = get_global_mouse_position()
    var x = pos.x
    var y = pos.y
    return (y_min <= y && y <= y_max) && (x_min <= x && x <= x_max)
    
func set_label_text():
    label.text = str(int(roundf(star_num * 20))) + "/" + str(star_num_max * 20)


func _process(_delta):
    draw_stars()
    
    if (mouse_in_area()):
        set_label_text()
        label.global_position = get_viewport().get_mouse_position() + Vector2(-30, -30)
        label.visible = true
    else:
        label.visible = false

func _ready():
    draw_frame()
    
func extend_focus_area():
    x_min -= 10
    x_max += 10
    y_min -= 10
    y_max += 10
    
func update_focus_area(star: Sprite2D):
    x_min = min(x_min, star.global_position.x)
    x_max = max(x_max, star.global_position.x)
    y_min = min(y_min, star.global_position.y)
    y_max = max(y_max, star.global_position.y)

func clear_frame():
    for c in $Frame.get_children():
        c.queue_free()
    x_min = 10000
    x_max = 0
    y_min = 10000
    y_max = 0

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
    clear_frame()
    var frame_num = star_num_max
    var frame: Array[Sprite2D] = get_frame(frame_num)
    
    for f in frame:
        $Frame.add_child(f)
        update_focus_area(f)
    extend_focus_area()
        
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
    clear_star()
    var star_scale_delta = (sin($Timer.time_left * TAU) + 1) * 0.05
        
    var star: Array[Sprite2D] = get_star(star_num, star_scale_delta)

    for s in star:
        $Star.add_child(s)

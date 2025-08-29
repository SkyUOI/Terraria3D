extends Sprite2D

var is_sun = true

@export var sun:Texture2D
@export var moons:Array[Texture2D]

const moon_frames = 8

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	change_to_sun()


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _on_visible_on_screen_notifier_2d_screen_exited() -> void:
	if is_sun:
		change_to_moon()
	else:
		change_to_sun()
	is_sun = !is_sun

func change_to_sun():
	texture = sun
	vframes = 1

func change_to_moon():
	vframes = moon_frames
	texture = moons.pick_random()

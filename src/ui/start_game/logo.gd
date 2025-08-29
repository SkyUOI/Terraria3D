extends Sprite2D

@onready var original_scale = scale
@onready var original_rotation = rotation

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	larger()
	if randi() % 2 == 0:
		left_rotate()
	else:
		right_rotate()

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass

func larger():
	var tween = create_tween()\
	.tween_property(self, "scale", original_scale * 1.3, 12)\
	.set_trans(Tween.TRANS_LINEAR)\
	.set_ease(Tween.EASE_IN_OUT)
	tween.finished.connect(smaller)
	
func smaller():
	var tween = create_tween()\
	.tween_property(self, "scale", original_scale, 13)\
	.set_trans(Tween.TRANS_LINEAR)\
	.set_ease(Tween.EASE_IN_OUT)
	tween.finished.connect(larger)
	
func left_rotate():
	var tween = create_tween().tween_property(self, "rotation", original_rotation + deg_to_rad(7), 17)
	tween.finished.connect(right_rotate)

func right_rotate():
	var tween = create_tween().tween_property(self, "rotation", original_rotation - deg_to_rad(7), 17)
	tween.finished.connect(left_rotate)

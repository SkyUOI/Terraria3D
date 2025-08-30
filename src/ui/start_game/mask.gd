extends ColorRect


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	show()
	get_tree().create_tween().tween_property(self, "color", Color8(0, 0, 0, 0), 2)

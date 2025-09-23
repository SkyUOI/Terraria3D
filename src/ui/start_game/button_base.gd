extends Button

@onready var init_var = scale


func _ready() -> void:
	self.mouse_entered.connect(_on_mouse_entered)
	self.mouse_exited.connect(_on_mouse_exited)
	pivot_offset = size / 2


func _on_mouse_entered() -> void:
	(
		get_tree()
		. create_tween()
		. tween_property(self, "scale", init_var * 1.4, 0.15)
		. set_trans(Tween.TRANS_LINEAR)
		. set_ease(Tween.EASE_IN_OUT)
	)


func _on_mouse_exited() -> void:
	(
		get_tree()
		. create_tween()
		. tween_property(self, "scale", init_var, 0.15)
		. set_trans(Tween.TRANS_LINEAR)
		. set_ease(Tween.EASE_IN_OUT)
	)

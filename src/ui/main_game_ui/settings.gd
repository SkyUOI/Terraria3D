extends Button

@onready var init_var = scale


func _on_mouse_entered() -> void:
	(
		get_tree()
		. create_tween()
		. tween_property(self, "scale", init_var * 1.2, 0.2)
		. set_trans(Tween.TRANS_LINEAR)
		. set_ease(Tween.EASE_IN_OUT)
	)


func _on_mouse_exited() -> void:
	(
		get_tree()
		. create_tween()
		. tween_property(self, "scale", init_var, 0.2)
		. set_trans(Tween.TRANS_LINEAR)
		. set_ease(Tween.EASE_IN_OUT)
	)

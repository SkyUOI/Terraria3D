extends "res://src/ui/start_game/button_base.gd"

@onready var player_choose_panel = $"../../PlayerChoose"
@onready var main_buttons = $".."


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	super._ready()


func _on_pressed() -> void:
	main_buttons.hide()
	player_choose_panel.LoadAndShow()

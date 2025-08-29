extends Control

@onready var bg = $Background
@onready var mask = $Mask
@onready var title = $Title
@onready var tree = $Tree

const res_min_number = 1
const res_max_number = 10
@export var bg_textures: Array[Texture2D]
@export var title_textures: Array[Texture2D]
@export var tree_textures: Array[Texture2D]

func _ready() -> void:
	var selected = randi_range(res_min_number, res_max_number)
	bg.texture = bg_textures[selected - 1]
	var scale_ratio = get_viewport_rect().size.y / 1000
	title.texture = title_textures[selected - 1]
	title.scale = Vector2(scale_ratio, scale_ratio)
	tree.texture = tree_textures[selected - 1]
	tree.scale = Vector2(scale_ratio, scale_ratio)
	get_tree().create_tween().tween_property(mask, "color", Color8(0,0,0,0), 2)
	
	await get_tree().create_timer(3).timeout
	
	var tween = get_tree().create_tween()
	tween.tween_property(mask, "color", Color8(0,0,0,255), 2)
	tween.finished.connect(next)

func next():
	get_tree().change_scene_to_file("res://src/ui/start_game/start_game.tscn")

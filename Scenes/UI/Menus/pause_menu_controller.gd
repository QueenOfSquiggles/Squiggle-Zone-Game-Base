extends Node

@export var pause_menu_scene : PackedScene

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_cancel"):
		print("Pause Menu Controller handling pause input")
		var node = pause_menu_scene.instantiate()
		add_child(node)
		get_viewport().set_input_as_handled()

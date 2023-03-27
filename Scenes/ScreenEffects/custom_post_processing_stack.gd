extends Node

@export var post_process_enabled := true

func _ready():
	if not post_process_enabled:
		queue_free()

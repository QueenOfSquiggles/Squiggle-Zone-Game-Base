extends Node3D

@onready var hud_req := $"HUD Requests"

func _ready() -> void:
	print("World Base is now ready")
	hud_req.call("RequestSubtitle", "This is a subtitle")


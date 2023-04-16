@tool
extends Node3D

"""
Justification for use of GDScript:
This system requires that a variety of values are assigned as a consequence of the changing of the time of day value. This requires a tool script which generally works best as GDSCript compared to C# tool scripts. Perhaps in the future C# tool scripts will be more usable, but as of writing, this is best done in GDSCript
"""

@export_category("Time Settings")
@export var current_time :int = 600 : set=_set_current_time
@export var midnight_wrapping :int = 2400

@export_category("Debugging Settings")
@export_group("Daylight Cycle")
@export var test_daylight_cycle := false
@export var test_daylight_cycle_step := 12.0

@onready var light_angle :Node3D = get_node("LightsChunk")
@onready var sun :DirectionalLight3D = get_node("LightsChunk/Sun")
@onready var moon :DirectionalLight3D = get_node("LightsChunk/Moon")

var has_editor_init := false

func _set_current_time(value : int) -> void:
	if value < 0 or value > midnight_wrapping:
		current_time = value % midnight_wrapping
	else:
		current_time = value
#	print("Time set to %s" % str(current_time))
	_set_time_of_day_artifacts()

func _set_time_of_day_artifacts() -> void:
	if not has_editor_init:
		return
	var is_day := current_time < (midnight_wrapping / 2.0)
	var time_of_segment = float(current_time) / float(midnight_wrapping)

	light_angle.rotation = Vector3(time_of_segment * 2.0 * PI, 0, 0)
	if is_day:
		sun.visible = true
		moon.visible = false
	else:
		sun.visible = false
		moon.visible = true
		

func _process(delta: float) -> void:
	if Engine.is_editor_hint() and not has_editor_init:
		# extra init phase because onready vars don't work well in editor
		light_angle = $LightsChunk
		sun = $LightsChunk/Sun
		moon = $LightsChunk/Moon
		has_editor_init = true
	if Engine.is_editor_hint():
		return
	if test_daylight_cycle:
		_set_current_time(current_time + int(test_daylight_cycle_step * delta))

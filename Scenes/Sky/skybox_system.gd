@tool
extends Node3D

"""
Justification for use of GDScript:
This system requires that a variety of values are assigned as a consequence of the changing of the time of day value. This requires a tool script which generally works best as GDSCript compared to C# tool scripts. Perhaps in the future C# tool scripts will be more usable, but as of writing, this is best done in GDSCript
"""

@export_category("Time Settings")
@export var current_time :int = 0 : set=_set_current_time
@export var midnight_wrapping :int = 2400

@export_category("Debugging Settings")
@export var test_daylight_cycle := false
@export var test_daylight_cycle_step := 12

@onready var light_angle : Node3D = $"LightsChunk"

func _set_current_time(value : int) -> void:
	if value < 0 or value > midnight_wrapping:
		current_time = value % midnight_wrapping
	else:
		current_time = value
#	print("Time set to %s" % str(current_time))
	_set_time_of_day_artifacts()

func _set_time_of_day_artifacts() -> void:
	var is_day := current_time < (midnight_wrapping / 2)
	var time_of_segment = float(current_time) / float(midnight_wrapping)
	var angle = sin(time_of_segment * PI) #map percentage to half phase
	if not light_angle:
		light_angle = $"LightsChunk"
	light_angle.rotation = Vector3(time_of_segment * 2.0 * PI, 0, 0)

func _process(delta: float) -> void:
	if not Engine.is_editor_hint():
		# only process in editor
		return
	if test_daylight_cycle:
		_set_current_time(current_time + (test_daylight_cycle_step * delta))

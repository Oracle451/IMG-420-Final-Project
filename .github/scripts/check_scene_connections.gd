# CI smoke test, run *inside the exported game*:
#   ./SlimeChronicles.x86_64 --headless --script check_scene_connections.gd
#
# Prints the total number of signal connections stored in all exported scenes.
# The workflow compares this with the number of "[connection ...]" lines in the
# source .tscn files. If they differ, the export silently dropped connections
# (this happened when the editor had no Debug build of the C# assembly: the
# ExitPipe -> LevelOne connection vanished and level 2 could never be reached).
extends SceneTree


func _scan(dir_path: String, out: Array) -> void:
	var dir := DirAccess.open(dir_path)
	if dir == null:
		return
	for file in dir.get_files():
		# Exported scenes are listed as "name.tscn.remap".
		var name := file.trim_suffix(".remap")
		if name.ends_with(".tscn"):
			out.append(dir_path.path_join(name))
	for sub in dir.get_directories():
		if not sub.begins_with("."):
			_scan(dir_path.path_join(sub), out)


func _initialize() -> void:
	var scenes: Array = []
	_scan("res://", scenes)
	var total := 0
	for path in scenes:
		var packed := load(path) as PackedScene
		if packed == null:
			push_error("Could not load " + path)
			continue
		total += packed.get_state().get_connection_count()
	print("SCENES_CHECKED=", scenes.size())
	print("SCENE_CONNECTIONS=", total)
	quit()

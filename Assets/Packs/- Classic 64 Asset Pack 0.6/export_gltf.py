import bpy
import sys


blend_path = sys.argv[-1]
print("Exporting : ", blend_path)
bpy.ops.export_scene.gltf(filepath=blend_path)

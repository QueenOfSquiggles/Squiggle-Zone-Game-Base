import os
import glob


def select_option(options: list) -> str:
    input_msg = "Select from the list:\n"
    for index, item in enumerate(options):
        input_msg += f'{index+1}) {item}\n'
    input_msg += "#] "
    found_match = False
    user_input = ""
    while not found_match:
        user_input = input(input_msg)
        if user_input.lower() in options:
            return user_input.lower()
        if (int(user_input)-1) in range(len(options)):
            return options[int(user_input)-1]
    return ""


def get_all_blend_files() -> list:
    # just as a test let's focus on one
    return glob.glob("./**/*.blend")

    #
    #
    #


print("Running bulk export")

supported_file_types = ['gltf', 'fbx', 'obj', 'x3d']
print("Please select a file format")
file_type_choice = select_option(supported_file_types)

if file_type_choice == "":
    print("Failed to select an option")
    quit()

print(f"You have selected {file_type_choice}")

#
#   Run exports
#

blend_export_cmd = "blender {blend_file} --background --python ./export_{file_type}.py -- {export_file}"
target_files = get_all_blend_files()

print(f"Found {len(target_files)} blend files to export.")

if input("List files? #(y/n)] ").lower() == 'y':
    for file in target_files:
        print(f":: {file}")

if input("Continue? #(y/n)] ").lower() != 'y':
    print("Process cancelled. Have a lovely day UwU")
    quit()


for file in target_files:
    print(f"Targeting: {file}")
    export_file_path = file.replace('.blend', f'.{file_type_choice}')
    cmd = blend_export_cmd.format(
        blend_file=file, file_type=file_type_choice, export_file=export_file_path)
    os.system(cmd)

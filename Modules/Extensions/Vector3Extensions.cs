namespace queen.extension;

using Godot;

public static class Vector3Extensions
{

    public static float AngleXZ(this Vector3 vec)
    {
        return Mathf.Atan2(vec.X, vec.Z);
    }
   

}
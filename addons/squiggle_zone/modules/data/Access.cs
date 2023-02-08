using Godot;
using queen.error;

namespace queen.data;

public class Access
{

//
//  Meaningful information
//      Defaults assigned as well
//
    public bool UseSubtitles = true;
    public bool PreventFlashingLights = false;
    public float AudioDecibelLimit = 0.0f;



//
//  Singleton Setup
//
    public static Access Instance
    {
        get {
            if (_instance == null) CreateInstance();
            return _instance;

        }
    }
    private static Access _instance = null;
    private const string FILE_PATH = "user://access.json";

    public static void ForceLoadInstance()
    {
        if (_instance != null) return;
        CreateInstance();
    }

    private static void CreateInstance()
    {   
        _instance = new Access();
        var loaded = Data.Load<Access>(FILE_PATH);
        if (loaded != null) _instance = loaded;
    }

    public static void SaveSettings()
    {
        if (_instance == null) return;
        ApplyChanges();
        Data.Save(_instance, FILE_PATH);
    }

    private static void ApplyChanges()
    {
        // TODO affect limiter
        var effect = AudioServer.GetBusEffect(0, 0) as AudioEffectLimiter;
        Debugging.Assert(effect != null, "Access failed to get the Limiter effect on the Master audio bus.  Make sure the indices are correct!");
        effect.CeilingDb = Instance.AudioDecibelLimit;

    }
}
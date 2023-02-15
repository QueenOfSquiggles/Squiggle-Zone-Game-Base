using Godot;
using queen.error;

namespace queen.data;

public class Access
{


    private const int FONT_GOTHIC = 0;
    private const int FONT_NOTO_SANS = 1;
    private const int FONT_OPEN_DYSLEXIE = 2;
    private const string FONT_PATH_GOTHIC = "res://Assets/Fonts/DelaGothicOne-Regular.ttf";
    private const string FONT_PATH_NOTO_SANS = "res://Assets/Fonts/NotoSans-Regular.ttf";
    private const string FONT_PATH_OPEN_DYSLEXIE = "res://Assets/Fonts/OpenDyslexic-Regular.otf";

//
//  Meaningful information
//      Defaults assigned as well
//
    public bool UseSubtitles = true;
    public bool PreventFlashingLights = false;
    public float AudioDecibelLimit = 0.0f;
    public int FontOption = FONT_OPEN_DYSLEXIE;
    public float EngineTimeScale = 1.0f; 



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
        if (loaded != null)
        {
            _instance = loaded;
            ApplyChanges();
        }
    }

    public static void SaveSettings()
    {
        if (_instance == null) return;
        ApplyChanges();
        Data.Save(_instance, FILE_PATH);
    }

    private static void ApplyChanges()
    {
        // audio limiter
        var effect = AudioServer.GetBusEffect(0, 0) as AudioEffectLimiter;
        Debugging.Assert(effect != null, "Access failed to get the Limiter effect on the Master audio bus.  Make sure the indices are correct!");
        effect.CeilingDb = Instance.AudioDecibelLimit;

        // font management
        string path = "";
        switch(Instance.FontOption)
        {
            case FONT_GOTHIC:
                path = FONT_PATH_GOTHIC;
                break;
            case FONT_NOTO_SANS:
                path = FONT_PATH_NOTO_SANS;
                break;
            case FONT_OPEN_DYSLEXIE:
                path = FONT_PATH_OPEN_DYSLEXIE;
                break;
        }
        var font = GD.Load<FontFile>(path);
        if (font == null) Print.Error($"Failed to load font option from file: {path}");
        else ThemeDB.GetProjectTheme().DefaultFont = font;
    }
}
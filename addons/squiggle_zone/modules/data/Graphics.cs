using System;
using queen.error;

namespace queen.data;

public class Graphics
{

//
//  Meaningful information
//      Defaults assigned as well
//
    public bool Bloom = true;
    public bool SSR = true;
    public bool SSAO = true;
    public bool SSIL = false;
    public bool SDFGI = true;
    public float TonemapExposure = 1.0f;
    public float Brightness = 1.0f;
    public float Contrast = 1.0f;
    public float Saturation = 1.0f;

//
//  Callback
//

    public event Action OnGraphicsSettingsChanged;

//
//  Singleton Setup
//
    public static Graphics Instance
    {
        get {
            if (_instance == null) CreateInstance();
            return _instance;

        }
    }
    private static Graphics _instance = null;
    private const string FILE_PATH = "user://graphics.json";

    private static void CreateInstance()
    {   
        _instance = new Graphics();
        var loaded = Data.Load<Graphics>(FILE_PATH);
        if (loaded != null) _instance = loaded;
        else SaveSettings();
    }

    public static void SaveSettings()
    {
        if (_instance == null) return;
        Data.Save(_instance, FILE_PATH);
    }

    public static void MarkGraphicsChanged()
    {
        Instance.OnGraphicsSettingsChanged?.Invoke();
        SaveSettings();
    }
}
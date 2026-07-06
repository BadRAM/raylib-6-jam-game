using Raylib_cs;

namespace GameLibrary;

// Game data assets (things like enemy stats, level names, etc.) can just be hard coded constants in this file. 
public static class Assets
{
    public static Dictionary<string, MusicAsset> Musics = new Dictionary<string, MusicAsset>();

    public static void Load()
    {
        Musics = new Dictionary<string, MusicAsset>()
        {
            {"null_function", new MusicAsset(Resources.Musics["null_function"], "Null Function", "CongusBongus", 162, 0.1f)},
            {"av_adr", new MusicAsset(Resources.Musics["av_adr"], "A Different Reality (Lagoona rmx)", "Andreas Viklund", 145, 0.076f)},
            {"andreas_v_avalanche", new MusicAsset(Resources.Musics["andreas_v_avalanche"], "Avalanche", "Andreas Viklund", 140, 0.166f)},
        };
        
        Console.WriteLine("All Assets loaded OK!");
    }
}
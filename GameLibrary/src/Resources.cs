using Raylib_cs;

namespace GameLibrary;

public static class Resources
{
    public static Dictionary<string, Texture2D> Sprites = new Dictionary<string, Texture2D>();
    public static Dictionary<string, Sound> Sounds = new Dictionary<string, Sound>();
    public static Dictionary<string, Music> Musics = new Dictionary<string, Music>();
    public static Dictionary<string, Font> Fonts = new Dictionary<string, Font>();
    public static Dictionary<string, Shader> Shaders = new Dictionary<string, Shader>();

    public static void Load()
    {
        Console.WriteLine("Detecting game files:");
        
        foreach (string path in Directory.GetFiles(Game.IsWeb ? "/" : Game.Dir, "*", SearchOption.AllDirectories))
        {
            Console.WriteLine("    " + path);
        }
        
        foreach (string spritePath in Directory.GetFiles(Game.Dir + "sprite/", "*.png", SearchOption.AllDirectories))
        {
            Sprites.Add(Path.GetFileNameWithoutExtension(spritePath), Raylib.LoadTexture(spritePath));
            Raylib.SetTextureFilter(Sprites[Path.GetFileNameWithoutExtension(spritePath)], TextureFilter.Bilinear);
        }
        
        foreach (string soundPath in Directory.GetFiles(Game.Dir + "sound/", "*", SearchOption.AllDirectories))
        {
            Sounds.Add(Path.GetFileNameWithoutExtension(soundPath), Raylib.LoadSound(soundPath));
        }
        
        foreach (string musicPath in Directory.GetFiles(Game.Dir + "music/", "*", SearchOption.AllDirectories))
        {
            Musics.Add(Path.GetFileNameWithoutExtension(musicPath), Raylib.LoadMusicStream(musicPath));
        }
        
        foreach (string fontPath in Directory.GetFiles(Game.Dir + "font/", "*", SearchOption.AllDirectories))
        {
            Fonts.Add(Path.GetFileNameWithoutExtension(fontPath), Raylib.LoadFont(fontPath));
        }
        
        // Fonts.Add("sd_auto_pilot", Raylib.LoadFontEx(Game.Dir + "font/sd_auto_pilot.ttf", 21, null, 0));
        // Fonts.Add("arcadepix", Raylib.LoadFontEx(Game.Dir + "font/arcadepix.ttf", 10, null, 0));
        
        foreach (string shaderPath in Directory.GetFiles(Game.Dir + "shader/", "*", SearchOption.AllDirectories))
        {
            Shaders.Add(Path.GetFileNameWithoutExtension(shaderPath), Raylib.LoadShader("", shaderPath));
        }

        
        Console.WriteLine("All game files loaded OK!");
    }
}
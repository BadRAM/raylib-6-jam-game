using Raylib_cs;

namespace GameLibrary;

public static class Mixer
{
    public static bool AutoPlay = false;
    public static bool IsPaused = false;
    public static List<MusicAsset> Playlist = new List<MusicAsset>();
    public static MusicAsset? MusicPlaying;
    private static AnimCurve<float> _musicFade = new AnimCurve<float>(0.5f);
    public static List<DialogueAsset> DialogueQueue = new List<DialogueAsset>();
    public static DialogueAsset? DialoguePlaying;

    static Mixer()
    {
        Playlist = new List<MusicAsset>()
        {
            Assets.Musics["null_function"],
            Assets.Musics["av_adr"],
            Assets.Musics["andreas_v_avalanche"],
        };
    }

    public static void Update()
    {
        if (DialoguePlaying != null && !DialoguePlaying.IsPlaying())
        {
            DialoguePlaying = null;
            _musicFade = AnimCurve.NewFloat(_musicFade.Sample(), 0.5f, 1);
        }

        if (DialoguePlaying == null && DialogueQueue.Count > 0)
        {
            DialoguePlaying = DialogueQueue[0];
            DialoguePlaying.Play();
            DialogueQueue.RemoveAt(0);
            _musicFade = AnimCurve.NewFloat(_musicFade.Sample(), 0.2f, 1);
        }
        
        if (MusicPlaying != null)
        {
            Raylib.SetMusicVolume(MusicPlaying.Music, _musicFade.Sample());

            if (!Raylib.IsMusicStreamPlaying(MusicPlaying.Music) && !IsPaused)
            {
                MusicPlaying = null;
            }
        }

        if (MusicPlaying == null && AutoPlay)
        {
            PlayNextMusic();
        }
        
        MusicPlaying?.Update();
    }
    
    public static void PlayMusic(MusicAsset music)
    {
        _musicFade = AnimCurve.NewFloat(0, 0.5f, 1);
        MusicPlaying?.Stop();
        MusicPlaying = music;
        MusicPlaying.Play();
        Game.ScrollText($"Now Playing: {MusicPlaying.Title} - {MusicPlaying.ArtistName}");
    }

    public static void Pause()
    {
        IsPaused = true;
        Raylib.PauseMusicStream(MusicPlaying.Music);
    }

    public static void Resume()
    {
        IsPaused = false;
        Raylib.ResumeMusicStream(MusicPlaying.Music);
    }

    public static void PlayNextMusic()
    {
        var music = new List<MusicAsset>(Playlist); // clone list so we can modify it without breaking referenced static lists.
        if (MusicPlaying != null) music.Remove(MusicPlaying); // Prevent shuffle from picking the song that was already playing.
        if (music.Count == 0) throw new Exception("No music :(");
    
        PlayMusic(music.PickRandom());
    }

    public static void PlayDialogue(DialogueAsset dialogue)
    {
        DialogueQueue.Add(dialogue);
    }

    public static float GetMusicVolume() => _musicFade.Sample();
    
    public static float Beat() => MusicPlaying?.Beat() ?? 0;
    public static float Pulse(float width) => MusicPlaying?.Pulse(width) ?? 0;

    public static bool IsBeatThisFrame()
    {
        return MusicPlaying?.IsBeatThisFrame() ?? false;
    }
}
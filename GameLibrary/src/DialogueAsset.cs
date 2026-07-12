using Raylib_cs;

namespace GameLibrary;

public class DialogueAsset
{
    public int Index;
    public string Text;
    public SoundResource Sound;
    public bool HasBeenPlayed;

    public DialogueAsset(int index, string text)
    {
        Index = index;
        Text = text;
        Sound = Resources.Sounds[$"dialogue{index.ToString().PadLeft(2, '0')}"];
    }
    
    public void Play()
    {
        HasBeenPlayed = true;
        Mixer.PlayDialogue(this);
    }

    public bool PlayIfUnheard(bool important = false)
    {
        if (!important && (Mixer.IsDialoguePlaying() || Game.Level.Gimmick == LevelAsset.LevelGimmick.Kill)) return false;
        if (!HasBeenPlayed)
        {
            Play();
            return true;
        }
        return false;
    }
    
    public bool IsPlaying() => Sound.IsPlaying();
}
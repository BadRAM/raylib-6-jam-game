using Raylib_cs;

namespace GameLibrary;

public class DialogueAsset
{
    public int Index;
    public string Text;
    public SoundResource Sound;

    public DialogueAsset(int index, string text)
    {
        Index = index;
        Text = text;
        Sound = Resources.Sounds[$"dialogue{index.ToString().PadLeft(2, '0')}"];
    }
    
    public void Play()
    {
        Game.ScrollText(Text);
        Sound.Play();
    }
}
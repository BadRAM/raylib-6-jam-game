using Raylib_cs;

namespace GameLibrary;

public class SoundResource
{
    private Sound[] _soundBuffer;
    private int _bufferIndex;
    private float _volume = 1;
    private int _lastPlayed;

    private const int BufferSize = 16;

    public SoundResource(string path)
    {
        _soundBuffer = new Sound[BufferSize];
        _soundBuffer[0] = Raylib.LoadSound(path);
        Raylib.SetSoundVolume(_soundBuffer[0], 0.75f);
        for (int i = 1; i < BufferSize; i++)
        {
            _soundBuffer[i] = Raylib.LoadSoundAlias(_soundBuffer[0]);
        }
    }
    
    public void Play(float pan = 0.5f, float pitch = 1f, float volume = 0.75f)
    {
        if (Raylib.IsSoundPlaying(_soundBuffer[_bufferIndex])) return;
        if (_lastPlayed == Time.Frame) return;
        Raylib.SetSoundPan(_soundBuffer[_bufferIndex], pan);
        Raylib.SetSoundPitch(_soundBuffer[_bufferIndex], pitch);
        Raylib.SetSoundVolume(_soundBuffer[_bufferIndex], volume);
        Raylib.PlaySound(_soundBuffer[_bufferIndex]);
        _bufferIndex++;
        _bufferIndex %= _soundBuffer.Length;
        _lastPlayed = Time.Frame;
    }

    public void PlayRandomPitch(float pan = 0.5f, float pitch = 1f, float volume = 0.75f)
    {
        Play(pan, pitch * (0.8f + Random.Shared.NextSingle() * 0.4f), volume);
    }

    ~SoundResource()
    {
        Game.LateActions.Enqueue(() => Raylib.UnloadSound(_soundBuffer[0]));
    }
}
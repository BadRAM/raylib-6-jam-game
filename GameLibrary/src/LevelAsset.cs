namespace GameLibrary;

public class LevelAsset
{
    public int TargetScore;
    public DialogueAsset? SmallTalk;
    public Action TargetReachedAction;
    public LevelGimmick Gimmick;
    
    public enum LevelGimmick
    {
        None,
        KeepCharging,
        Kill
    }

    public LevelAsset(int targetScore, DialogueAsset? smallTalk, Action targetReachedAction, LevelGimmick levelGimmick = LevelGimmick.None)
    {
        TargetScore = targetScore;
        SmallTalk = smallTalk;
        TargetReachedAction = targetReachedAction;
        Gimmick = levelGimmick;
    }
}
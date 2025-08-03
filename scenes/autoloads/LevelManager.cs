using Godot;

public partial class LevelManager : Node
{
  public static LevelManager Instance { get; private set; }

  [Export]
  private PackedScene[] levelScenes;

  private int currentLevelIndex = 0;

  public override void _Notification(int what)
  {
    base._Notification(what);

    if (what == NotificationSceneInstantiated)
    {
      Instance = this;
    }
  }

  public void ChangeToLevel(int levelIndex)
  {
    if (levelIndex < 0 || levelIndex >= levelScenes.Length)
    {
      GD.PrintErr("Invalid level index: " + levelIndex);
      return;
    }

    currentLevelIndex = levelIndex;


    var levelScene = levelScenes[levelIndex];

    if (levelScene == null)
    {
      GD.PrintErr("Level scene is null for index: " + levelIndex);
      return;
    }

    GD.Print("Changing to level: " + levelIndex + " - " + levelScene.ResourcePath);
    GetTree().ChangeSceneToPacked(levelScene);
  }

  public int CurrentLevelIndex() => currentLevelIndex;

  public void ChangeToNextLevel()
  {
    ChangeToLevel(currentLevelIndex + 1);
  }
}

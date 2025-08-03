using Godot;

public partial class LevelManager : Node
{
  public static LevelManager Instance { get; private set; }

  [Export]
  private PackedScene[] levelScenes;

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
    GD.Print("Changing to level: " + levelIndex);

    if (levelIndex < 0 || levelIndex >= levelScenes.Length)
    {
      GD.PrintErr("Invalid level index: " + levelIndex);
      return;
    }

    var levelScene = levelScenes[levelIndex];

    if (levelScene == null)
    {
      GD.PrintErr("Level scene is null for index: " + levelIndex);
      return;
    }

    GetTree().ChangeSceneToPacked(levelScene);
  }
}

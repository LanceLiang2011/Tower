using Game.Components;
using Godot;


namespace Game.Events;

public partial class GameEvents : Node
{
  public static GameEvents Instance { get; private set; }

  [Signal]
  public delegate void BuildingPlacedEventHandler(BuildingComponent buildingComponent);
  [Signal]
  public delegate void BuildingDestroyedEventHandler(BuildingComponent buildingComponent);

  public override void _Notification(int what)
  {
    base._Notification(what);

    if (what == NotificationSceneInstantiated)
    {
      Instance = this;
    }
  }

  public static void EmitBuildingPlaced(BuildingComponent buildingComponent)
  {
    Instance?.EmitSignal(SignalName.BuildingPlaced, buildingComponent);
  }

  public static void EmitBuildingDestroyed(BuildingComponent buildingComponent)
  {
    Instance?.EmitSignal(SignalName.BuildingDestroyed, buildingComponent);
  }
}

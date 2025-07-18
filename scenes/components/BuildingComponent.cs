using Game.Events;
using Godot;

namespace Game.Components;

public partial class BuildingComponent : Node2D
{
  public float GRID_SIZE = 64f;

  [Export]
  public int BuildingRadius { get; private set; }

  public override void _Ready()
  {
    AddToGroup(nameof(BuildingComponent));

    Callable.From(() => GameEvents.EmitBuildingPlaced(this)).CallDeferred();
  }

  public Vector2I GetBuildingCellPosition()
  {
    GD.Print($"Getting building cell position for {Name} at {GlobalPosition}"); // TODO: Remove this
    return GetGridPositionFromPosition(GlobalPosition);
  }

  public Vector2I GetGridPositionFromPosition(Vector2 position)
  {
    var positionAsFloat = (position / GRID_SIZE).Floor();
    return new Vector2I((int)positionAsFloat.X, (int)positionAsFloat.Y);
  }
}

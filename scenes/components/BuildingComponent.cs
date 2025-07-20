using Game.Events;
using Game.Resources.Building;
using Godot;

namespace Game.Components;

public partial class BuildingComponent : Node2D
{
  public float GRID_SIZE = 64f;

  [Export(PropertyHint.File, "*.tres")]
  public string BuildingResourcePath { get; private set; }

  public BuildingResource buildingResource { get; private set; }


  public override void _Ready()
  {
    AddToGroup(nameof(BuildingComponent));

    Callable.From(() => GameEvents.EmitBuildingPlaced(this)).CallDeferred();

    buildingResource = GD.Load<BuildingResource>(BuildingResourcePath);
  }

  public Vector2I GetBuildingCellPosition()
  {
    return GetGridPositionFromPosition(GlobalPosition);
  }

  public Vector2I GetGridPositionFromPosition(Vector2 position)
  {
    var positionAsFloat = (position / GRID_SIZE).Floor();
    return new Vector2I((int)positionAsFloat.X, (int)positionAsFloat.Y);
  }
}

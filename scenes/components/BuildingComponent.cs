using System.Collections.Generic;
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

  public Vector2I GetGridPositionFromPosition(Vector2 position)
  {
    var positionAsFloat = (position / GRID_SIZE).Floor();
    return new Vector2I((int)positionAsFloat.X, (int)positionAsFloat.Y);
  }

  public Vector2I GetBuildingCellPosition()
  {
    return GetGridPositionFromPosition(GlobalPosition);
  }

  public List<Vector2I> GetListOfOccupiedCells()
  {
    var occupiedCells = new List<Vector2I>();
    var buildingCellPosition = GetBuildingCellPosition();

    for (int x = buildingCellPosition.X; x < buildingCellPosition.X + buildingResource.Dimensions.X; x++)
    {
      for (int y = buildingCellPosition.Y; y < buildingCellPosition.Y + buildingResource.Dimensions.Y; y++)
      {
        occupiedCells.Add(new Vector2I(x, y));
      }
    }

    return occupiedCells;
  }



  public void DestroyBuilding()
  {
    Owner.QueueFree();
  }
}

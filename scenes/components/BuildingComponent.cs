using System.Collections.Generic;
using System.Linq;
using Game.Events;
using Game.Resources.Building;
using Godot;

namespace Game.Components;

public partial class BuildingComponent : Node2D
{
  public float GRID_SIZE = 64f;

  [Export(PropertyHint.File, "*.tres")]
  private string BuildingResourcePath { get; set; }

  public BuildingResource buildingResource { get; private set; }

  private HashSet<Vector2I> occupiedCells = new();


  public override void _Ready()
  {
    if (BuildingResourcePath != null && BuildingResourcePath != "")
    {
      buildingResource = GD.Load<BuildingResource>(BuildingResourcePath);
    }

    AddToGroup(nameof(BuildingComponent));

    Callable.From(Initialize).CallDeferred();


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

  public HashSet<Vector2I> GetListOfOccupiedCells()
  {
    return occupiedCells.ToHashSet(); // Return a copy to avoid external modifications
  }

  public bool IsTileInBuildingArea(Vector2I tilePosition)
  {
    return occupiedCells.Contains(tilePosition);
  }

  // Private methods
  private void Initialize()
  {
    CalculateListOfOccupiedCells();
    GameEvents.EmitBuildingPlaced(this);
  }

  private void CalculateListOfOccupiedCells()
  {
    var buildingCellPosition = GetBuildingCellPosition();

    for (int x = buildingCellPosition.X; x < buildingCellPosition.X + buildingResource.Dimensions.X; x++)
    {
      for (int y = buildingCellPosition.Y; y < buildingCellPosition.Y + buildingResource.Dimensions.Y; y++)
      {
        occupiedCells.Add(new Vector2I(x, y));
      }
    }

  }



  public void DestroyBuilding()
  {
    Owner.QueueFree();
  }
}

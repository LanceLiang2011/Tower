using Game.Components;
using Game.Events;
using Godot;
using System.Collections.Generic;
using System.Linq;


namespace Game.Manager;

public partial class GridManager : Node
{
  private HashSet<Vector2I> validBuildableTilePositions = new();

  public float GRID_SIZE = 64f;


  [Export]
  private TileMapLayer highlightTileMapLayer;
  [Export]
  private TileMapLayer baseTerrainTileMapLayer;


  public override void _Ready()
  {
    base._Ready();

    GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
  }

  public Vector2I GetMouseGridPosition()
  {
    var mousePosition = highlightTileMapLayer.GetGlobalMousePosition();

    return GetGridPositionFromPosition(mousePosition);
  }

  public Vector2I GetGridPositionFromPosition(Vector2 position)
  {
    var positionAsFloat = (position / GRID_SIZE).Floor();
    return new Vector2I((int)positionAsFloat.X, (int)positionAsFloat.Y);
  }

  public void HighlightBuildableTiles()
  {
    foreach (var validBuildablePotion in validBuildableTilePositions)
    {
      highlightTileMapLayer.SetCell(
        validBuildablePotion,
        0,
        Vector2I.Zero
      );
    }
  }

  public void HighlightExpandedBuildableTiles(Vector2I rootCellPosition, int radius)
  {
    highlightTileMapLayer.Clear();
    HighlightBuildableTiles();

    var validCellsInRadius = GetValidCellsInRadius(rootCellPosition, radius).ToHashSet();
    var expandedCellsPosition = validCellsInRadius.Except(validBuildableTilePositions).Except(GetBuildingOccupiesPositions());


    var atlasCoords = new Vector2I(1, 0);

    foreach (var expandedCell in expandedCellsPosition)
    {
      highlightTileMapLayer.SetCell(
        expandedCell,
        0,
        atlasCoords
      );
    }
  }



  public void ClearHighlightedTiles()
  {
    highlightTileMapLayer.Clear();
  }


  private bool IsTilePositionValid(Vector2I tilePosition)
  {
    var customTerrainData = baseTerrainTileMapLayer.GetCellTileData(tilePosition);

    if (customTerrainData == null) return false;

    return customTerrainData.GetCustomData("buildable").As<bool>();
  }

  public bool IsTilePositionBuildable(Vector2I tilePosition)
  {
    return validBuildableTilePositions.Contains(tilePosition);
  }


  private void UpdateValidBuildableTilePositions(BuildingComponent buildingComponent)
  {
    var buildingCellPosition = buildingComponent.GetBuildingCellPosition();
    var buildingRadius = buildingComponent.BuildingRadius;

    var validCellsInRadius = GetValidCellsInRadius(buildingCellPosition, buildingRadius);

    validBuildableTilePositions.UnionWith(validCellsInRadius);

    var allBuildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();

    validBuildableTilePositions.ExceptWith(GetBuildingOccupiesPositions());
  }

  private IEnumerable<Vector2I> GetBuildingOccupiesPositions()
  {
    var allBuildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();

    return allBuildingComponents.Select(b => b.GetBuildingCellPosition());
  }

  private List<Vector2I> GetValidCellsInRadius(Vector2I center, int radius)
  {
    var validCells = new List<Vector2I>();

    for (int x = center.X - radius; x <= center.X + radius; x++)
    {
      for (int y = center.Y - radius; y <= center.Y + radius; y++)
      {
        var tilePosition = new Vector2I(x, y);

        if (IsTilePositionValid(tilePosition))
        {
          validCells.Add(tilePosition);
        }
      }
    }

    return validCells;
  }

  private void OnBuildingPlaced(BuildingComponent buildingComponent)
  {
    UpdateValidBuildableTilePositions(buildingComponent);
  }
}

using Game.Components;
using Godot;
using System.Collections.Generic;
using System.Linq;


namespace Game.Manager;

public partial class GridManager : Node
{
  private HashSet<Vector2I> occupiedTilePositions = new();

  public float GRID_SIZE = 64f;


  [Export]
  private TileMapLayer highlightTileMapLayer;
  [Export]
  private TileMapLayer baseTerrainTileMapLayer;

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
    ClearHighlightedTiles();

    var buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();

    foreach (var buildingComponent in buildingComponents)
    {
      var buildingCellPosition = buildingComponent.GetBuildingCellPosition();

      HighlightValidTilesInRadius(buildingCellPosition, buildingComponent.BuildingRadius);
    }
  }


  public void ClearHighlightedTiles()
  {
    highlightTileMapLayer.Clear();
  }

  public void MarkTileAsOccupied(Vector2I tilePosition)
  {
    occupiedTilePositions.Add(tilePosition);
  }

  public bool IsTilePositionValid(Vector2I tilePosition)
  {

    var customTerrainData = baseTerrainTileMapLayer.GetCellTileData(tilePosition);

    if (customTerrainData == null || !customTerrainData.GetCustomData("buildable").As<bool>()) return false;

    return !occupiedTilePositions.Contains(tilePosition);
  }

  private void HighlightValidTilesInRadius(Vector2I rootCellPosition, int radius)
  {

    for (int x = (int)rootCellPosition.X - radius; x <= (int)rootCellPosition.X + radius; x++)
    {
      for (int y = (int)rootCellPosition.Y - radius; y <= (int)rootCellPosition.Y + radius; y++)
      {
        var tilePosition = new Vector2I(x, y);

        if (!IsTilePositionValid(tilePosition)) continue;

        highlightTileMapLayer.SetCell(
          tilePosition,
          0,
          Vector2I.Zero
        );
      }
    }
  }
}

using Godot;
using System.Collections.Generic;


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
    var mousePositionAsFloat = (mousePosition / GRID_SIZE).Floor();

    return new Vector2I((int)mousePositionAsFloat.X, (int)mousePositionAsFloat.Y);
  }

  public void HighlightValidTilesInRadius(Vector2I rootCellPosition, int radius)
  {
    ClearHighlightedTiles();

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
}

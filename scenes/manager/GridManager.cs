using Game.Components;
using Game.Events;
using Godot;
using System.Collections.Generic;


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

    GD.Print($"Updating valid buildable tile positions for building at {buildingCellPosition} with radius {buildingRadius}"); // TODO: Remove this

    for (int x = buildingCellPosition.X - buildingRadius; x <= buildingCellPosition.X + buildingRadius; x++)
    {
      for (int y = buildingCellPosition.Y - buildingRadius; y <= buildingCellPosition.Y + buildingRadius; y++)
      {
        var tilePosition = new Vector2I(x, y);

        if (!IsTilePositionValid(tilePosition)) continue;

        validBuildableTilePositions.Add(tilePosition);
      }
    }

    validBuildableTilePositions.Remove(buildingCellPosition);

    GD.Print("Valid Buildable Positions:");
    foreach (var position in validBuildableTilePositions)
    {
      GD.Print($"  ({position.X}, {position.Y})");
    }
  }

  private void OnBuildingPlaced(BuildingComponent buildingComponent)
  {
    UpdateValidBuildableTilePositions(buildingComponent);
  }
}

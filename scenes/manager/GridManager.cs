using Game.Components;
using Game.Events;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Game.Manager;

public partial class GridManager : Node
{
  // Constants
  private const string IS_BUILDABLE = "is_buildable";
  private const string IS_WOOD = "is_wood";

  // Signals
  [Signal]
  public delegate void ResourceTilesUpdatedEventHandler(int numberOfTilesCollected);

  // Local data
  private HashSet<Vector2I> validBuildableTilePositions = new();
  private HashSet<Vector2I> collectedResourceTilePositions = new();


  public float GRID_SIZE = 64f;


  [Export]
  private TileMapLayer highlightTileMapLayer;
  [Export]
  private TileMapLayer baseTerrainTileMapLayer;

  private List<TileMapLayer> allTileMapLayers = new();


  public override void _Ready()
  {
    base._Ready();

    GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;

    allTileMapLayers = GetAllTileMapLayers(baseTerrainTileMapLayer);

  }

  public Vector2I GetMouseGridPosition()
  {
    var mousePosition = highlightTileMapLayer.GetGlobalMousePosition();

    return GetGridPositionFromPosition(mousePosition);
  }


  // A recursive method to get all TileMapLayers in the root layer and its children
  private List<TileMapLayer> GetAllTileMapLayers(TileMapLayer rootLayer)
  {
    var result = new List<TileMapLayer>();

    var rootLayerChildren = rootLayer.GetChildren();

    rootLayerChildren.Reverse();

    foreach (var child in rootLayerChildren)
    {
      if (child is TileMapLayer childMapLayer)
      {
        result.AddRange(GetAllTileMapLayers(childMapLayer));
      }
    }

    result.Add(rootLayer);

    return result;
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



  public void HighlightResourceTiles(Vector2I rootCellPosition, int radius)
  {

    var resourceTilesInRadius = GetResourceTilesInRadius(rootCellPosition, radius);


    var atlasCoords = new Vector2I(1, 0);

    foreach (var expandedCell in resourceTilesInRadius)
    {
      highlightTileMapLayer.SetCell(
        expandedCell,
        0,
        atlasCoords
      );
    }
  }

  public void HighlightExpandedBuildableTiles(Vector2I rootCellPosition, int radius)
  {
    var validCellsInRadius = GetValidTilesInRadius(rootCellPosition, radius).ToHashSet();
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

  private bool IsTileWithCustomData(Vector2I tilePosition, string customDataKey)
  {
    foreach (var layer in allTileMapLayers)
    {
      var customTerrainData = layer.GetCellTileData(tilePosition);

      // If the tile is not found, we should check next next layer which is the higher layer in hierarchy
      if (customTerrainData == null) continue;

      // If the lowest layer is found, then whether it has the custom data key determines the validity of the tile position
      return customTerrainData.GetCustomData(customDataKey).As<bool>();
    }

    // This line will be reached if all layers at this position are null. Then this is a null position hence not valid
    return false;
  }


  private bool IsTilePositionValid(Vector2I tilePosition)
  {
    return IsTileWithCustomData(tilePosition, IS_BUILDABLE);

  }

  private bool IsTilePositionResource(Vector2I tilePosition)
  {
    return IsTileWithCustomData(tilePosition, IS_WOOD);
  }

  public bool IsTilePositionBuildable(Vector2I tilePosition)
  {
    return validBuildableTilePositions.Contains(tilePosition);
  }


  private void UpdateValidBuildableTilePositions(BuildingComponent buildingComponent)
  {
    var buildingCellPosition = buildingComponent.GetBuildingCellPosition();
    var buildingRadius = buildingComponent.buildingResource.BuildingRadius;

    var validCellsInRadius = GetValidTilesInRadius(buildingCellPosition, buildingRadius);

    validBuildableTilePositions.UnionWith(validCellsInRadius);

    var allBuildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();

    validBuildableTilePositions.ExceptWith(GetBuildingOccupiesPositions());
  }

  private void UpdateCollectedResourceTilePositions(BuildingComponent buildingComponent)
  {
    var buildingCellPosition = buildingComponent.GetBuildingCellPosition();
    var resourceTiles = GetResourceTilesInRadius(buildingCellPosition, buildingComponent.buildingResource.ResourceCollectionRadius);

    var oldTileCount = collectedResourceTilePositions.Count;

    collectedResourceTilePositions.UnionWith(resourceTiles);

    var newTileCount = collectedResourceTilePositions.Count;

    if (newTileCount != oldTileCount) EmitSignal(SignalName.ResourceTilesUpdated, collectedResourceTilePositions.Count);
  }

  private IEnumerable<Vector2I> GetBuildingOccupiesPositions()
  {
    var allBuildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();

    return allBuildingComponents.Select(b => b.GetBuildingCellPosition());
  }

  private List<Vector2I> GetTilesInRadius(Vector2I center, int radius, Func<Vector2I, bool> predicate)
  {
    var tiles = new List<Vector2I>();

    for (int x = center.X - radius; x <= center.X + radius; x++)
    {
      for (int y = center.Y - radius; y <= center.Y + radius; y++)
      {
        var tilePosition = new Vector2I(x, y);

        if (predicate(tilePosition))
        {
          tiles.Add(tilePosition);
        }
      }
    }

    return tiles;
  }

  private List<Vector2I> GetValidTilesInRadius(Vector2I center, int radius)
  {
    return GetTilesInRadius(center, radius, IsTilePositionValid);
  }

  private List<Vector2I> GetResourceTilesInRadius(Vector2I center, int radius)
  {
    return GetTilesInRadius(center, radius, IsTilePositionResource);
  }

  private void OnBuildingPlaced(BuildingComponent buildingComponent)
  {
    UpdateValidBuildableTilePositions(buildingComponent);
    UpdateCollectedResourceTilePositions(buildingComponent);
  }
}

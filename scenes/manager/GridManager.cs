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
  private const string IS_IGNORED = "is_ignored";

  // Signals
  [Signal]
  public delegate void ResourceTilesUpdatedEventHandler(int numberOfTilesCollected);
  [Signal]
  public delegate void GridStateUpdatedEventHandler();

  // Local data
  private HashSet<Vector2I> validBuildableTilePositions = new();
  private HashSet<Vector2I> collectedResourceTilePositions = new();
  private HashSet<Vector2I> occupiedTilePositions = new();

  public const int GRID_SIZE = 64;


  [Export]
  private TileMapLayer highlightTileMapLayer;
  [Export]
  private TileMapLayer baseTerrainTileMapLayer;

  private List<TileMapLayer> allTileMapLayers = new();


  public override void _Ready()
  {
    base._Ready();

    ConnectSignals();

    allTileMapLayers = GetAllTileMapLayers(baseTerrainTileMapLayer);

  }

  private void ConnectSignals()
  {
    GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
    GameEvents.Instance.BuildingDestroyed += OnBuildingDestroyed;
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



  public void HighlightResourceTiles(Rect2I tileArea, int radius)
  {

    var resourceTilesInRadius = GetResourceTilesInRadius(tileArea, radius);


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

  public void HighlightExpandedBuildableTiles(Rect2I tileArea, int radius)
  {
    var validCellsInRadius = GetValidTilesInRadius(tileArea, radius).ToHashSet();
    var expandedCellsPosition = validCellsInRadius.Except(validBuildableTilePositions).Except(occupiedTilePositions);


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
      if (customTerrainData == null || customTerrainData.GetCustomData(IS_IGNORED).As<bool>()) continue;

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
    occupiedTilePositions.UnionWith(buildingComponent.GetListOfOccupiedCells());

    var buildingCellPosition = buildingComponent.GetBuildingCellPosition();
    var buildingRadius = buildingComponent.buildingResource.BuildingRadius;

    var tileArea = new Rect2I(buildingCellPosition, buildingComponent.buildingResource.Dimensions);

    var validCellsInRadius = GetValidTilesInRadius(tileArea, buildingRadius);

    validBuildableTilePositions.UnionWith(validCellsInRadius);

    var allBuildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();

    validBuildableTilePositions.ExceptWith(occupiedTilePositions);

    EmitSignal(SignalName.GridStateUpdated);
  }

  private void UpdateCollectedResourceTilePositions(BuildingComponent buildingComponent)
  {
    var buildingCellPosition = buildingComponent.GetBuildingCellPosition();

    var tileArea = new Rect2I(buildingCellPosition, buildingComponent.buildingResource.Dimensions);

    var resourceTiles = GetResourceTilesInRadius(tileArea, buildingComponent.buildingResource.ResourceCollectionRadius);

    var oldTileCount = collectedResourceTilePositions.Count;

    collectedResourceTilePositions.UnionWith(resourceTiles);

    var newTileCount = collectedResourceTilePositions.Count;

    if (newTileCount != oldTileCount) EmitSignal(SignalName.ResourceTilesUpdated, collectedResourceTilePositions.Count);

    EmitSignal(SignalName.GridStateUpdated);
  }

  private void RecalculateGrids(BuildingComponent buildingToDestroy)
  {
    validBuildableTilePositions.Clear();
    occupiedTilePositions.Clear();
    collectedResourceTilePositions.Clear();

    var buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>().Where(b => b != buildingToDestroy);

    foreach (var buildingComponent in buildingComponents)
    {
      UpdateValidBuildableTilePositions(buildingComponent);
      UpdateCollectedResourceTilePositions(buildingComponent);
    }

    // Emit the signal with the updated count of collected resource tiles
    EmitSignal(SignalName.ResourceTilesUpdated, collectedResourceTilePositions.Count);
    EmitSignal(SignalName.GridStateUpdated);
  }


  private List<Vector2I> GetTilesInRadius(Rect2I area, int radius, Func<Vector2I, bool> predicate)
  {
    var tiles = new List<Vector2I>();

    for (int x = area.Position.X - radius; x < area.End.X + radius; x++)
    {
      for (int y = area.Position.Y - radius; y < area.End.Y + radius; y++)
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

  private List<Vector2I> GetValidTilesInRadius(Rect2I area, int radius)
  {
    return GetTilesInRadius(area, radius, IsTilePositionValid);
  }

  private List<Vector2I> GetResourceTilesInRadius(Rect2I area, int radius)
  {
    return GetTilesInRadius(area, radius, IsTilePositionResource);
  }

  private void OnBuildingPlaced(BuildingComponent buildingComponent)
  {
    UpdateValidBuildableTilePositions(buildingComponent);
    UpdateCollectedResourceTilePositions(buildingComponent);
  }

  private void OnBuildingDestroyed(BuildingComponent buildingComponent)
  {
    RecalculateGrids(buildingComponent);
  }

  internal void Connect(StringName resourceTilesUpdated, object onGridManagerResourceTilesUpdated)
  {
    throw new NotImplementedException();
  }
}

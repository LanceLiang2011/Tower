using Game.Components;
using Game.Events;
using Game.Utils;
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
  private HashSet<Vector2I> allTilesInBuildingRadius = new();
  private HashSet<Vector2I> collectedResourceTilePositions = new();
  private HashSet<Vector2I> occupiedTilePositions = new();

  public const int GRID_SIZE = 64;


  [Export]
  private TileMapLayer highlightTileMapLayer;
  [Export]
  private TileMapLayer baseTerrainTileMapLayer;

  private List<TileMapLayer> allTileMapLayers = new();
  private Dictionary<TileMapLayer, ElevationLayer> tileMapLayerToElevationLayer = new();


  public override void _Ready()
  {
    base._Ready();

    ConnectSignals();

    allTileMapLayers = GetAllTileMapLayers(baseTerrainTileMapLayer);

    MapTileMapLayerToElevationLayer();

  }

  public override void _ExitTree()
  {
    DisconnectSignals();
  }

  private void ConnectSignals()
  {
    GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
    GameEvents.Instance.BuildingDestroyed += OnBuildingDestroyed;
  }

  private void DisconnectSignals()
  {
    GameEvents.Instance.BuildingPlaced -= OnBuildingPlaced;
    GameEvents.Instance.BuildingDestroyed -= OnBuildingDestroyed;
  }

  public Vector2I GetMouseGridPosition()
  {
    var mousePosition = highlightTileMapLayer.GetGlobalMousePosition();

    return GetGridPositionFromPosition(mousePosition);
  }


  // A recursive method to get all TileMapLayers in the root layer and its children
  private List<TileMapLayer> GetAllTileMapLayers(Node2D rootNode)
  {
    var result = new List<TileMapLayer>();

    var rootLayerChildren = rootNode.GetChildren();

    rootLayerChildren.Reverse();

    foreach (var child in rootLayerChildren)
    {
      if (child is Node2D childNode)
      {
        result.AddRange(GetAllTileMapLayers(childNode));
      }
    }

    if (rootNode is TileMapLayer tmLayer) result.Add(tmLayer);

    return result;
  }

  private void MapTileMapLayerToElevationLayer()
  {
    foreach (var layer in allTileMapLayers)
    {
      ElevationLayer elevationLayer;
      Node startNode = layer;

      do
      {
        var parent = startNode.GetParent();
        elevationLayer = parent as ElevationLayer; // If can't cast, it will be null
        startNode = parent;
      } while (elevationLayer == null && startNode != null);

      tileMapLayerToElevationLayer[layer] = elevationLayer;
    }
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



  private (TileMapLayer, bool) GetTileWithCustomData(Vector2I tilePosition, string customDataKey)
  {
    foreach (var layer in allTileMapLayers)
    {
      var customTerrainData = layer.GetCellTileData(tilePosition);

      // If the tile is not found, we should check next next layer which is the higher layer in hierarchy
      if (customTerrainData == null || customTerrainData.GetCustomData(IS_IGNORED).As<bool>()) continue;

      // If the lowest layer is found, then whether it has the custom data key determines the validity of the tile position
      return (layer, customTerrainData.GetCustomData(customDataKey).As<bool>());
    }

    // This line will be reached if all layers at this position are null. Then this is a null position hence not valid
    return (null, false);
  }


  private bool IsTilePositionValid(Vector2I tilePosition)
  {
    return GetTileWithCustomData(tilePosition, IS_BUILDABLE).Item2;

  }

  private bool IsTilePositionResource(Vector2I tilePosition)
  {
    return GetTileWithCustomData(tilePosition, IS_WOOD).Item2;
  }

  public bool IsTilePositionBuildable(Vector2I tilePosition)
  {
    return validBuildableTilePositions.Contains(tilePosition);
  }

  public bool IsTilePositionInAnyBuildingRadius(Vector2I tilePosition)
  {
    return allTilesInBuildingRadius.Contains(tilePosition);
  }

  public bool IsTileAreaBuildable(Rect2I tileArea)
  {
    var tiles = tileArea.ToTiles();

    for (int x = tileArea.Position.X; x < tileArea.End.X; x++)
    {
      for (int y = tileArea.Position.Y; y < tileArea.End.Y; y++)
      {
        tiles.Add(new Vector2I(x, y));
      }
    }

    if (tiles.Count == 0) return false;

    var firstTileMapLayer = GetTileWithCustomData(tiles[0], IS_BUILDABLE).Item1;

    var targetElevationLayer = tileMapLayerToElevationLayer[firstTileMapLayer];

    return tiles.All(tilePosition =>
    {
      var (layer, isValid) = GetTileWithCustomData(tilePosition, IS_BUILDABLE);
      var elevationLayer = tileMapLayerToElevationLayer[layer];

      return isValid && validBuildableTilePositions.Contains(tilePosition) && elevationLayer == targetElevationLayer;
    });
  }


  private void UpdateValidBuildableTilePositions(BuildingComponent buildingComponent)
  {
    occupiedTilePositions.UnionWith(buildingComponent.GetListOfOccupiedCells());

    var buildingCellPosition = buildingComponent.GetBuildingCellPosition();
    var buildingRadius = buildingComponent.buildingResource.BuildingRadius;

    var tileArea = new Rect2I(buildingCellPosition, buildingComponent.buildingResource.Dimensions);

    var allTiles = GetTilesInRadius(tileArea, buildingRadius, (_) => true);
    allTilesInBuildingRadius.UnionWith(allTiles);

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
    allTilesInBuildingRadius.Clear();

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

  private bool IsTileInsideCircle(Vector2 center, Vector2 tilePosition, int radius)
  {
    var distanceX = center.X - (tilePosition.X + 0.5); // adding 0.5 to center the tile
    var distanceY = center.Y - (tilePosition.Y + 0.5); // adding 0.5 to center the tile

    return (distanceX * distanceX + distanceY * distanceY) <= (radius * radius);
  }


  private List<Vector2I> GetTilesInRadius(Rect2I area, int radius, Func<Vector2I, bool> predicate)
  {
    var tiles = new List<Vector2I>();

    var areaF = area.ToRect2F();

    var areaCenter = areaF.GetCenter();

    var radiusMod = (Mathf.Max(area.Size.X, area.Size.Y) / 2) + radius;


    for (int x = area.Position.X - radius; x < area.End.X + radius; x++)
    {
      for (int y = area.Position.Y - radius; y < area.End.Y + radius; y++)
      {
        var tilePosition = new Vector2I(x, y);

        if (!IsTileInsideCircle(areaCenter, tilePosition, radiusMod) || !predicate(tilePosition)) continue;

        tiles.Add(tilePosition);

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

using Game.Resources.Building;
using Game.UI;
using Godot;

namespace Game.Manager;

public partial class BuildingManager : Node
{

  [Export]
  private GridManager gridManager;
  [Export]
  private GameUi gameUi;
  [Export]
  private Node2D ySortRoot;
  [Export]
  private PackedScene buildingGhostScene;

  // Variables
  private Vector2I? hoveredTilePosition = null;
  private BuildingResource buildingResourceToPlace = null;
  private int currentResourceCount;
  private int startingResourceCount = 6; // TODO: Remove hardcoded value
  private int currentlyUsedResourceCount;
  private Node2D buildingGhostInstance;

  private int AvailableResourceCount => startingResourceCount + currentResourceCount - currentlyUsedResourceCount;


  public override void _Ready()
  {
    ConnectSignals();
  }

  public override void _Process(double delta)
  {
    if (!IsInstanceValid(buildingGhostInstance) || buildingResourceToPlace == null) return;

    var gridPosition = gridManager.GetMouseGridPosition();

    if (buildingGhostInstance != null) buildingGhostInstance.GlobalPosition = new Vector2(gridPosition.X, gridPosition.Y) * gridManager.GRID_SIZE;


    if (hoveredTilePosition == null || hoveredTilePosition != gridPosition)
    {
      hoveredTilePosition = gridPosition;

      gridManager.ClearHighlightedTiles();
      gridManager.HighlightExpandedBuildableTiles(hoveredTilePosition.Value, buildingResourceToPlace.BuildingRadius);
      gridManager.HighlightResourceTiles(hoveredTilePosition.Value, buildingResourceToPlace.ResourceCollectionRadius);
    }


  }

  public override void _UnhandledInput(InputEvent evt)
  {
    if (hoveredTilePosition.HasValue &&
    evt.IsActionPressed("left_click") &&
    gridManager.IsTilePositionBuildable(hoveredTilePosition.Value) &&
    AvailableResourceCount >= buildingResourceToPlace.ResourceCost)
    {
      SpawnBuildingOnHoveredGridPosition();
      hoveredTilePosition = null;
      gridManager.ClearHighlightedTiles();
    }
  }


  private void ConnectSignals()
  {
    gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
    gameUi.PlaceBuildingButtonPressed += HandleTowerBuildingPlacement;
  }


  private void OnResourceTilesUpdated(int resourceCount)
  {
    currentResourceCount = resourceCount;

    GD.Print($"Current resource count: {currentResourceCount}"); // TODO: Remove debug print
  }

  private void HandleTowerBuildingPlacement(BuildingResource buildingResource)
  {
    if (IsInstanceValid(buildingGhostInstance))
    {
      buildingGhostInstance.QueueFree();
      buildingGhostInstance = null;
    }

    buildingGhostInstance = buildingGhostScene.Instantiate<Node2D>();
    ySortRoot.AddChild(buildingGhostInstance);

    var buildingSprite = buildingResource.BuildingSpriteScene.Instantiate<Sprite2D>();
    buildingGhostInstance.AddChild(buildingSprite);

    buildingResourceToPlace = buildingResource;
    gridManager.HighlightBuildableTiles();
  }

  private void SpawnBuildingOnHoveredGridPosition()
  {
    if (hoveredTilePosition == null || buildingResourceToPlace == null) return;

    // charge resource cost first
    currentlyUsedResourceCount += buildingResourceToPlace.ResourceCost;

    // Okay you pay the cost now let's build the building
    var building = buildingResourceToPlace.BuildingScene.Instantiate<Node2D>();
    ySortRoot.AddChild(building);

    building.GlobalPosition = new Vector2(hoveredTilePosition.Value.X, hoveredTilePosition.Value.Y) * gridManager.GRID_SIZE;

    // Remove the ghost building
    buildingGhostInstance?.QueueFree();
    buildingGhostInstance = null;
  }
}

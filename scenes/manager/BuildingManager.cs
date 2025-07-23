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
  private Sprite2D cursor;

  // Variables
  private Vector2I? hoveredTilePosition = null;


  private BuildingResource buildingResourceToPlace = null;


  private int currentResourceCount;
  private int startingResourceCount = 4; // TODO: Remove hardcoded value


  public override void _Ready()
  {
    ConnectSignals();
    SetupNodes();
  }

  public override void _Process(double delta)
  {
    var gridPosition = gridManager.GetMouseGridPosition();

    cursor.GlobalPosition = new Vector2(gridPosition.X, gridPosition.Y) * gridManager.GRID_SIZE;

    if (cursor.Visible && (hoveredTilePosition == null || hoveredTilePosition != gridPosition))
    {
      hoveredTilePosition = gridPosition;

      gridManager.ClearHighlightedTiles();
      gridManager.HighlightExpandedBuildableTiles(hoveredTilePosition.Value, buildingResourceToPlace.BuildingRadius);
      gridManager.HighlightResourceTiles(hoveredTilePosition.Value, buildingResourceToPlace.ResourceCollectionRadius);
    }


  }

  public override void _UnhandledInput(InputEvent evt)
  {
    if (hoveredTilePosition.HasValue && evt.IsActionPressed("left_click") && gridManager.IsTilePositionBuildable(hoveredTilePosition.Value))
    {
      SpawnBuildingOnHoveredGridPosition();
      cursor.Visible = false;
      hoveredTilePosition = null;
      gridManager.ClearHighlightedTiles();
    }
  }


  private void ConnectSignals()
  {
    gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
    gameUi.PlaceBuildingButtonPressed += HandleTowerBuildingPlacement;
  }

  private void SetupNodes()
  {
    cursor.Visible = false;
  }

  private void OnResourceTilesUpdated(int resourceCount)
  {
    currentResourceCount = resourceCount;

    GD.Print($"Current resource count: {currentResourceCount}"); // TODO: Remove debug print
  }

  private void HandleTowerBuildingPlacement(BuildingResource buildingResource)
  {
    buildingResourceToPlace = buildingResource;
    cursor.Visible = true;
    gridManager.HighlightBuildableTiles();
  }

  private void SpawnBuildingOnHoveredGridPosition()
  {
    if (hoveredTilePosition == null || buildingResourceToPlace == null) return;

    var building = buildingResourceToPlace.BuildingScene.Instantiate<Node2D>();
    ySortRoot.AddChild(building);

    building.GlobalPosition = new Vector2(hoveredTilePosition.Value.X, hoveredTilePosition.Value.Y) * gridManager.GRID_SIZE;
  }
}

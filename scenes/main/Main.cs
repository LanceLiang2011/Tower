using Godot;
using Game.Manager;
using Game.Resources.Building;

namespace Game;

public partial class Main : Node
{

  // Nodes
  private GridManager gridManager;
  private Sprite2D cursor;
  private Button placeVillageButton;
  private Button placeTowerButton;
  private Node2D ySortRoot;

  // Resources
  private BuildingResource towerResource;
  private BuildingResource villageResource;

  private BuildingResource buildingResourceToPlace = null;

  // Variables
  private Vector2I? hoveredTilePosition = null;


  public override void _Ready()
  {
    GetNodesAndScenes();
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
      gridManager.HighlightExpandedBuildableTiles(hoveredTilePosition.Value, buildingResourceToPlace.BuildingRadius);
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

  private void GetNodesAndScenes()
  {
    gridManager = GetNode<GridManager>("%GridManager");
    cursor = GetNode<Sprite2D>("%Cursor");
    placeTowerButton = GetNode<Button>("%PlaceTowerButton");
    placeVillageButton = GetNode<Button>("%PlaceVillageButton");
    ySortRoot = GetNode<Node2D>("%YSortRoot");

    towerResource = GD.Load<BuildingResource>("res://resources/building/tower_building_resource.tres");
    villageResource = GD.Load<BuildingResource>("res://resources/building/village_building_resource.tres");
  }

  private void ConnectSignals()
  {
    placeTowerButton.Pressed += HandleTowerPlacement;
    placeVillageButton.Pressed += HandleVillagePlacement;
  }

  private void SetupNodes()
  {
    cursor.Visible = false;
  }



  private void SpawnBuildingOnHoveredGridPosition()
  {
    if (hoveredTilePosition == null || buildingResourceToPlace == null) return;

    var building = buildingResourceToPlace.BuildingScene.Instantiate<Node2D>();
    ySortRoot.AddChild(building);

    building.GlobalPosition = new Vector2(hoveredTilePosition.Value.X, hoveredTilePosition.Value.Y) * gridManager.GRID_SIZE;


  }

  private void HandleTowerPlacement()
  {
    buildingResourceToPlace = towerResource;
    cursor.Visible = true;
    gridManager.HighlightBuildableTiles();
  }

  private void HandleVillagePlacement()
  {
    buildingResourceToPlace = villageResource;
    cursor.Visible = true;
    gridManager.HighlightBuildableTiles();
  }

}

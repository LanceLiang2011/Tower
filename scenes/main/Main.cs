using Godot;
using Game.Manager;

namespace Game;

public partial class Main : Node
{

  // Nodes
  private GridManager gridManager;
  private Sprite2D cursor;
  private Button placeVillageButton;
  private Button placeTowerButton;
  private Node2D ySortRoot;

  // Scenes
  private PackedScene towerScene;
  private PackedScene villageScene;

  private PackedScene buildingSceneToPlace = null;

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
      gridManager.HighlightExpandedBuildableTiles(hoveredTilePosition.Value, 3);
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

    towerScene = GD.Load<PackedScene>("res://scenes/building/tower.tscn");
    villageScene = GD.Load<PackedScene>("res://scenes/building/village.tscn");
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
    if (hoveredTilePosition == null || buildingSceneToPlace == null) return;

    var building = buildingSceneToPlace.Instantiate<Node2D>();

    ySortRoot.AddChild(building);

    building.GlobalPosition = new Vector2(hoveredTilePosition.Value.X, hoveredTilePosition.Value.Y) * gridManager.GRID_SIZE;


  }

  private void HandleTowerPlacement()
  {
    buildingSceneToPlace = towerScene;
    cursor.Visible = true;
    gridManager.HighlightBuildableTiles();
  }

  private void HandleVillagePlacement()
  {
    buildingSceneToPlace = villageScene;
    cursor.Visible = true;
    gridManager.HighlightBuildableTiles();
  }

}

using Godot;
using Game.Manager;

namespace Game;

public partial class Main : Node
{

  // Nodes
  private GridManager gridManager;
  private Sprite2D cursor;
  private Button placeBuildingButton;

  // Scenes
  private PackedScene buildingScene;

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

    if (cursor.Visible && (hoveredTilePosition == null || hoveredTilePosition != gridPosition)) hoveredTilePosition = gridPosition;

    if (hoveredTilePosition.HasValue) gridManager.HighlightBuildableTiles();

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
    placeBuildingButton = GetNode<Button>("%PlaceBuildingButton");

    buildingScene = GD.Load<PackedScene>("res://scenes/building/building.tscn");
  }

  private void ConnectSignals()
  {
    placeBuildingButton.Pressed += OnPlaceBuildingButtonPressed;
  }

  private void SetupNodes()
  {
    cursor.Visible = false;
  }



  private void SpawnBuildingOnHoveredGridPosition()
  {
    if (hoveredTilePosition == null) return;

    var building = buildingScene.Instantiate<Node2D>();

    AddChild(building);

    building.GlobalPosition = new Vector2(hoveredTilePosition.Value.X, hoveredTilePosition.Value.Y) * gridManager.GRID_SIZE;


  }

  private void OnPlaceBuildingButtonPressed()
  {
    cursor.Visible = true;
  }

}

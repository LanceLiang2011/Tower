using Godot;
using Game.Manager;
using Game.Special;
using Game.UI;

namespace Game;

public partial class BaseLevel : Node
{
  // Scenes
  [Export]
  private PackedScene levelCompleteScreenScene;

  // Nodes
  private GridManager gridManager;
  private GoldMine goldMine;
  private PanCamera panCamera;
  private TileMapLayer baseTerrainTileMapLayer;
  private Node2D baseBuilding;
  private GameUi gameUi;

  public override void _Ready()
  {
    GetNodes();
    SetUpNodes();
    ConnectSignals();
  }

  private void GetNodes()
  {
    gridManager = GetNode<GridManager>("%GridManager");
    goldMine = GetNode<GoldMine>("%GoldMine");
    panCamera = GetNode<PanCamera>("%PanCamera");
    baseTerrainTileMapLayer = GetNode<TileMapLayer>("%BaseTerrainTileMapLayer");
    gameUi = GetNode<GameUi>("%GameUI");
    baseBuilding = GetNode<Node2D>("%Base");
  }

  private void SetUpNodes()
  {
    panCamera.SetBoundingRect(baseTerrainTileMapLayer.GetUsedRect());
    panCamera.FocusOnPosition(baseBuilding.GlobalPosition);
  }

  private void ConnectSignals()
  {
    gridManager.GridStateUpdated += OnGridManagerGridStateUpdated;
  }

  private void WinLevel()
  {
    var levelCompleteScreen = levelCompleteScreenScene.Instantiate<LevelCompleteScreen>();
    AddChild(levelCompleteScreen);

    gameUi.HideUI();
  }



  private void OnGridManagerGridStateUpdated()
  {
    // Check if the gold mine cell position is in gridManager's validBuildableTilePositions hash set

    var goldMineCellPosition = gridManager.GetGridPositionFromPosition(goldMine.GlobalPosition);

    if (gridManager.IsTilePositionInAnyBuildingRadius(goldMineCellPosition))
    {
      goldMine.SetActiveTexture();
      WinLevel();
    }

  }
}


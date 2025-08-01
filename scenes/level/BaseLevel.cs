using Godot;
using Game.Manager;
using Game.Special;

namespace Game;

public partial class BaseLevel : Node
{
  // Nodes
  private GridManager gridManager;
  private GoldMine goldMine;
  private PanCamera panCamera;
  private TileMapLayer baseTerrainTileMapLayer;
  private Node2D baseBuilding;

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



  private void OnGridManagerGridStateUpdated()
  {
    // Check if the gold mine cell position is in gridManager's validBuildableTilePositions hash set

    var goldMineCellPosition = gridManager.GetGridPositionFromPosition(goldMine.GlobalPosition);

    if (gridManager.IsTilePositionBuildable(goldMineCellPosition))
    {
      GD.Print("Gold Mine is in a valid buildable position.");
      goldMine.SetActiveTexture();
    }

  }
}


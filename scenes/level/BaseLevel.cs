using Godot;
using Game.Manager;
using Game.Special;

namespace Game;

public partial class BaseLevel : Node
{
  private GridManager gridManager;
  private GoldMine goldMine;

  public override void _Ready()
  {
    GetNodes();
    ConnectSignals();
  }

  private void GetNodes()
  {
    gridManager = GetNode<GridManager>("%GridManager");
    goldMine = GetNode<GoldMine>("%GoldMine");
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


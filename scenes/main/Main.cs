using Godot;
using Game.Manager;
using Game.Resources.Building;

namespace Game;

public partial class Main : Node
{
  private GridManager gridManager;
  private Node2D goldMine;

  public override void _Ready()
  {
    GetNodes();
    ConnectSignals();
  }

  private void GetNodes()
  {
    gridManager = GetNode<GridManager>("%GridManager");
    goldMine = GetNode<Node2D>("%GoldMine");
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
    }
    else
    {
      GD.Print("Gold Mine is NOT in a valid buildable position.");
    }
  }
}


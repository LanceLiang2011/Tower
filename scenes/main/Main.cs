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
  }

  private void GetNodes()
  {
    gridManager = GetNode<GridManager>("%GridManager");
    goldMine = GetNode<Node2D>("%GoldMine");
  }
}

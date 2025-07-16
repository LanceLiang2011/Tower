using Godot;
using System;

namespace Game.Components;

public partial class BuildingComponent : Node2D
{
  public float GRID_SIZE = 64f;

  [Export]
  public int BuildingRadius { get; private set; }

  public override void _Ready()
  {
    AddToGroup(nameof(BuildingComponent));
  }

  public Vector2I GetBuildingCellPosition()
  {
    return GetGridPositionFromPosition(GlobalPosition);
  }

  public Vector2I GetGridPositionFromPosition(Vector2 position)
  {
    var positionAsFloat = (position / GRID_SIZE).Floor();
    return new Vector2I((int)positionAsFloat.X, (int)positionAsFloat.Y);
  }
}

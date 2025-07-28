using Godot;

namespace Game.Resources.Building;

[GlobalClass]
public partial class BuildingResource : Resource
{
  [Export]
  public string BuildingName { get; private set; }
  [Export]
  public Vector2I Dimensions { get; private set; } = Vector2I.One;

  [Export]
  public int BuildingRadius { get; private set; }

  [Export]
  public int ResourceCollectionRadius { get; private set; }
  [Export]
  public int ResourceCost { get; private set; }

  [Export]
  public PackedScene BuildingScene { get; private set; }
  [Export]
  public PackedScene BuildingSpriteScene { get; private set; }
}

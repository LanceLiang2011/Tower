using Godot;

namespace Game.Special;

public partial class GoldMine : Node2D
{
  [Export]
  public Texture2D activeTexture;

  private Sprite2D sprite;

  public override void _Ready()
  {
    sprite = GetNode<Sprite2D>("%Sprite2D");
  }

  public void SetActiveTexture()
  {
    if (activeTexture != null) sprite.Texture = activeTexture;
  }

}

using Game.Manager;
using Godot;


namespace Game;

public partial class PanCamera : Camera2D
{
  private readonly StringName ACTION_PAN_LEFT = "pan_left";
  private readonly StringName ACTION_PAN_RIGHT = "pan_right";
  private readonly StringName ACTION_PAN_UP = "pan_up";
  private readonly StringName ACTION_PAN_DOWN = "pan_down";

  [Export]
  public float panSpeed = 400f;

  public override void _Process(double delta)
  {
    GlobalPosition = GetScreenCenterPosition();
    var movementVector = Input.GetVector(ACTION_PAN_LEFT, ACTION_PAN_RIGHT, ACTION_PAN_UP, ACTION_PAN_DOWN);
    GlobalPosition += movementVector * panSpeed * (float)delta;
  }

  public void SetBoundingRect(Rect2I boundingRect)
  {
    // Set the camera's limit to the bounding rectangle
    LimitLeft = boundingRect.Position.X * GridManager.GRID_SIZE;
    LimitTop = boundingRect.Position.Y * GridManager.GRID_SIZE;
    LimitRight = boundingRect.End.X * GridManager.GRID_SIZE;
    LimitBottom = boundingRect.End.Y * GridManager.GRID_SIZE;
  }

  public void FocusOnPosition(Vector2 position)
  {
    GlobalPosition = position;
  }
}

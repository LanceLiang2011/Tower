using Godot;

public partial class PanCamera : Camera2D
{
  private readonly StringName ACTION_PAN_LEFT = "pan_left";
  private readonly StringName ACTION_PAN_RIGHT = "pan_right";
  private readonly StringName ACTION_PAN_UP = "pan_up";
  private readonly StringName ACTION_PAN_DOWN = "pan_down";

  [Export]
  public float panSpeed = 200f;

  public override void _Process(double delta)
  {
    var movementVector = Input.GetVector(ACTION_PAN_LEFT, ACTION_PAN_RIGHT, ACTION_PAN_UP, ACTION_PAN_DOWN);
    GlobalPosition += movementVector * panSpeed * (float)delta;
  }
}

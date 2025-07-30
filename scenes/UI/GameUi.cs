using System;
using Game.Resources.Building;
using Godot;

namespace Game.UI;

public partial class GameUi : CanvasLayer
{
  [Export]
  private Godot.Collections.Array<BuildingResource> buildingResources;

  [Signal]
  public delegate void PlaceBuildingButtonPressedEventHandler(BuildingResource buildingResource);

  private HBoxContainer buttonsContainer;

  public override void _Ready()
  {
    buttonsContainer = null;

    // foreach (var buildingResource in buildingResources)
    // {
    //   var button = new Button();
    //   button.Text = $"Place {buildingResource.BuildingName}";
    //   button.Name = buildingResource.BuildingName;
    //   buttonsContainer.AddChild(button);

    //   button.Pressed += () => EmitSignal(SignalName.PlaceBuildingButtonPressed, buildingResource);
    // }
  }

}

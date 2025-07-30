using System;
using Game.Resources.Building;
using Godot;

namespace Game.UI;

public partial class GameUi : CanvasLayer
{
  [Export]
  private Godot.Collections.Array<BuildingResource> buildingResources;
  [Export]
  private PackedScene buildingSectionScene;

  [Signal]
  public delegate void PlaceBuildingButtonPressedEventHandler(BuildingResource buildingResource);

  private VBoxContainer buildingSectionsContainer;

  public override void _Ready()
  {
    buildingSectionsContainer = GetNode<VBoxContainer>("%BuildingSectionsContainer");
    CreateBuildingSection();
  }


  private void CreateBuildingSection()
  {
    foreach (var buildingResource in buildingResources)
    {
      var buildingSection = buildingSectionScene.Instantiate<BuildingSection>();
      buildingSectionsContainer.AddChild(buildingSection);
      buildingSection.ConfigureSettingsWithResource(buildingResource);

      // Connect the select button signal to the handler
      buildingSection.SelectButtonPressed += () => EmitSignal(SignalName.PlaceBuildingButtonPressed, buildingResource);
    }
  }
}

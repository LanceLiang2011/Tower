using Game.Manager;
using Game.Resources.Building;
using Godot;

namespace Game.UI;

public partial class GameUi : CanvasLayer
{
  [Export]
  private Godot.Collections.Array<BuildingResource> buildingResources;
  [Export]
  private PackedScene buildingSectionScene;
  [Export]
  private BuildingManager buildingManager;

  [Signal]
  public delegate void PlaceBuildingButtonPressedEventHandler(BuildingResource buildingResource);

  private VBoxContainer buildingSectionsContainer;
  private Label resourceLabel;

  public override void _Ready()
  {
    GetNodes();
    ConnectSignals();
    CreateBuildingSection();
  }

  private void GetNodes()
  {
    buildingSectionsContainer = GetNode<VBoxContainer>("%BuildingSectionsContainer");
    resourceLabel = GetNode<Label>("%ResourceLabel");
  }

  private void ConnectSignals()
  {
    buildingManager.AvailableResourceCountChanged += OnAvailableResourceCountChanged;
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

  private void OnAvailableResourceCountChanged(int availableResourceCount)
  {
    resourceLabel.Text = availableResourceCount.ToString();
  }
}

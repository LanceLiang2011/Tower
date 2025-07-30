using Game.Resources.Building;
using Godot;

public partial class BuildingSection : PanelContainer
{

  [Signal]
  public delegate void SelectButtonPressedEventHandler();

  private Label titleLabel;
  private Button selectButton;


  public override void _Ready()
  {
    GetNodes();
    ConnectSignals();
  }

  public void ConfigureSettingsWithResource(BuildingResource buildingResource)
  {
    titleLabel.Text = buildingResource.BuildingName;
    selectButton.Text = $"Select (Cost: {buildingResource.ResourceCost})";
  }

  private void GetNodes()
  {
    titleLabel = GetNode<Label>("%TitleLabel");
    selectButton = GetNode<Button>("%SelectButton");
  }

  private void ConnectSignals()
  {
    selectButton.Pressed += OnSelectButtonPressed;
  }

  private void OnSelectButtonPressed()
  {
    EmitSignal(SignalName.SelectButtonPressed);
  }
}

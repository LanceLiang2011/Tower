using Game.Resources.Building;
using Godot;

public partial class BuildingSection : PanelContainer
{

  [Signal]
  public delegate void SelectButtonPressedEventHandler();

  private Label titleLabel;
  private Label descriptionLabel;
  private Label costLabel;
  private Button selectButton;


  public override void _Ready()
  {
    GetNodes();
    ConnectSignals();
  }

  public void ConfigureSettingsWithResource(BuildingResource buildingResource)
  {
    titleLabel.Text = buildingResource.BuildingName;
    descriptionLabel.Text = buildingResource.Description;
    costLabel.Text = buildingResource.ResourceCost.ToString();
    selectButton.Text = "Select";
  }

  private void GetNodes()
  {
    titleLabel = GetNode<Label>("%TitleLabel");
    descriptionLabel = GetNode<Label>("%DescriptionLabel");
    costLabel = GetNode<Label>("%CostLabel");
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

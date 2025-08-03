using Godot;

namespace Game.UI;

public partial class MainMenu : Node
{

  private Button playButton;
  private Button optionsButton;
  private Button quitButton;


  public override void _Ready()
  {
    GetNodes();
    ConnectSignals();
  }

  private void GetNodes()
  {
    playButton = GetNode<Button>("%PlayButton");
    optionsButton = GetNode<Button>("%OptionsButton");
    quitButton = GetNode<Button>("%QuitButton");
  }

  private void ConnectSignals()
  {
    playButton.Pressed += OnPlayButtonPressed;
    optionsButton.Pressed += OnOptionsButtonPressed;
    quitButton.Pressed += OnQuitButtonPressed;
  }

  private void OnPlayButtonPressed()
  {
    GD.Print("Play button pressed");
    // Logic to start the game or change to the game scene
    LevelManager.Instance.ChangeToLevel(0); // Assuming level 0 is the first level
  }

  private void OnOptionsButtonPressed()
  {
    GD.Print("Options button pressed");
    // Logic to open options menu
    // This could be a new scene or a popup dialog
  }

  private void OnQuitButtonPressed()
  {
    GD.Print("Quit button pressed");
    // Logic to quit the game
    GetTree().Quit();
  }
}

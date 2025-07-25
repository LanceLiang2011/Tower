using Game.Building;
using Game.Resources.Building;
using Game.UI;
using Godot;

namespace Game.Manager;

public partial class BuildingManager : Node
{
  // State Definitions
  private enum State
  {
    Normal,
    PlacingBuilding,
  }

  // Input actions
  private readonly StringName ACTION_LEFT_CLICK = "left_click";
  private readonly StringName ACTION_RIGHT_CLICK = "right_click";
  private readonly StringName ACTION_CANCEL_BUILDING_PLACEMENT = "cancel_building_placement";


  [Export]
  private GridManager gridManager;
  [Export]
  private GameUi gameUi;
  [Export]
  private Node2D ySortRoot;
  [Export]
  private PackedScene buildingGhostScene;

  // Variables
  private State currentState = State.Normal;
  private Vector2I hoveredTilePosition;
  private BuildingResource buildingResourceToPlace = null;
  private int currentResourceCount;
  private int startingResourceCount = 6; // TODO: Remove hardcoded value
  private int currentlyUsedResourceCount;
  private BuildingGhost buildingGhostInstance;

  private int AvailableResourceCount => startingResourceCount + currentResourceCount - currentlyUsedResourceCount;


  public override void _Ready()
  {
    ConnectSignals();
  }


  public override void _Process(double delta)
  {
    var gridPosition = gridManager.GetMouseGridPosition();


    if (hoveredTilePosition != gridPosition)
    {
      hoveredTilePosition = gridPosition;
      UpdateHoveredGridCell();
    }

    switch (currentState)
    {
      case State.Normal:
        break;
      case State.PlacingBuilding:
        buildingGhostInstance.GlobalPosition = new Vector2(gridPosition.X, gridPosition.Y) * gridManager.GRID_SIZE;
        break;
      default:
        GD.PrintErr($"Unhandled state: {currentState}");
        break;
    }
  }

  public override void _UnhandledInput(InputEvent evt)
  {
    switch (currentState)
    {
      case State.Normal:
        if (evt.IsActionPressed(ACTION_RIGHT_CLICK))
        {
          DestroyBuildingOnHoveredGridPosition();
        }
        break;
      case State.PlacingBuilding:
        if (evt.IsActionPressed(ACTION_CANCEL_BUILDING_PLACEMENT))
        {
          SetState(State.Normal);
        }
        else if (evt.IsActionPressed(ACTION_LEFT_CLICK) &&
        IsBuildingPlaceableOnTile(hoveredTilePosition))
        {
          SpawnBuildingOnHoveredGridPosition();
        }
        break;
      default:
        GD.PrintErr($"Unhandled state: {currentState}");
        break;
    }


  }


  private void ConnectSignals()
  {
    gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
    gameUi.PlaceBuildingButtonPressed += OnBuildingButtonSelected;
  }

  private bool IsBuildingPlaceableOnTile(Vector2I tilePosition) =>
  gridManager.IsTilePositionBuildable(tilePosition) &&
  AvailableResourceCount >= buildingResourceToPlace.ResourceCost;

  private void UpdateGridAndGhostDisplay()
  {
    gridManager.ClearHighlightedTiles();
    gridManager.HighlightBuildableTiles();

    if (IsBuildingPlaceableOnTile(hoveredTilePosition))
    {
      gridManager.HighlightExpandedBuildableTiles(hoveredTilePosition, buildingResourceToPlace.BuildingRadius);
      gridManager.HighlightResourceTiles(hoveredTilePosition, buildingResourceToPlace.ResourceCollectionRadius);
      buildingGhostInstance.SetValid();
    }
    else
    {
      buildingGhostInstance.SetInvalid();
    }
  }


  private void UpdateHoveredGridCell()
  {
    switch (currentState)
    {
      case State.Normal:
        break;
      case State.PlacingBuilding:
        UpdateGridAndGhostDisplay();
        break;
      default:
        GD.PrintErr($"Unhandled state: {currentState}");
        break;
    }
  }


  private void SetState(State newState)
  {
    // Clear the current state
    switch (currentState)
    {
      case State.Normal:
        break;
      case State.PlacingBuilding:
        ClearGhostAndHighlight();
        buildingResourceToPlace = null;
        break;
      default:
        GD.PrintErr($"Unhandled state: {currentState}");
        break;
    }

    currentState = newState;

    // Set up the new state 
    switch (currentState)
    {
      case State.Normal:
        break;
      case State.PlacingBuilding:
        buildingGhostInstance = buildingGhostScene.Instantiate<BuildingGhost>();
        ySortRoot.AddChild(buildingGhostInstance);
        break;
      default:
        GD.PrintErr($"Unhandled state: {currentState}");
        break;
    }
  }

  private void OnResourceTilesUpdated(int resourceCount)
  {
    currentResourceCount = resourceCount;

    GD.Print($"Current resource count: {currentResourceCount}"); // TODO: Remove debug print
  }

  private void OnBuildingButtonSelected(BuildingResource buildingResource)
  {
    SetState(State.PlacingBuilding);

    var buildingSprite = buildingResource.BuildingSpriteScene.Instantiate<Sprite2D>();
    buildingGhostInstance.AddChild(buildingSprite);
    buildingResourceToPlace = buildingResource;

    UpdateGridAndGhostDisplay();
  }

  private void SpawnBuildingOnHoveredGridPosition()
  {
    // charge resource cost first
    currentlyUsedResourceCount += buildingResourceToPlace.ResourceCost;

    // Okay you pay the cost now let's build the building
    var building = buildingResourceToPlace.BuildingScene.Instantiate<Node2D>();
    ySortRoot.AddChild(building);
    building.GlobalPosition = new Vector2(hoveredTilePosition.X, hoveredTilePosition.Y) * gridManager.GRID_SIZE;

    SetState(State.Normal);
  }

  private void DestroyBuildingOnHoveredGridPosition()
  {
  }

  private void ClearGhostAndHighlight()
  {

    // Clear the highlighted tiles
    gridManager.ClearHighlightedTiles();

    // Remove the ghost building if they exist
    if (IsInstanceValid(buildingGhostInstance))
    {
      buildingGhostInstance?.QueueFree();
      buildingGhostInstance = null;
    }
  }


}

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Game.Building;
using Game.Components;
using Game.Events;
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
  [Export]
  private int startingResourceCount = 6;

  // Variables
  private State currentState = State.Normal;
  private Rect2I hoveredGridArea = new(Vector2I.Zero, Vector2I.One); // Default to a one by one grid area at the origin
  private BuildingResource buildingResourceToPlace = null;
  private int currentResourceCount;

  private int currentlyUsedResourceCount;
  private BuildingGhost buildingGhostInstance;

  private int AvailableResourceCount => startingResourceCount + currentResourceCount - currentlyUsedResourceCount;


  public override void _Ready()
  {
    ConnectSignals();
  }


  public override void _Process(double delta)
  {
    var mouseGridPosition = gridManager.GetMouseGridPosition();
    var rootCell = hoveredGridArea.Position;


    if (rootCell != mouseGridPosition)
    {
      hoveredGridArea.Position = mouseGridPosition;
      UpdateHoveredGridCell();
    }

    switch (currentState)
    {
      case State.Normal:
        break;
      case State.PlacingBuilding:
        buildingGhostInstance.GlobalPosition = new Vector2(mouseGridPosition.X, mouseGridPosition.Y) * GridManager.GRID_SIZE;
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
        IsBuildingPlaceableAtArea(hoveredGridArea))
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

  private List<Vector2I> GetTilePositionsInArea(Rect2I tileArea)
  {
    var positions = new List<Vector2I>();

    for (int x = tileArea.Position.X; x < tileArea.End.X; x++)
    {
      for (int y = tileArea.Position.Y; y < tileArea.End.Y; y++)
      {
        positions.Add(new Vector2I(x, y));
      }
    }
    return positions;
  }

  private bool IsBuildingPlaceableAtArea(Rect2I tileArea)
  {
    var tilePositions = GetTilePositionsInArea(tileArea);

    return tilePositions.All(gridManager.IsTilePositionBuildable) && AvailableResourceCount >= buildingResourceToPlace.ResourceCost;
  }


  private void UpdateGridAndGhostDisplay()
  {
    gridManager.ClearHighlightedTiles();
    gridManager.HighlightBuildableTiles();

    if (IsBuildingPlaceableAtArea(hoveredGridArea))
    {
      gridManager.HighlightExpandedBuildableTiles(hoveredGridArea, buildingResourceToPlace.BuildingRadius);
      gridManager.HighlightResourceTiles(hoveredGridArea, buildingResourceToPlace.ResourceCollectionRadius);
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

    hoveredGridArea.Size = buildingResource.Dimensions;

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
    building.GlobalPosition = hoveredGridArea.Position * GridManager.GRID_SIZE;

    SetState(State.Normal);
  }

  private void DestroyBuildingOnHoveredGridPosition()
  {
    var rootCell = hoveredGridArea.Position;

    var building = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>()
    .FirstOrDefault(b => b.buildingResource.IsDeletable && b.GetBuildingCellPosition() == rootCell);

    if (building == null) return;


    // Remove the used resource count
    currentlyUsedResourceCount -= building.buildingResource.ResourceCost;
    // Emit the building destroyed event
    GameEvents.EmitBuildingDestroyed(building);
    // Destroy the building scene
    building.DestroyBuilding();
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

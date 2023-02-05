using System.Numerics;
using Raylib_cs;
using Zoo.defs;
using Zoo.ui;
using Zoo.util;
using Zoo.world;

namespace Zoo.tools; 

public class Tool_FootPath : Tool {
    // Constants
    private const int ButtonSize = 30;
    
    // References
    private List<FootPathDef> allFootPaths;

    // Virtual Properties
    public override string   Name => "Path Tool";
    public override ToolType Type => ToolType.FootPath;
    
    // State
    private          FootPathDef?   currentFootPath;
    private          bool            isDragging;
    private          IntVec2         dragTile;
    private readonly List<ToolGhost> ghosts = new();

    public Tool_FootPath(ToolManager tm) : base(tm) {
        allFootPaths = Find.AssetManager.GetAllDefs<FootPathDef>();
    }

    public override void Set() {
        Ghost.Type   = GhostType.Sprite;
        Ghost.Snap   = true;
        Ghost.Scale  = new Vector2(1, 2);
        Ghost.Offset = new Vector2(0, -1);
    }

    public override void OnInput(InputEvent evt) {
        if (currentFootPath == null) return;
        
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            isDragging = true;
            dragTile   = evt.mouseWorldPos.Floor();

            evt.Consume();
        }

        if (evt.mouseUp == MouseButton.MOUSE_BUTTON_LEFT) {
            isDragging = false;

            TryPlaceGhosts();

            evt.Consume();
        }
        
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_RIGHT && currentFootPath != null) {
            SetFootPath(null);
            evt.Consume();
        }

    }

    private void TryPlaceGhosts() {
        // Return if can't afford
        if (currentFootPath.Cost * ghosts.Count > Find.Zoo.Funds) return;

        List<IntVec2> undoData = new();

        var placed = 0;

        while (ghosts.Any()) {
            var ghost = ghosts.Pop();
            if (ghost.CanPlace) {
                Find.World.FootPaths.PlacePathAtTile(currentFootPath, ghost.Pos.Floor(), isBlueprint: true);
                undoData.Add(ghost.Pos.Floor());
                placed++;
            }
        }

        if (placed > 0) {
            Find.Zoo.DeductFunds(currentFootPath.Cost * placed);

            toolManager.PushAction(new ToolAction() {
                Name = "Place paths",
                Data = undoData,
                Undo = data => {
                    var dataList = (List<IntVec2>)data;
                    var pathType = Find.World.FootPaths.GetFootPathAtTile(dataList[0]).Data;

                    Find.Zoo.AddFunds(pathType.Cost * dataList.Count);

                    foreach (var tile in dataList) {
                        Find.World.FootPaths.RemovePathAtTile(tile);
                    }
                }
            });
        }
    }

    public override void Update() {
        if (currentFootPath == null) return;
        var mousePos = Find.Input.GetMouseWorldPos();

        // TODO: Investigate making Ls
        if (isDragging) {
            // Dragging
            Ghost.Visible = false;

            var xDif       = mousePos.Floor().X - dragTile.X;
            var yDif       = mousePos.Floor().Y - dragTile.Y;
            var horizontal = MathF.Abs(xDif) > MathF.Abs(yDif);
            var length     = (horizontal ? MathF.Abs(xDif) : MathF.Abs(yDif)) + 1;

            // Push new ghosts to reach length
            while (ghosts.Count < length) {
                var ghost = new ToolGhost(toolManager);
                ghost.Type     = GhostType.Sprite;
                ghost.Follow   = false;
                ghost.Graphics = Ghost.Graphics;
                ghost.Offset   = Ghost.Offset;
                ghost.Scale    = Ghost.Scale;
                ghosts.Add(ghost);
            }

            var i = MathF.Floor(dragTile.X);
            var j = MathF.Floor(dragTile.Y);
            foreach(var ghost in ghosts) {
                ghost.Pos = new Vector2(i, j);
                UpdateGhostSprite(ghost);

                if (horizontal) {
                    i += Math.Sign(mousePos.Floor().X - i);
                } else {
                    j += Math.Sign(mousePos.Floor().Y - j);
                }
            }

            // Pop additional ghosts
            while (ghosts.Count > length) {
                ghosts.Pop();
            }
        } else {
            Ghost.Visible = true;

            UpdateGhostSprite(Ghost);
        }
    }

    public override void Render() {
        if (currentFootPath == null) return;
        
        foreach (var ghost in ghosts) {
            ghost.Render();
        }
    }

    public override void OnGUI() {
        Find.UI.DoImmediateWindow("immPathPanel", new Rectangle(10, 60, 200, ButtonSize + GUI.GapSmall * 2), inRect => {
            var i = 0;
            foreach (var footPath in allFootPaths) {
                // TODO: Wrap
                var buttonRect = new Rectangle(i * (ButtonSize + GUI.GapSmall) + GUI.GapSmall, GUI.GapSmall, ButtonSize, ButtonSize);
                
                if (GUI.ButtonEmpty(buttonRect, selected: currentFootPath != null && currentFootPath.Id == footPath.Id))
                    SetFootPath(footPath);
                
                GUI.DrawSubTexture(buttonRect, footPath.GraphicData.Texture, footPath.GraphicData.GetCellBounds(0).BottomHalf());
                
                i++;
            }
        });
    }

    public override bool CanPlace(ToolGhost ghost) {
        // Can't afford
        var ghostCount = Math.Max(ghosts.Count, 1);
        if (currentFootPath.Cost * ghostCount > Find.Zoo.Funds) return false;

        var path = Find.World.FootPaths.GetFootPathAtTile(ghost.Pos.Floor());

        // Path location invalid
        if (path == null) return false;
        // Path already exists
        if (!path.Empty) return false;
        // Can't place on slope corner
        if (Find.World.Elevation.IsPositionSlopeCorner(path.Tile)) return false;
        // Can't place on water
        if (Find.World.Elevation.IsTileWater(path.Tile)) return false;

        return true;
    }

    private void UpdateGhostSprite(ToolGhost ghost) {
        var path = Find.World.FootPaths.GetFootPathAtTile(ghost.Pos.Floor());

        ghost.Visible = path != null;
        if (!ghost.Visible || !ghost.CanPlace) return;

        var (spriteIndex, elevation) = FootPathUtility.GetSpriteInfo(path!);

        ghost.SpriteIndex = (int)spriteIndex;
        ghost.Offset      = new Vector2(0, -1 - elevation);
    }

    private void SetFootPath(FootPathDef data) {
        currentFootPath = data;

        if (currentFootPath != null) {
            Ghost.Graphics = data.GraphicData;
            Ghost.Visible  = true;
        } else {
            Ghost.Visible = false;
        }
    }
}
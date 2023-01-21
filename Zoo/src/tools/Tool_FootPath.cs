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

            List<IntVec2> undoData = new();

            while (ghosts.Any()) {
                var ghost = ghosts.Pop();
                if (ghost.CanPlace) {
                    Find.World.FootPaths.PlacePathAtTile(currentFootPath, ghost.Pos.Floor());
                    undoData.Add(ghost.Pos.Floor());
                }
            }
            
            toolManager.PushAction(new ToolAction() {
                Name = "Place paths",
                Data = undoData,
                Undo = (data) => {
                    foreach (var tile in (List<IntVec2>) data) {
                        Find.World.FootPaths.RemovePathAtTile(tile);
                    }
                }
            });

            evt.Consume();
        }
        
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_RIGHT && currentFootPath != null) {
            SetFootPath(null);
            evt.Consume();
        }

    }

    public override void Update() {
        if (currentFootPath == null) return;
        var mousePos = Find.Input.GetMouseWorldPos();
        
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
                
                GUI.DrawSubTexture(buttonRect, footPath.GraphicData.Sprite, footPath.GraphicData.GetCellBounds(0).BottomHalf());
                
                i++;
            }
        });
    }

    public override bool CanPlace(ToolGhost ghost) {
        var path = Find.World.FootPaths.GetFootPathAtTile(ghost.Pos.Floor());

        if (path == null) return false;
        if (path.Exists) return false;
        if (Find.World.Elevation.IsPositionSlopeCorner(path.Pos)) return false;
        if (Find.World.Elevation.IsTileWater(path.Pos)) return false;

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
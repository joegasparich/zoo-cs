﻿using System.Text.Json.Nodes;
using Raylib_cs;
using Zoo.ui;
using Zoo.util;
using Zoo.world;

namespace Zoo.tools; 

public class Tool_Biome : Tool {
    private const float DefaultRadius      = 0.65f;
    private const int   PlaceIntervalTicks = 5;
    private const int   ButtonSize         = 30;
    
    private Biome                           currentBiome;
    private bool                            isDragging;
    private Dictionary<string, Biome[][][]> oldChunkData = new();

    public override string   Name => "Biome Tool";
    public override ToolType Type => ToolType.Biome;

    public Tool_Biome(ToolManager tm) : base(tm) {}

    public override void Set() {
        SetBiome(Biome.Sand);

        Ghost.Type    = GhostType.Circle;
        Ghost.Radius  = DefaultRadius;
        Ghost.Elevate = true;
    }

    public override void OnInput(InputEvent evt) {
        // Only listen to down and up events so that we can't start dragging from UI
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            isDragging = true;
            oldChunkData.Clear();
            
            evt.Consume();
        }
        if (evt.mouseUp == MouseButton.MOUSE_BUTTON_LEFT) {
            if (!isDragging) return;
            isDragging = false;

            toolManager.PushAction(new ToolAction() {
                Name = "Biome brush",
                // Copy here so we don't lose the data when clearing
                Data = oldChunkData.ToDictionary(entry => entry.Key, entry => entry.Value),
                Undo = data => {
                    foreach (var (key, value) in (Dictionary<string, Biome[][][]>)data) {
                        var pos   = IntVec2.FromString(key);
                        var chunk = Find.World.Biomes.GetChunk(pos.X, pos.Y);
                        chunk.Load(value);
                    }
                }
            });
            
            evt.Consume();
        }
    }

    public override void Update() {
        if (!isDragging || Game.Ticks % PlaceIntervalTicks != 0) return;

        var pos    = Find.Input.GetMouseWorldPos() * BiomeGrid.BiomeScale;
        var radius = Ghost.Radius * BiomeGrid.BiomeScale;
        
        // Save backups for undo
        foreach (var chunk in Find.World.Biomes.GetChunksInRadius(pos, radius)) {
            var key = new IntVec2(chunk.X, chunk.Y).ToString();
            if (!oldChunkData.ContainsKey(key))
                oldChunkData.Add(key, chunk.Save());
        }
        
        // Set biome in a circle
        Find.World.Biomes.SetBiomeInRadius(pos, radius, currentBiome);
    }

    public override void OnGUI() {
        Find.UI.DoImmediateWindow("immBiomePanel", new Rectangle(10, 60, 200, ButtonSize + GUI.GapSmall * 2), inRect => {
            var i = 0;
            foreach (Biome biome in Enum.GetValues(typeof(Biome))) {
                var info = BiomeInfo.Get(biome);
                
                // TODO: Wrap
                var buttonRect = new Rectangle(i * (ButtonSize + GUI.GapSmall) + GUI.GapSmall, GUI.GapSmall, ButtonSize, ButtonSize);

                GUI.DrawRect(buttonRect, info.Colour);
                GUI.HighlightMouseover(buttonRect);
                
                if (currentBiome == biome)
                    GUI.DrawBorder(buttonRect, 2, Color.BLACK);

                if (GUI.ClickableArea(buttonRect))
                    SetBiome(biome);
                
                i++;
            }
        });
    }

    private void SetBiome(Biome biome) {
        currentBiome = biome;
    }
}
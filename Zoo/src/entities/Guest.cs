﻿using System.Numerics;
using Zoo.defs;
using Zoo.world;

namespace Zoo.entities; 

public class Guest : Actor {
    public HashSet<Exhibit> ExhibitsViewed = new ();
    
    public Guest(Vector2 pos, EntityDef? def) : base(pos, def) {}

    public override void Serialise() {
        base.Serialise();
        
        // TODO: Replace with ArchiveReferences
        Find.SaveManager.ArchiveValue("exhibitsViewed", () => ExhibitsViewed.Select(exhibit => exhibit.Id), ids => {
            foreach (var id in ids) {
                ExhibitsViewed.Add(Find.World.Exhibits.GetExhibitById(id));
            }
        });
    }
}
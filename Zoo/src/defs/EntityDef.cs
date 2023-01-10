﻿using System.Text.Json.Serialization;
using Zoo.entities;

namespace Zoo.defs; 

public class EntityDef : Def {
    public List<ComponentData> Components;
    
    public GraphicData? GraphicData => GetComponentData<RenderComponentData>().GraphicData;
    
    // TODO: Cache
    public T? GetComponentData<T>() where T : ComponentData {
        return Components.Find(c => c.GetType() == typeof(T)) as T;
    }
}
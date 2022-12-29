﻿using System.Numerics;

namespace Zoo.entities; 

public class PhysicsComponent : Component {
    private Vector2 velocity = Vector2.Zero;
    private Vector2 force = Vector2.Zero;
    
    public float Mass { get; set; } = 50;
    public float Friction { get; set; } = 0.5f;

    public PhysicsComponent(Entity entity) : base(entity) {}

    public override void Update() {
        entity.Pos = entity.Pos + velocity;
    }

    public override void PostUpdate() {
        // Add force
        velocity += force / Mass;
        // Apply dampening
        velocity *= 1 / (1 + Friction);
        
        force = Vector2.Zero;
    }
    
    public void AddForce(Vector2 force) {
        this.force += force;
    }
}
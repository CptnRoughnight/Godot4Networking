using Godot;
using System;
using System.Collections.Generic;

public partial class Networking : Node2D
{
    private Vector2 sync_position;
    private Vector2 sync_motion_velocity;
    private bool sync_is_jumping;
    
    private bool processed_position;
    
    public Vector2 Sync_position { get => sync_position; set { sync_position = value; processed_position = false; } }
    public bool Processed_position { get => processed_position; set => processed_position = value; }
    public Vector2 Sync_motion_velocity { get => sync_motion_velocity; set => sync_motion_velocity = value; }
    public bool Sync_is_jumping { get => sync_is_jumping; set => sync_is_jumping = value; }
}

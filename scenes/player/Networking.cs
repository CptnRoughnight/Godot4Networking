using Godot;
using System;
using System.Collections.Generic;

public partial class Networking : Node2D
{
    public struct ChangedTiles
    {
        public Vector2i chunkPosition;
        public Vector2i tilePosition;
        public Vector2i alteredTile;
    }

    private Vector2 sync_position;
    private Vector2 sync_motion_velocity;
    private bool sync_is_jumping;
    private Vector2i sync_mapchange_position;
    private Vector2i sync_mapchange_type;
    private Vector2i sync_mapchange_chunk;

    private bool processed_position;
    private bool processed_MapChange;

    public Vector2 Sync_position { get => sync_position; set { sync_position = value; processed_position = false; } }
    public bool Processed_position { get => processed_position; set => processed_position = value; }
    public Vector2 Sync_motion_velocity { get => sync_motion_velocity; set => sync_motion_velocity = value; }
    public bool Sync_is_jumping { get => sync_is_jumping; set => sync_is_jumping = value; }
    

}

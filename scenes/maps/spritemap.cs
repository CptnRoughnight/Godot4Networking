using Godot;
using System;
using System.Collections.Generic;

public partial class spritemap : Node2D
{
    [Export]
    private NodePath cameraPath;
	[Export]
	private int tileWidth;
	[Export]
	private int tileHeight;
  
    [Export]
    private int maxTerrainHeight = 64;
    [Export]
    private FastNoiseLite terrainNoise;


    private PackedScene tileScene;
    private Camera2D camera;
    private Vector2i oldCameraChunk;

    private List<Node2D> loadedChunks;
    private List<Node2D> chunksToUnload;

    // Chunk ist 40x23 tiles

    public override void _Ready()
	{
        camera = GetNode<Camera2D>(cameraPath);
        if (terrainNoise == null)
        {
            terrainNoise = new FastNoiseLite();
            terrainNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        }
        tileScene = ResourceLoader.Load<PackedScene>("res://scenes/maps/tile.tscn");
        CreateChunk(new Vector2i(0,0));
        CreateChunk(new Vector2i(-1, 0));
        CreateChunk(new Vector2i(1, 0));
        CreateChunk(new Vector2i(0, 1));
        CreateChunk(new Vector2i(0, -1));
        CreateChunk(new Vector2i(1, 1));
        CreateChunk(new Vector2i(1, -1));
        CreateChunk(new Vector2i(-1, 1));
        CreateChunk(new Vector2i(-1,-1));
    }


    public override void _PhysicsProcess(double delta)
    {
        var cameraChunk = new Vector2i((int)(camera.GlobalPosition.x / 40), (int)(camera.GlobalPosition.y / 23));
        if (cameraChunk != oldCameraChunk)
        {

            oldCameraChunk = cameraChunk;
        }
    }
    private void CreateChunk(Vector2i chunkPos)
	{
        Node2D chunk = new Node2D();
        chunk.Name = "Chunk"+chunkPos.x.ToString()+chunkPos.y.ToString();
        for (int x = 0; x < 40; x++)
        {
            int height = (int)(((terrainNoise.GetNoise1d(x+chunkPos.x*40) + 1.0f)) * maxTerrainHeight); // 0..maxTerrainHeight
            int n = 0;
            for (int y = height; y < 200; y++)
            {
                Sprite2D tile = (Sprite2D)tileScene.Instantiate();
                tile.Modulate = new Color(1 - n * 0.1f, 1 - n * 0.1f, 1 - n * 0.1f, 1);
                tile.GlobalPosition = new Vector2(x+chunkPos.x*40 * tileWidth, y + chunkPos.y * 23 * tileHeight);
                if ((y+chunkPos.y * 23) == height)
                    tile.Frame = 1;
                chunk.AddChild(tile);
                n++;         
            }
        }
        AddChild(chunk);
    }
}

using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;


[Tool]
public partial class dynamicMap : TileMap
{
	[Signal] public delegate void MapChangedEventHandler(Vector2i chunk, Vector2i position, Vector2i type);

	private struct Chunk
	{
		public Node2D chunkNode;
		public Vector2i chunkPosition;
		public MultiMeshInstance2D foliage;
	}

	private Vector2i DIRT = new Vector2i(0, 0);
	private Vector2i GRASSTOP = new Vector2i(1, 0);

	[Export]
	private int maxTerrainHeight = 64;
   
	[Export]
	private FastNoiseLite terrainNoise;

	[Export]
	private Mesh GrassMesh;
	[Export]
	private Texture2D GrassTexture;
	[Export]
	private int GrassPerChunk = 10;
	[Export]
	private FastNoiseLite vegetationNoise;
	[Export]
	private int ActiveChunks = 3;


	private Camera2D camera = null;
	private Vector2i oldCameraChunk;

	private List<Chunk?> loadedChunks;
	private List<Chunk?> chunksToUnload;

	private const int tileSize = 16;    
	private const int chunkWidth = 40;
	private const int chunkHeight = 23;
	private const int chunkWidthPixels = chunkWidth*tileSize;
	private const int chunkHeightPixels = chunkHeight*tileSize;

	private RandomNumberGenerator rng = new RandomNumberGenerator();

	// Stuff to put onto Map
	private string TreePath = "res://scenes/Trees/tree1.tscn";


	private PackedScene TreeScene;


	
	private bool Sync_MapChanged = false;

    private bool isNetworkAuthority = false;

	public bool IsNetworkAuthority { get => isNetworkAuthority; set => isNetworkAuthority = value; }

	public override void _Ready()
	{
		TreeScene = ResourceLoader.Load<PackedScene>(TreePath);
		
		if (terrainNoise == null)
		{
			terrainNoise = new FastNoiseLite();
			terrainNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
		}
		Clear();
		loadedChunks = new List<Chunk?>();
		chunksToUnload = new List<Chunk?>();

		GameObjects gameobjects = GetNode<GameObjects>("/root/GameObjects");
		gameobjects.Map = this;
		
    }

	public override void _PhysicsProcess(double delta)
	{
		if (camera == null)
			return;

		if (!Engine.IsEditorHint())
		{
			// calculate the current Chunk from the Camera-Position (Centered to the player)
			var cameraChunk = new Vector2i((int)((camera.GlobalPosition.x+320) / chunkWidthPixels), (int)(((camera.GlobalPosition.y+180) / chunkHeightPixels)));
			// adjust so that there is only 1 chunk_0_0
			if (cameraChunk.x < 0)
				cameraChunk.x -= 1;
			if (cameraChunk.y < 0)
				cameraChunk.y -= 1;

			
			// if the current Chunk is not equal to the old chunk
			if (cameraChunk != oldCameraChunk)
			{
				// generate new chunks around the current one
				for (int x = -ActiveChunks; x < ActiveChunks; x++)
					for (int y = -ActiveChunks; y < ActiveChunks; y++)
					{
						Vector2i nc = new Vector2i(x + cameraChunk.x, y + cameraChunk.y);
						generateChunk(nc);
					}
				oldCameraChunk = cameraChunk;

				// calc chunks to delete
				
				for (int x = -ActiveChunks-2; x < ActiveChunks+2; x++)
				{
					deleteChunk(new Vector2i(cameraChunk.x + x, cameraChunk.y - ActiveChunks-1));
					deleteChunk(new Vector2i(cameraChunk.x + x, cameraChunk.y + ActiveChunks + 1));
					deleteChunk(new Vector2i(cameraChunk.x + x, cameraChunk.y - ActiveChunks - 2));
					deleteChunk(new Vector2i(cameraChunk.x + x, cameraChunk.y + ActiveChunks + 2));
				}
				for (int y = -ActiveChunks; y < ActiveChunks; y++)
				{
					deleteChunk(new Vector2i(cameraChunk.x - ActiveChunks - 2, cameraChunk.y + y));
					deleteChunk(new Vector2i(cameraChunk.x + ActiveChunks + 2, cameraChunk.y + y));
					deleteChunk(new Vector2i(cameraChunk.x - ActiveChunks - 1, cameraChunk.y + y));
					deleteChunk(new Vector2i(cameraChunk.x + ActiveChunks + 1, cameraChunk.y + y));
				}
				
			}
		}
	}

	/*
	 * Generates a Chunk, with Height from Noise, some foliage with a Meshinstance2D and Tree Nodes
	 * */
	private void generateChunk(Vector2i chunkPos)
	{
		if (camera.GlobalPosition.x < 0)
			chunkPos.x -= 1;
		if (camera.GlobalPosition.y < 0)
			chunkPos.y -= 1;
		if (isInLoadedChunks(chunkPos))
			return;

		int[] heightArr = new int[chunkWidth];
		bool hasChunkVegetation = false;

		for (int x = 0; x < chunkWidth; x++)
		{
			int height = (int)(((terrainNoise.GetNoise1d(x + chunkPos.x * chunkWidth) + 1.0f)) * maxTerrainHeight);
			heightArr[x] = -1;

			for (int y = 0; y < chunkHeight; y++)
			{
				if ((y + chunkPos.y * chunkHeight) == height)
				{
					SetCell(0, new Vector2i(x + chunkPos.x * chunkWidth, y + chunkPos.y * chunkHeight), 0, GRASSTOP, 0);
					heightArr[x] = y + chunkPos.y * chunkHeight;
					hasChunkVegetation = true;
				}
				else if ((y + chunkPos.y * chunkHeight) > height)
				{
					SetCell(0, new Vector2i(x + chunkPos.x * chunkWidth, y + chunkPos.y * chunkHeight), 0, DIRT, 0);
				}
			}
			
		}
		Node2D chunkNode = new Node2D();
	  

		chunkNode.Name = "Chunk" + chunkPos.x.ToString() + "_" + chunkPos.y.ToString();
		AddChild(chunkNode);
		Chunk c = new Chunk();
		c.chunkNode = chunkNode;
		c.chunkPosition = chunkPos;
		if (hasChunkVegetation)
			c.foliage = placeFoliage(chunkNode,chunkPos,heightArr);
		loadedChunks.Add(c);

		Sprite2D chunkRec = new Sprite2D();
		chunkRec.Texture = ResourceLoader.Load<Texture2D>("res://assets/chunkrect.png");
		chunkRec.GlobalPosition = new Vector2(chunkPos.x * chunkWidthPixels, chunkPos.y * chunkHeightPixels);
		chunkRec.Centered = false;
		chunkNode.AddChild(chunkRec);

	}

	/*
	 * Creates a MuiltiMeshInstance2D to place grass on top of the terrain
	 * */
	private MultiMeshInstance2D placeFoliage(Node2D chunkNode,Vector2i chunkPos,int[] heightArr)
	{

		MultiMeshInstance2D chunkGrass = new MultiMeshInstance2D();
		chunkNode.AddChild(chunkGrass);
		chunkGrass.Multimesh = new MultiMesh();
		chunkGrass.Multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform2d;
		chunkGrass.Multimesh.Mesh = GrassMesh;
		chunkGrass.Texture = GrassTexture;
		chunkGrass.Multimesh.InstanceCount = GrassPerChunk;
		float GrassStep = ((float)chunkWidth /(float)GrassPerChunk);
		float grassCount=0;


		Node2D tree = (Node2D)TreeScene.Instantiate();
		chunkNode.AddChild(tree);
		tree.GlobalPosition = new Vector2(
			chunkPos.x * chunkWidthPixels + 20,
			heightArr[1]*tileSize
			);


		for (int i=0;i<GrassPerChunk;i++)
		{
			if (grassCount < chunkWidth)
			{
				int xpos = rng.RandiRange(0, GrassPerChunk);
				int heightPos = (int)(xpos * GrassStep);
				if (heightPos > 39) heightPos = 39;
				if ((heightArr[heightPos] >= 0))
				{
					chunkGrass.Multimesh.VisibleInstanceCount += 1;
					float x = (chunkPos.x * chunkWidthPixels)+heightPos*tileSize+4;
					var pos = new Vector2(x, (heightArr[heightPos] *tileSize ) - 4);
					var t = new Transform2D(0.0f, pos);
					chunkGrass.Multimesh.SetInstanceTransform2d(i, t);
				}
			}
			grassCount+=GrassStep;
		}

		return chunkGrass;
	}

	// Check if the chunk is currently loaded and active
	private bool isInLoadedChunks(Vector2i chunk)
	{
		foreach (Chunk c in loadedChunks)
			if ((c.chunkPosition.x == chunk.x) && (c.chunkPosition.y == chunk.y))
			{
			 
				return true;
			}
				
		return false;
	}

	// deletes a chunk from List and from Scene
	private void deleteChunk(Vector2i chunk)
	{
		foreach (Chunk c in loadedChunks)
			if ((c.chunkPosition.x == chunk.x) && (c.chunkPosition.y == chunk.y))
			{
				foreach (Node2D ch in GetChildren())
					if (ch.Name.Equals("Chunk" + c.chunkPosition.x.ToString() + "_" + c.chunkPosition.y.ToString()))
						ch.QueueFree();
				loadedChunks.Remove(c);
				break;
			}
	}


	public void setCamera(Camera2D cam)
	{
		camera = cam;
	}

	// Direct Call - NetworkAuthority or Singleplayer 
    public void ChangeCell_Local(Vector2i chunk, Vector2i position, Vector2i type)
    {
        Vector2i pos = new Vector2i(chunk.x * chunkWidth + position.x, chunk.y * chunkHeight + position.y);
        SetCell(0, pos, 0, type, 0);
    }

    // Network Call - Clients
    [RPC(MultiplayerAPI.RPCMode.AnyPeer)]
	private void ChangeCell(Vector2i chunk,Vector2i position,Vector2i type)
	{
		Vector2i pos = new Vector2i(chunk.x*chunkWidth+position.x, chunk.y*chunkHeight+position.y);
		SetCell(0, pos, 0, type, 0);
	}
}


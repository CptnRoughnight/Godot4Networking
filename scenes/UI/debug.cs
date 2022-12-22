using Godot;
using System;

public partial class debug : CanvasLayer
{
	[Export]
	private NodePath playerPath;
	[Export]
	private NodePath cameraPath;
	
	private player _player;
	private Camera2D camera;
	private dynamicMap tilemap;
	private Label fpsLabel;
	private Label camLabel;
	private Label playerLabel;
	private Label chunks;

	public override void _Ready()
	{
		
		fpsLabel = GetNode<Label>("MarginContainer/VBoxContainer/FPS");
		camLabel = GetNode<Label>("MarginContainer/VBoxContainer/CamPos");
		playerLabel = GetNode<Label>("MarginContainer/VBoxContainer/PlayerPos");
		chunks = GetNode<Label>("MarginContainer/VBoxContainer/Chunks");

        _player = GetNode<player>(playerPath);
        camera = GetNode<Camera2D>(cameraPath);
		
    }

	public override void _Process(double delta)
	{
		if (tilemap==null)
		{
            var gameobjects = GetNode<GameObjects>("/root/GameObjects");
			tilemap = gameobjects.Map;
        }
		fpsLabel.Text = "FPS : " + Engine.GetFramesPerSecond().ToString();
		if (camera!=null)
			camLabel.Text = "CAM : " + (camera.GlobalPosition.ToString());
		if (_player != null)
		{
			Vector2 tile = new Vector2();
			tile.x = (int)(_player.GlobalPosition.x / 16.0f);
			tile.y = (int)(_player.GlobalPosition.y / 16.0f);
			Vector2 sector = new Vector2i();
			sector.x = (int)(_player.GlobalPosition.x / 640);
			sector.y = (int)(_player.GlobalPosition.y / 360);
			if (_player.GlobalPosition.x < 0)
				sector.x -= 1;
			if (_player.GlobalPosition.y < 0)
				sector.y -= 1;
			playerLabel.Text = "PLAYER TILE : " + tile.ToString() + " SECTOR " + sector.ToString();
		}
		if (tilemap!=null)
			chunks.Text = "Chunks active : " + tilemap.GetChildCount().ToString();
	}

	
}

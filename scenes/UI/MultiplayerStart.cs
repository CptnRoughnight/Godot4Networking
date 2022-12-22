using Godot;
using System;

public partial class MultiplayerStart : Control
{
	private ENetMultiplayerPeer multiplayer_peer;

	private LineEdit playerName;
	private LineEdit ipAddress;
	private LineEdit port;


	private player PlayerScene;
	private dynamicMap mapScene;

	public override void _Ready()
	{
		playerName = GetNode<LineEdit>("VBoxContainer/HBoxContainer/PlayerName");
		ipAddress = GetNode<LineEdit>("VBoxContainer/HBoxContainer2/IP");
		port = GetNode<LineEdit>("VBoxContainer/HBoxContainer3/Port");
		multiplayer_peer = new ENetMultiplayerPeer();

	}

	private void _on_create_pressed()
	{
		multiplayer_peer.CreateServer(int.Parse(port.Text));
		Multiplayer.MultiplayerPeer = multiplayer_peer;
        multiplayer_peer.PeerConnected += Multiplayer_peer_PeerConnected;
        multiplayer_peer.PeerDisconnected += Multiplayer_peer_PeerDisconnected;
		CanvasLayer l = (CanvasLayer)GetParent();
		l.Visible = false;
        mapScene = (dynamicMap)ResourceLoader.Load<PackedScene>("res://scenes/maps/dynamicMap.tscn").Instantiate();
		mapScene.Name = "dynamicMap";
        GetParent().GetParent().AddChild(mapScene, true);
		mapScene.IsNetworkAuthority = true;

    }

    private void _on_join_pressed()
	{
		multiplayer_peer.CreateClient("127.0.0.1",int.Parse(port.Text));
		Multiplayer.MultiplayerPeer = multiplayer_peer;

        CanvasLayer l = (CanvasLayer)GetParent();
        l.Visible = false;
    }

    private void Multiplayer_peer_PeerConnected(long id)
    {

		
		PlayerScene = (player)ResourceLoader.Load<PackedScene>("res://scenes/player/player.tscn").Instantiate();
        PlayerScene.Name = id.ToString();
		GetParent().GetParent().AddChild(PlayerScene);
    }

    private void Multiplayer_peer_PeerDisconnected(long id)
    {
		GetParent().GetParent().GetNode(id.ToString()).QueueFree();
    }

    private void _on_single_player_pressed()
	{
        var gameobjects = GetNode<GameObjects>("/root/GameObjects");
		gameobjects.IsSinglePlayer = true;
        
		PlayerScene = (player)ResourceLoader.Load<PackedScene>("res://scenes/player/player.tscn").Instantiate();
		GetParent().GetParent().AddChild(PlayerScene);

        mapScene = (dynamicMap)ResourceLoader.Load<PackedScene>("res://scenes/maps/dynamicMap.tscn").Instantiate();
        mapScene.Name = "dynamicMap";
        GetParent().GetParent().AddChild(mapScene, true);
        mapScene.IsNetworkAuthority = true;
        
		
        CanvasLayer l = (CanvasLayer)GetParent();
        l.Visible = false;

    }


}


using Godot;
using System;

public partial class player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	private AnimationTree animationTree;
	private AnimationNodeStateMachinePlayback state_machine;

	private float AnimDirection = 0;
	private bool isJumping = false;

    private Vector2i GRASSTOP = new Vector2i(1, 0);

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private Camera2D camera;
	private MultiplayerSynchronizer synchronizer;
	private Networking networking;

	private dynamicMap map;

	private long peer_id;

	private Label debug;

	private debug DebugLayer;

	private inventar_hotbar hotbar;
    public long Peer_id
	{
		get => peer_id; set
		{
			peer_id = value;
            
            synchronizer.SetMultiplayerAuthority((int)peer_id);
        }
	}

    public override void _Ready()
    {
        var gameobjects = GetNode<GameObjects>("/root/GameObjects");
        synchronizer = GetNode<MultiplayerSynchronizer>("Networking/MultiplayerSynchronizer");
        if (!gameobjects.IsSinglePlayer)
		{
			synchronizer.SetMultiplayerAuthority(int.Parse(Name));
		}

        animationTree = GetNode<AnimationTree>("AnimationTree");
		camera = GetNode<Camera2D>("Camera2D");
		hotbar = GetNode<inventar_hotbar>("InventarHotbar");
		state_machine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
		state_machine.Travel("idle");

		// get the map node and set the camera
		
		gameobjects.OnMapLoaded += onMapLoaded;

		DebugLayer = GetNode<debug>("Debug");

		if (!gameobjects.IsSinglePlayer)
		{
			networking = GetNode<Networking>("Networking");
			camera.Current = is_local_authority();
			debug = GetNode<Label>("Label");
			debug.Text = "";
			DebugLayer.Visible = is_local_authority();
			hotbar.Visible = is_local_authority();
		} else
		{
			camera.Current = true;
			DebugLayer.Visible = true;
			hotbar.Visible = true;
        }

    }

	public void SetCamera(bool value)
	{
		if (camera!=null)
			camera.Current = value;
	}
	private bool is_local_authority()
	{
        var gameobjects = GetNode<GameObjects>("/root/GameObjects");
		if (!gameobjects.IsSinglePlayer)
			return synchronizer.GetMultiplayerAuthority() == Multiplayer.GetUniqueId();
		else
			return true;
	}

    public override void _PhysicsProcess(double delta)
	{
        var gameobjects = GetNode<GameObjects>("/root/GameObjects");
		// Wait for the Map to load
		if (map == null)
		{
			onMapLoaded();
			return;
		}
		
		// if this node is not the local_authority (network-client) and it's a network game
		// then get the values from the synchronizer Node

		if (!is_local_authority() && !gameobjects.IsSinglePlayer)
		{
			if (networking.Processed_position == false)
			{
				Position = networking.Sync_position;
				networking.Processed_position = true;
			}
			Velocity = networking.Sync_motion_velocity;
			isJumping = networking.Sync_is_jumping;
			MoveAndSlide();
            PickNewState();


            return;
		}

		// Change the map - Key T - for testing purpose 
        if (Input.IsActionJustPressed("test"))
        {
            var c = new Vector2i((int)GlobalPosition.x / 640, (int)GlobalPosition.y / 360);
            var px = ((int)GlobalPosition.x / 16)-(c.x*40);
			var py = ((int)GlobalPosition.y / 16)-(c.y*23);

			map.ChangeCell_Local(c, new Vector2i(px + 2, py), GRASSTOP);
            
            if (!gameobjects.IsSinglePlayer)
				map.Rpc("ChangeCell", new Variant[] { c, new Vector2i(px+2,py), GRASSTOP });
        }

        Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.y += gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("move_jump") && IsOnFloor())
		{
			velocity.y = JumpVelocity;
			isJumping = true;
		}
		else if (IsOnFloor())
			isJumping = false;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("move_left", "move_right", "ui_up","ui_down");
		if (direction != Vector2.Zero)
		{
			velocity.x = direction.x * Speed;
		}
		else
		{
			velocity.x = Mathf.MoveToward(Velocity.x, 0, Speed);
		}

		AnimDirection = direction.x;
		Velocity = velocity;
		MoveAndSlide();

		PickNewState();

		// If the game is a Network Game... sync the values to the clients
		if (!gameobjects.IsSinglePlayer)
		{
			networking.Sync_position = Position;
			networking.Sync_motion_velocity = Velocity;
			networking.Sync_is_jumping = isJumping;
		}
	}


	private void PickNewState()
	{
		if (isJumping)
		{
            animationTree.Set("parameters/jump/blend_position", AnimDirection);
            state_machine.Travel("jump");
        } else
		if (Velocity.Length() != 0)
		{
			animationTree.Set("parameters/walk/blend_position", AnimDirection);
			state_machine.Travel("walk");
		} else
		{
			state_machine.Travel("idle");
		}
	}

    [RPC(MultiplayerAPI.RPCMode.AnyPeer)]
	private void changeMap(Vector2i chunk, Vector2i position, Vector2i type)
	{
        if (map != null)
            map.SetCell(0, chunk + position, 0, type, 0);
    }


	private void onMapLoaded()
	{
        var gameobjects = GetNode<GameObjects>("/root/GameObjects");
        map = gameobjects.Map;
		if (map!=null)
		{
		//	GD.PrintErr("map loaded");
			//debug.Text = "map loaded";
			map.IsNetworkAuthority = is_local_authority();
			if (is_local_authority())
				map.setCamera(camera);
        }

    }

}

using Godot;
using System;


/* 
 * this Godot-Singleton is used to store some global stuff... 
 * I had to put the map in here because I had trouble to retrieve the
 * node on clients
 * for now there is the Signal 
 * >[Signal] public delegate void OnMapLoadedEventHandler();<
 * that is fired when the Map is set, so all other nodes that are registered to
 * that event can retrieve a valid dynamicMap Node
 * */

public partial class GameObjects : Node
{
    
	private dynamicMap map;
    [Signal] public delegate void OnMapLoadedEventHandler();
    private bool isSinglePlayer;

    public dynamicMap Map
    {
        get => map;
        set
        {
            map = value;
            EmitSignal(SignalName.OnMapLoaded);

        }
    }

    public bool IsSinglePlayer { get => isSinglePlayer; set => isSinglePlayer = value; }
}

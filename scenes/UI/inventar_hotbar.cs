using Godot;
using System;

public partial class inventar_hotbar : CanvasLayer
{
	private Vector2 selectStartPosition = new Vector2(4, 5);
	private Vector2 selectPosition = new Vector2();

	private TextureRect selectRect;
    private TextureRect inventory;

    private int currentSelectedId = 0;

    private bool isInventoryOpen = false;

	public override void _Ready()
	{
		selectRect = GetNode<TextureRect>("MarginContainer/VBoxContainer/HBoxContainer4/HBoxContainer2/Hotbar/SelectRect");
        inventory = GetNode<TextureRect>("MarginContainer/VBoxContainer/HBoxContainer4/HBoxContainer2/Hotbar/inventory");
        inventory.Visible = false;
        selectRect.Position = selectStartPosition;
    }

	public override void _Process(double delta)
	{
        if (Input.IsActionJustPressed("hotbar_1"))
            currentSelectedId = 0;

        if (Input.IsActionJustPressed("hotbar_2"))
            currentSelectedId = 1;

        if (Input.IsActionJustPressed("hotbar_3"))
            currentSelectedId = 2;

        if (Input.IsActionJustPressed("hotbar_4"))
            currentSelectedId = 3;

        if (Input.IsActionJustPressed("hotbar_5"))
            currentSelectedId = 4;

        if (Input.IsActionJustPressed("hotbar_6"))
            currentSelectedId = 5;

        if (Input.IsActionJustPressed("hotbar_7"))
            currentSelectedId = 6;

        if (Input.IsActionJustPressed("hotbar_8"))
            currentSelectedId = 7;

        if (Input.IsActionJustReleased("hotbar_plus"))
            currentSelectedId = (currentSelectedId + 1)%8;

        if (Input.IsActionJustReleased("hotbar_minus"))
        {
            currentSelectedId = (currentSelectedId - 1);
            if (currentSelectedId < 0)
                currentSelectedId = 7;
        }

        if (Input.IsActionJustPressed("open_inventory"))
        {
            if (isInventoryOpen == false)
            {
                var tween = inventory.CreateTween();
                tween.SetTrans(Tween.TransitionType.Circ);
                tween.SetEase(Tween.EaseType.InOut);
                tween.TweenProperty(inventory, "position", new Vector2(0, 0), 0.6f);
                
                tween.Play();
                inventory.Visible = true;
                isInventoryOpen = true;
            } else
            {
                Callable tweenDone = new Callable(this, nameof(TweenDone));
                var tween = inventory.CreateTween();
                tween.SetTrans(Tween.TransitionType.Circ);
                tween.SetEase(Tween.EaseType.InOut);
                tween.TweenProperty(inventory, "position", new Vector2(0, -300), 0.6f);
                
                tween.TweenCallback(tweenDone);
                tween.Play();
            }

        }

        selectPosition = selectStartPosition + new Vector2(34 * currentSelectedId, 0);
        selectRect.Position = selectPosition;
    }

    private void TweenDone()
    {
        inventory.Visible = false;
        isInventoryOpen = false;
    }
}

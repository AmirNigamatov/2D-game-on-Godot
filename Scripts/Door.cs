using Godot;
using System;

public partial class Door : Area2D
{
	[Export] private string nextScenePath;
	[Export] private int X;
	[Export] private int Y;
	
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}
	
	private void OnBodyEntered(Node2D body)
	{
		GD.Print($"Door: вошёл {body.Name}");
		if (body is CharacterBody2D)
		{
			Callable.From(EnterDoor).CallDeferred();
		}
	}
	
	private void EnterDoor()
	{
		var gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		gameManager.Position = new Vector2(X, Y);
		gameManager.Position = new Vector2(X, Y);
		GetTree().ChangeSceneToFile(nextScenePath);
	}
}

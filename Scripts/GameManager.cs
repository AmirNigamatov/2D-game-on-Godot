using Godot;
using System;

public partial class GameManager : Node
{
	public int Health { get; set; } = 100;
	public Vector2 Position { get; set; } = new Vector2(400, 300);
}

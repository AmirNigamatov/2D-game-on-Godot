using Godot;
using System;

public partial class Dummy : StaticBody2D, IHittable
{
	[Export] private int Health = 50;
	[Export] private float respawnDelay = 5.0f;
	
	private int currentHealth;
	private bool isBroken = false;
	
	private Sprite2D Normal;
	private Sprite2D Broken;
	private Timer respawnTimer;
	
	public override void _Ready()
	{
		Normal = GetNode<Sprite2D>("Normal");
		Broken = GetNode<Sprite2D>("Broken");
		
		respawnTimer = new Timer();
		respawnTimer.WaitTime = respawnDelay;
		respawnTimer.OneShot = true;
		respawnTimer.Timeout += OnRespawnFinished;
		AddChild(respawnTimer);
		ResetState();
	}
	
	private void ResetState()
	{
		currentHealth = Health;
		isBroken = false;
		Normal.Visible = true;
		Broken.Visible = false;
	}
	
	public void TakeDamage(int amount) 
	{
		if (isBroken)
		{
			return;
		}
		
		currentHealth -= amount;
		
		if (currentHealth <= 0 && !isBroken)
		{
			BreakDummy();
		}
	}
	
	private void BreakDummy()
	{
		isBroken = true;
		Normal.Visible = false;
		Broken.Visible = true;
		respawnTimer.Start();
	}
	
	private void OnRespawnFinished()
	{
		ResetState();
	}
}

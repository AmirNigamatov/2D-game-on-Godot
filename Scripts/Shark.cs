using Godot;
using System;

public partial class Shark : CharacterBody2D, IHittable
{
	[Export] private int health = 30;
	[Export] private float verticalSpeed = 150f;
	[Export] private float range = 250f;
	
	[Export] private float laserCooldown = 3f;
	[Export] private float laserDuration = 2.0f;
	[Export] private float laserTickInterval = 0.5f;
	[Export] private int laserDamage = 10;
	
	private int currentHealth;
	private Vector2 startPos;
	private float currentOffset;
	private bool goingUp = true;
	
	private Area2D laserArea;
	private Timer cooldownTimer;
	private Timer durationTimer;
	private Timer tickTimer;
	
	[Export] private PackedScene bubbleScene;
	[Export] private int bubbleCount = 20;
	[Export] private float bubbleSpawnCooldown = 6f;
	[Export] private float bubbleSpawnRadius = 60f;
	private Timer bubbleTimer;
	
	public override void _Ready()
	{
		currentHealth = health;
		startPos = GlobalPosition;
		currentOffset = 0f;
		
		laserArea = GetNode<Area2D>("Laser");
		laserArea.Visible = false;
		laserArea.Monitoring = false;
		laserArea.BodyEntered += OnLaserBodyEntered;

		cooldownTimer = new Timer();
		cooldownTimer.WaitTime = laserCooldown;
		cooldownTimer.OneShot = false;
		cooldownTimer.Timeout += ActivateLaser;
		AddChild(cooldownTimer);
		cooldownTimer.Start();

		durationTimer = new Timer();
		durationTimer.WaitTime = laserDuration;
		durationTimer.OneShot = true;
		durationTimer.Timeout += DeactivateLaser;
		AddChild(durationTimer);

		tickTimer = new Timer();
		tickTimer.WaitTime = laserTickInterval;
		tickTimer.Timeout += DamageInBeam;
		AddChild(tickTimer);
		
		bubbleTimer = new Timer();
		bubbleTimer.WaitTime = bubbleSpawnCooldown;
		bubbleTimer.OneShot = false;
		bubbleTimer.Timeout += SpawnBubbles;
		AddChild(bubbleTimer);
		bubbleTimer.Start();
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (goingUp)
		{
			currentOffset += (float)delta * verticalSpeed;
			if (currentOffset >= range)
			{
				currentOffset = range;
				goingUp = false;
			}
		}
		else
		{
			currentOffset -= (float)delta * verticalSpeed;
			if (currentOffset <= -range)
			{
				currentOffset = -range;
				goingUp = true;
			}
		}
		
		Velocity = new Vector2(0, goingUp ? verticalSpeed : -verticalSpeed);
		MoveAndSlide();
	}
	
	public void TakeDamage(int amount)
	{
		currentHealth -= amount;
		if (currentHealth <= 0)
		{
			Die();
		}
	}
	
	public void Die()
	{
		QueueFree();
	}
	
	private void ActivateLaser()
	{
		laserArea.Visible = true;
		laserArea.Monitoring = true;

		DamageInBeam();
		durationTimer.Start();
		tickTimer.Start();
	}

	private void DeactivateLaser()
	{
		laserArea.Visible = false;
		laserArea.Monitoring = false;
		tickTimer.Stop();
	}

	private void DamageInBeam()
	{
		foreach (var body in laserArea.GetOverlappingBodies())
		{
			if (body is IHittable hittable)
			{
				hittable.TakeDamage(laserDamage);
			}
		}
	}

	private void OnLaserBodyEntered(Node body)
	{
		if (body is IHittable hittable)
		{
			hittable.TakeDamage(laserDamage);
		}
	}
	
	private void SpawnBubbles()
	{
		if (bubbleScene == null)
		{
			return;
		}

		for (int i = 0; i < bubbleCount; i++)
		{
			var bubble = bubbleScene.Instantiate<Bubble>();
			GetParent().AddChild(bubble);
			Vector2 offset = new Vector2((float)GD.RandRange(-bubbleSpawnRadius, bubbleSpawnRadius),(float)GD.RandRange(-bubbleSpawnRadius, bubbleSpawnRadius));
			bubble.GlobalPosition = GlobalPosition + offset;
		}
	}
}

using Godot;
using System;

public partial class Crab : CharacterBody2D, IHittable
{
	[Export] private float moveSpeed = 200f;
	[Export] private float attackCooldown = 0.5f;
	[Export] private float attackDuration = 0.3f;
	
	private Area2D attackArea;
	
	private float cooldownTimer;
	private bool isAttacking;
	
	int Health;
	
	public override void _Ready()
	{
		AddToGroup("player");
		var gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		if (gameManager != null)
		{
			GlobalPosition = gameManager.Position;
			Health = gameManager.Health;
		}
		else
		{
			GlobalPosition = new Vector2(400, 300);
		}
		
		attackArea = GetNode<Area2D>("Attack/Area2D");
		
		attackArea.Visible = false;
		attackArea.Monitoring = false;
		attackArea.Monitorable = false;
		
		attackArea.BodyEntered += OnBodyEntered;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		HandleMovement((float)delta);
		HandleAttack((float)delta);
	}
	
	private void HandleMovement(float delta)
	{
		var velocity = Vector2.Zero;
		if (Input.IsActionPressed("ui_up"))
		{
			velocity.Y -= 1;
		}
		if (Input.IsActionPressed("ui_down"))
		{
			velocity.Y += 1;
		}
		if (Input.IsActionPressed("ui_left"))
		{
			velocity.X -= 1;
		}
		if (Input.IsActionPressed("ui_right"))
		{
			velocity.X += 1;
		}
		
		if (velocity.Length() > 0)
		{
			velocity = velocity.Normalized();
		}
		Velocity = velocity * moveSpeed;
		MoveAndSlide();
	}
	
	private void HandleAttack(float delta)
	{
		if (cooldownTimer > 0)
		{
			cooldownTimer -= delta;
		}
		
		if (Input.IsActionJustPressed("attack") && cooldownTimer <= 0 && !isAttacking)
		{
			PerformAttack();
		}
	}
	
	private void PerformAttack()
	{
		isAttacking = true;
		cooldownTimer = attackCooldown;
		
		attackArea.Visible = true;
		attackArea.Monitoring = true;
		attackArea.Monitorable = true;
		
		var timer = new Timer();
		timer.WaitTime = attackDuration;
		timer.OneShot = true;
		timer.Timeout += () =>
		{
			isAttacking = false;
			attackArea.Visible = false;
			attackArea.Monitoring = false;
			attackArea.Monitorable = false;
		};
		AddChild(timer);
		timer.Start();
	}
	
	private void OnBodyEntered(Node body)
	{
		var hittable = body as IHittable;
		
		if (hittable != null)
		{
			hittable.TakeDamage(10);
			return;
		}
	}
	
	public void TakeDamage(int amount)
	{
		Health -= amount;
		if (Health <= 0)
		{
			Die();
		}
	}
	
	public void Die()
	{
		QueueFree();
	}
}

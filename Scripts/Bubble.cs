using Godot;
using System.Collections.Generic;

public partial class Bubble : Area2D
{
	[Export] public float MaxSpeed = 120f;
	[Export] public float MaxForce = 500f;
	[Export] public float NeighborRadius = 50f;
	[Export] public float SeparationRadius = 25f;
	[Export] public float SeparationWeight = 4f;
	[Export] public float AlignmentWeight = 1.0f;
	[Export] public float CohesionWeight = 0.4f;
	[Export] public float ChaseWeight = 1.5f;
	[Export] public float Lifetime = 10f;
	[Export] public int Damage = 20;
	[Export] public float ChasePredictTime = 0.3f;

	public Vector2 Velocity;
	private float lifeTimer;
	private Node2D target;
	private Vector2 targetLastPos;
	private float chaseJitterTimer;

	public override void _Ready()
	{
		AddToGroup("bubbles");
		BodyEntered += OnBodyEntered;
		AreaEntered += OnAreaEntered;
		
		target = GetTree().GetFirstNodeInGroup("player") as Node2D;
		if (target != null)
			targetLastPos = target.GlobalPosition;

		Velocity = new Vector2((float)GD.RandRange(-1f, 1f), (float)GD.RandRange(-1f, 1f)).Normalized() * MaxSpeed * 0.5f;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		lifeTimer += dt;
		if (lifeTimer >= Lifetime)
		{
			QueueFree();
			return;
		}

		if (target == null || !IsInstanceValid(target))
		{
			target = GetTree().GetFirstNodeInGroup("player") as Node2D;
			if (target != null)
				targetLastPos = target.GlobalPosition;
		}

		var neighbors = GetNeighbors();
		Vector2 steer = Vector2.Zero;
		steer += SeparationWeight * Separation(neighbors);
		steer += AlignmentWeight * Alignment(neighbors);
		steer += CohesionWeight * Cohesion(neighbors);

		if (target != null && IsInstanceValid(target))
		{
			Vector2 currentTargetPos = target.GlobalPosition;
			Vector2 targetVelocity = (currentTargetPos - targetLastPos) / dt;
			Vector2 predictedPos = currentTargetPos + targetVelocity * ChasePredictTime;
			targetLastPos = currentTargetPos;

			chaseJitterTimer += dt;
			if (chaseJitterTimer > 0.5f)
			{
				chaseJitterTimer = 0f;
			}
			Vector2 smallJitter = new Vector2(
				Mathf.Sin(chaseJitterTimer * 10f) * 3f,
				Mathf.Cos(chaseJitterTimer * 7f) * 3f
			);

			steer += ChaseWeight * Seek(predictedPos + smallJitter);
		}

		steer = steer.LimitLength(MaxForce);
		Velocity += steer * dt;
		Velocity = Velocity.LimitLength(MaxSpeed);
		GlobalPosition += Velocity * dt;
	}

	private List<Bubble> GetNeighbors()
	{
		var result = new List<Bubble>();
		foreach (var node in GetTree().GetNodesInGroup("bubbles"))
		{
			if (node is Bubble b
				&& b != this
				&& GlobalPosition.DistanceTo(b.GlobalPosition) <= NeighborRadius)
			{
				result.Add(b);
			}
		}
		return result;
	}

	private Vector2 Separation(List<Bubble> neighbors)
	{
		if (neighbors.Count == 0) return Vector2.Zero;

		var steer = Vector2.Zero;
		foreach (var b in neighbors)
		{
			Vector2 diff = GlobalPosition - b.GlobalPosition;
			float d = diff.Length();

			if (d < SeparationRadius)
			{
				if (d < 1f)
					diff = new Vector2((float)GD.RandRange(-1.0, 1.0), (float)GD.RandRange(-1.0, 1.0));
				else
					diff /= d;
				steer += diff * (SeparationRadius - d) / SeparationRadius;
			}
		}
		return steer.LimitLength(MaxForce);
	}

	private Vector2 Alignment(List<Bubble> neighbors)
	{
		if (neighbors.Count == 0) return Vector2.Zero;

		var avg = Vector2.Zero;
		foreach (var b in neighbors) avg += b.Velocity;
		avg /= neighbors.Count;

		var desired = avg.Normalized() * MaxSpeed;
		return (desired - Velocity).LimitLength(MaxForce);
	}

	private Vector2 Cohesion(List<Bubble> neighbors)
	{
		if (neighbors.Count == 0) return Vector2.Zero;

		var center = Vector2.Zero;
		foreach (var b in neighbors) center += b.GlobalPosition;
		center /= neighbors.Count;

		return Seek(center);
	}

	private Vector2 Seek(Vector2 targetPos)
	{
		Vector2 to = targetPos - GlobalPosition;
		if (to.LengthSquared() < 0.01f) return Vector2.Zero;
		var desired = (targetPos - GlobalPosition).Normalized() * MaxSpeed;
		return (desired - Velocity).LimitLength(MaxForce);
	}

	private void OnBodyEntered(Node body)
	{
		if (body is IHittable hittable)
		{
			hittable.TakeDamage(Damage);
			QueueFree();
		}
	}

	private void OnAreaEntered(Area2D area)
	{
		if (area.IsInGroup("attack"))
		{
			QueueFree();
		}
	}
}

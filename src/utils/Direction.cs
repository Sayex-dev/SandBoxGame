using Godot;
using System;
using System.Collections.Generic;

public enum Direction
{
	RIGHT,
	LEFT,
	UP,
	DOWN,
	BACKWARD,
	FORWARD
}

static class DirectionMethods
{
	public static Vector3 GetVecFromForward(Vector3 forwardVec, Direction direction)
	{
		Vector3 forward = forwardVec.Normalized();
		Vector3 worldUp = Vector3.Up;

		// Handle edge case where forward is parallel to up
		if (Mathf.Abs(forward.Dot(worldUp)) > 0.999f)
			worldUp = Vector3.Right;

		Vector3 right = worldUp.Cross(forward).Normalized();
		Vector3 up = forward.Cross(right).Normalized();

		return direction switch
		{
			Direction.FORWARD => forward,
			Direction.BACKWARD => -forward,
			Direction.RIGHT => right,
			Direction.LEFT => -right,
			Direction.UP => up,
			Direction.DOWN => -up,
			_ => Vector3.Zero
		};
	}
}
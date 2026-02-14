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

static class DirectionTools
{
	public static Direction RotateLeft(Direction dir)
	{
		switch (dir)
		{
			case Direction.FORWARD:
				return Direction.LEFT;
			case Direction.LEFT:
				return Direction.BACKWARD;
			case Direction.BACKWARD:
				return Direction.RIGHT;
			case Direction.RIGHT:
				return Direction.FORWARD;
			default:
				return dir;
		}
	}

	public static Direction RotateRight(Direction dir)
	{
		switch (dir)
		{
			case Direction.FORWARD:
				return Direction.RIGHT;
			case Direction.RIGHT:
				return Direction.BACKWARD;
			case Direction.BACKWARD:
				return Direction.LEFT;
			case Direction.LEFT:
				return Direction.FORWARD;
			default:
				return dir;
		}
	}

	public static Direction Invert(Direction dir)
	{
		switch (dir)
		{
			case Direction.FORWARD:
				return Direction.BACKWARD;
			case Direction.RIGHT:
				return Direction.LEFT;
			case Direction.BACKWARD:
				return Direction.FORWARD;
			case Direction.LEFT:
				return Direction.RIGHT;
			case Direction.UP:
				return Direction.DOWN;
			case Direction.DOWN:
				return Direction.UP;
			default:
				return dir;
		}
	}

	public static Vector3 GetWorldDirVec(Direction direction)
	{
		return direction switch
		{
			Direction.FORWARD => Vector3.Forward,
			Direction.BACKWARD => Vector3.Back,
			Direction.RIGHT => Vector3.Right,
			Direction.LEFT => Vector3.Left,
			Direction.UP => Vector3.Up,
			Direction.DOWN => Vector3.Down,
			_ => Vector3.Zero
		};
	}

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
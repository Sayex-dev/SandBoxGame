using Godot;
using System;

public partial class CameraTemp : Node3D
{
	private float speed = 5f;
	private float sprintSpeed = 50f;
	private float rotationSpeed = 0.0001f;

	public override void _PhysicsProcess(double delta)
	{
		bool isSprinting = Input.IsActionPressed("sprint");
		Vector3 moveVec = Vector3.Zero;

		if (Input.IsActionPressed("move_right"))
			moveVec += Vector3.Right;

		if (Input.IsActionPressed("move_left"))
			moveVec += Vector3.Left;

		if (Input.IsActionPressed("move_forward"))
			moveVec += Vector3.Forward;

		if (Input.IsActionPressed("move_backward"))
			moveVec += Vector3.Back;

		if (Input.IsActionPressed("move_up"))
			moveVec += Vector3.Up;

		if (Input.IsActionPressed("move_down"))
			moveVec += Vector3.Down;

		// Camera rotation
		if (Input.IsActionPressed("move_camera"))
			Rotate(Vector3.Up, Input.GetLastMouseVelocity().X * -rotationSpeed);

		float moveSpeed = isSprinting ? sprintSpeed : speed;

		if (moveVec != Vector3.Zero)
			moveVec = moveVec.Normalized() * moveSpeed * (float)delta;

		// Apply movement relative to current rotation
		Position += moveVec.Rotated(Vector3.Up, Rotation.Y);
	}
}

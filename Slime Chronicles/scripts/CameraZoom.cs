using Godot;

public partial class CameraZoom: Camera2D
{
	[Export] public Vector2 DefaultZoom = new Vector2(1.0f, 1.0f);
	[Export] public Vector2 ZoomedOutScale = new Vector2(0.5f, 0.5f);
	[Export] public float ZoomSpeed = 5.0f;

	// The target Zoom Vector2 we are moving towards
	private Vector2 _targetZoom;

	public override void _Ready()
	{
		// Initialize the camera's zoom and target
		_targetZoom = DefaultZoom;
		Zoom = DefaultZoom;
	}

	public override void _Process(double delta)
	{
		// 1. Check for Key Input and Set Target Zoom
		if (Input.IsActionPressed("zoom_out"))
		{
			// Key is pressed, set target to the zoomed-out scale (a larger vector = further out)
			_targetZoom = ZoomedOutScale;
		}
		else
		{
			// Key is released, set target back to the default zoom scale
			_targetZoom = DefaultZoom;
		}

		// 2. Smoothly Move the Current Zoom to the Target Zoom
		// Vector2.Lerp (Linear Interpolation) handles the smooth transition between vectors
		Zoom = Zoom.Lerp(_targetZoom, (float)delta * ZoomSpeed);
	}
}

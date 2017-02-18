using UnityEngine;

public class CellInfo {

	public bool TopLeft;
	public bool Top;
	public bool TopRight;
	public bool Right;
	public bool BottomRight;
	public bool Bottom;
	public bool BottomLeft;
	public bool Left;

	private int x;
	private int y;
	private  Texture2D bitmapTexture;

	public CellInfo(Texture2D bitmapTexture, int x, int y) {
		this.bitmapTexture = bitmapTexture;
		this.x = x;
		this.y = y;

		TopLeft = false;
		Top = false;
		TopRight = false;
		Right = false;
		BottomRight = false;
		Bottom = false;
		BottomLeft = false;
		Left = false;

		FillInfo ();
	}

	private void FillInfo() {
		if (IsInBounds (this.x - 1, this.y + 1)) {
			TopLeft = bitmapTexture.GetPixel (x - 1, y + 1).r == 0;
		}
		if (IsInBounds (this.x, this.y + 1)) {
			Top = bitmapTexture.GetPixel (x, y + 1).r == 0;
		}
		if (IsInBounds (this.x + 1, this.y + 1)) {
			TopRight = bitmapTexture.GetPixel (x + 1, y + 1).r == 0;
		}
		if (IsInBounds (this.x + 1, this.y)) {
			Right = bitmapTexture.GetPixel (x + 1, y).r == 0;
		}
		if (IsInBounds (this.x + 1, this.y - 1)) {
			BottomRight = bitmapTexture.GetPixel (x + 1, y - 1).r == 0;
		}
		if (IsInBounds (this.x, this.y - 1)) {
			Bottom = bitmapTexture.GetPixel (x, y - 1).r == 0;
		}
		if (IsInBounds (this.x - 1, this.y - 1)) {
			BottomLeft = bitmapTexture.GetPixel (x - 1, y - 1).r == 0;
		}
		if (IsInBounds (this.x - 1, this.y)) {
			Left = bitmapTexture.GetPixel (x - 1, y).r == 0;
		}
	}

	private bool IsInBounds(int x, int y) {
		return x >= 0 && x < bitmapTexture.width && y >= 0 && y < bitmapTexture.height;
	}

}

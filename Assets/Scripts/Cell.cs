using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell {

	public int x;
	public int y;

	public short value;

	public Cell(int x, int y, short value) {
		this.x = x;
		this.y = y;

		this.value = value;
	}

	public int GetCellIndex() {
		return x * 512 + y;
	}

}

using UnityEngine;
using System.Collections;

public class Tile
{
	public int x;           //The x position of the tile.
	public int y;           //The y position of the tile.
	public TileType type;   //The type of tile that it is.
	public GameObject tile; //The gameobject relevant to this tile.
	public Tile()
	{
		type = TileType.Empty;
	}
	public Tile(int x, int y, TileType type)
	{
		this.x = x;
		this.y = y;
		this.type = type;
	}

}



using UnityEngine;
using System.Collections;
using System.IO;

public class GameManager : MonoBehaviour
{
	//Level
	public int levelId;                                 //The current level.
	public int levels;                                  //The total amount of levels there are.
	public Tile[,] grid;                                //The 2D array holding all out tiles.
	public Vector2 playerSpawn;                         //The position where the player will spawn.
	public GravityDirection gravityDirection;           //The current direction of gravity.
	public ParticleSystem levelCompleteParticleEffect;  //The particle effect that will be played once the player completes a level.

	public Player player;                               //The player class.
	public CameraController cameraController;           //The camera controller class.
	public GameUI ui;                                   //The game ui class.

	void Start()
	{
		//When we load in, we set levelId to be the level that the player selected in the menu.
		levelId = PlayerPrefs.GetInt("SelectedLevel");

		//Then we load the level.
		LoadLevel();
		ui.manager = this;
	}

	//This function loads the level. The level it loads is based on what the levelId is.
	//It reads the level text file and builds the level based on that.
	public void LoadLevel()
	{
		// Khởi tạo grid
		grid = new Tile[20, 20];
		string level = Resources.Load<TextAsset>("Levels/Level" + levelId).ToString();
		string[] lines = level.Split("\n"[0]);
		ui.levelText.text = "LEVEL " + levelId;

		levels = Resources.LoadAll<TextAsset>("Levels").Length;

		// Thiết lập grid ban đầu
		for (int x = 0; x < 20; x++)
		{
			for (int y = 0; y < 20; y++)
			{
				Tile g = new Tile
				{
					x = x,
					y = y,
					type = TileType.Empty
				};
				grid[x, y] = g;
			}
		}

		// Tạo các tile từ dữ liệu level
		foreach (string l in lines)
		{
			if (!string.IsNullOrEmpty(l))
			{
				string[] s = l.Split(","[0]);
				Vector3 pos = new Vector3((int.Parse(s[0]) + 1) * 9, (int.Parse(s[1]) + 1) * 9, 0);
				GameObject t = Pool.GetTile(int.Parse(s[2]));

				GameObject tile = Instantiate(t, pos, Quaternion.identity);

				Tile g = new Tile
				{
					x = int.Parse(s[0]) + 1,
					y = int.Parse(s[1]) + 1,
					type = (TileType)int.Parse(s[2]) + 1,
					tile = tile
				};

				// Xử lý Player tile
				if (s[2].Contains("1"))
				{
					player = tile.GetComponent<Player>();
					player.manager = this;
					cameraController.playerTarget = player.gameObject;
					g.type = TileType.Empty;
					playerSpawn = pos;

					// Áp dụng skin đã chọn
					string selectedSkin = PlayerPrefs.GetString("SelectedItem", "default_skin");
					ApplySkinToPlayer(player, selectedSkin);
				}

				// Xử lý các tile khác
				if (s[2].Contains("2"))
				{
					tile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, int.Parse(s[3]) * -90));
				}

				if (s[2].Contains("3"))
				{
					levelCompleteParticleEffect.transform.position = tile.transform.position;
				}

				grid[int.Parse(s[0]) + 1, int.Parse(s[1]) + 1] = g;
			}
		}

		// Thiết lập camera và UI
		cameraController.SetTarget(this);
		ui.SetTutorial();
	}

	// Hàm áp dụng skin cho Player
	private void ApplySkinToPlayer(Player player, string skinID)
	{
		SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
		switch (skinID)
		{
			case "red_skin":
				sr.color = Color.red;
				break;
			case "green_skin":
				sr.color = Color.green;
				break;
			default:
				sr.color = Color.white; // Màu mặc định
				break;
		}
	}

	//Can the player move in the direction they want to?
	public bool PlayerCanMove(Vector3 playerPos, Vector3 dir)
	{
		int posX = (int)(playerPos.x / 9);
		int posY = (int)(playerPos.y / 9);
		int dirX = (int)dir.x;
		int dirY = (int)dir.y;

		Tile g = grid[(posX + dirX), (posY + dirY)];

		if (g.type == TileType.Block || g.type == TileType.Spike)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	//Returns the tile type in the direction sent from the player's position.
	public TileType GetTileType(Vector3 playerPos, Vector3 dir)
	{
		int posX = (int)(playerPos.x / 9);
		int posY = (int)(playerPos.y / 9);
		int dirX = (int)dir.x;
		int dirY = (int)dir.y;

		Tile g = grid[(posX + dirX), (posY + dirY)];

		return g.type;
	}

	//Returns the tile that the player is ontop of.
	public Tile GetFloor(Vector3 playerPos, Vector3 dir)
	{
		int posX = (int)(playerPos.x / 9);
		int posY = (int)(playerPos.y / 9);
		int dirX = (int)dir.x;
		int dirY = (int)dir.y;

		if (dirY == 0)
		{
			for (int x = 0; x < 20; x++)
			{
				if (grid[(posX + (dirX * x)), posY].type == TileType.Block)
				{
					return grid[(posX + (dirX * x) - dirX), posY];
				}
				if (grid[(posX + (dirX * x)), posY].type == TileType.Spike)
				{
					player.fallingInSpikes = true;
				}
			}
		}

		if (dirX == 0)
		{
			for (int y = 0; y < 20; y++)
			{
				if (grid[posX, (posY + (dirY * y))].type == TileType.Block)
				{
					return grid[posX, (posY + (dirY * y) - dirY)];
				}
				if (grid[posX, (posY + (dirY * y))].type == TileType.Spike)
				{
					player.fallingInSpikes = true;
				}
			}
		}

		return null;
	}

	//Returns true if there is a floor for the player to fall onto.
	public bool HasFloor(Vector3 playerPos, Vector3 dir)
	{
		int posX = (int)(playerPos.x / 9);
		int posY = (int)(playerPos.y / 9);
		int dirX = (int)dir.x;
		int dirY = (int)dir.y;

		if (dirY == 0)
		{
			for (int x = 0; x < 20; x++)
			{
				if (posX + (dirX * x) > 0 && posX + (dirX * x) < 20)
				{
					if (grid[(posX + (dirX * x)), posY].type == TileType.Block)
					{
						return true;
					}
				}
			}
		}

		if (dirX == 0)
		{
			for (int y = 0; y < 20; y++)
			{
				if (posY + (dirY * y) > 0 && posY + (dirY * y) < 20)
				{
					if (grid[posX, (posY + (dirY * y))].type == TileType.Block)
					{
						return true;
					}
				}
			}
		}

		return false;
	}

	//Flips the gravity to the requested direction.
	public void FlipGravity(GravityDirection dir)
	{
		switch (dir)
		{
			case GravityDirection.North:
				{
					cameraController.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
					levelCompleteParticleEffect.transform.eulerAngles = new Vector3(-90, 0, 0);
					gravityDirection = GravityDirection.North;
					break;
				}
			case GravityDirection.South:
				{
					cameraController.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -180));
					levelCompleteParticleEffect.transform.eulerAngles = new Vector3(-90, 0, 180);
					gravityDirection = GravityDirection.South;
					break;
				}
			case GravityDirection.East:
				{
					cameraController.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
					levelCompleteParticleEffect.transform.eulerAngles = new Vector3(-90, 0, 90);
					gravityDirection = GravityDirection.East;
					break;
				}
			case GravityDirection.West:
				{
					cameraController.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -270));
					levelCompleteParticleEffect.transform.eulerAngles = new Vector3(-90, 0, 270);
					gravityDirection = GravityDirection.West;
					break;
				}
		}

		player.CheckGravity();
		ui.StartCoroutine("RotateCompass", dir);
	}

	//Called when player reaches EndFlag - complete the level
	public void CompleteLevel()
	{
		// Award gold for completing level
		int goldReward = 50;

		// Check if this is first time completion for bonus
		string levelKey = "Level" + levelId;
		if (PlayerPrefs.GetInt(levelKey + "_FirstTime", 1) == 1)
		{
			// First time completion - bonus gold
			goldReward += 25;
			PlayerPrefs.SetInt(levelKey + "_FirstTime", 0);
		}

		// Add gold to player's currency
		CurrencyManager.AddGold(goldReward);

		// Mark level as completed
		PlayerPrefs.SetInt(levelKey, 2);

		// Unlock next level if exists
		if (levelId + 1 < levels)
		{
			if (PlayerPrefs.GetInt("Level" + (levelId + 1)) == 0)
			{
				PlayerPrefs.SetInt("Level" + (levelId + 1), 1);
			}
		}

		// Update UI to show gold earned
		ui.ShowLevelComplete(goldReward);
	}
}

public enum GravityDirection {North, South, East, West}
public enum TileType {Empty, Block, Player, Spike, EndFlag}
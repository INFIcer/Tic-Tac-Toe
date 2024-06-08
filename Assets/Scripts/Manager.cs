using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class SpriteResources
{
	internal static void Init()
	{
		O = Resources.Load<Sprite>("O");
		X = Resources.Load<Sprite>("X");
	}
	public static Sprite GetSprite(bool? chess)
	{
		if (chess.HasValue)
			return chess.Value switch
			{
				false => X,
				true => O,
			};
		return null;
	}
	static Sprite O, X;
}
public class Cell
{
	public bool? chess
	{
		get => m_Chess;
		set
		{
			m_Chess = value;
			chessChanged?.Invoke(value);
		}
	}
	bool? m_Chess;
	public event Action<bool?> chessChanged;
}
public class Manager : MonoBehaviour
{
	#region UI
	public RectTransform boardUIParent;
	public CellView prefab;
	//Cell[] cells;
	public DeclareView declareView;
	public Toggle toggle;
	public Image indicator;
	void ShowDeclareView(bool? winner)
	{
		declareView.Bind(winner);
		declareView.Show();
	}
	void HideDeclareView()
	{
		declareView.Hide();
	}
	#endregion

	#region Game
	const int BOARD_SIZE = 3;
	Cell[,] board;
	int chessCount;
	public bool? currentPlayer
	{
		get => m_CurrentPlayer;
		private set
		{
			m_CurrentPlayer = value;
			OnCurrentPlayerChanged?.Invoke(value);
		}
	}
	bool? m_CurrentPlayer;
	public event Action<bool?> OnCurrentPlayerChanged;
	public void TriggerHumanPlace(int x, int y)
	{
		HumanPlace?.Invoke(x, y);
	}
	Action<int, int> HumanPlace;//人类玩家放置

	bool declared;//已决出胜负？
	bool humanPlayer = false;//人类玩家执子（X:false O:true）

	Coroutine coroutine = null;

	public void StartGame()
	{
		if (coroutine != null)
			StopCoroutine(coroutine);
		coroutine = StartCoroutine(GameLoop());
	}

	void InitGame()
	{
		for (int i = 0; i < BOARD_SIZE; i++)
			for (int j = 0; j < BOARD_SIZE; j++)
				board[i, j].chess = null;
		chessCount = 0;
		declared = false;
		humanPlayer = !toggle.isOn;
		HumanPlace = null;
		HideDeclareView();
	}

	void Place(int x, int y, bool? chess)
	{
		board[x, y].chess = chess;
		if (chess != null)
			chessCount++;
		if (chess == null)
			chessCount--;
	}

	public bool? CheckChess(int x, int y)
	{
		return board[x, y].chess;
	}

	//检查此次放置是否触发游戏结束
	(bool over, bool playerWin) IsGameOver(int x, int y, bool player)
	{
		if (CheckWin(x, y, player))
			return (true, true);
		if (CheckDraw())
			return (true, false);
		return (false, false);
	}

	bool CheckDraw()
	{
		return chessCount == 9;
	}

	bool CheckWin(int x, int y, bool player)
	{
		//行
		var res = true;
		for (int i = 0; i < BOARD_SIZE; i++)
			if (CheckChess(i, y) != player)
			{
				res = false;
				break;
			}
		if (res)
			return true;

		//列
		res = true;
		for (int i = 0; i < BOARD_SIZE; i++)
			if (CheckChess(x, i) != player)
			{
				res = false;
				break;
			}
		if (res)
			return true;

		//对角
		if (x == y)
		{
			res = true;
			for (int i = 0; i < BOARD_SIZE; i++)
				if (CheckChess(i, i) != player)
				{
					res = false;
					break;
				}
			if (res)
				return true;
		}

		if (x + y == 2)
		{
			res = true;
			for (int i = 0; i < BOARD_SIZE; i++)
				if (CheckChess(i, 2 - i) != player)
				{
					res = false;
					break;
				}
			if (res)
				return true;
		}

		return false;
	}

	void ActualPlace(int x, int y, bool player)
	{
		Place(x, y, player);
		var res = IsGameOver(x, y, player);
		if (res.over)
		{
			Declare(res.playerWin ? player : null);
		}
	}

	void Declare(bool? player)
	{
		declared = true;
		ShowDeclareView(player);
		if (player.HasValue)
		{
			Debug.Log($"{player} win");
		}
		else
			Debug.Log($"draw");
	}
	#region Turn
	IEnumerator HumanTurn()
	{
		HumanPlace += OnPlayerPlace;
		yield return new WaitUntil(() => HumanPlace == null);
	}
	void OnPlayerPlace(int x, int y)
	{
		if (CheckChess(x, y) == null)
		{
			ActualPlace(x, y, humanPlayer);
			HumanPlace -= OnPlayerPlace;
		}
	}
	IEnumerator AITurn()
	{
		yield return wait1s;

		if (chessCount == 0)//AI先手
		{
			(int x, int y) = UnityEngine.Random.Range(0, 4) switch
			{
				0 => (0, 0),
				1 => (2, 2),
				2 => (0, 2),
				3 => (2, 0),
				_ => throw new NotSupportedException(),
			};
			ActualPlace(x, y, !humanPlayer);
		}
		else
		{
			var score = Minimax(0, !humanPlayer, !humanPlayer, out var bestPos);
			//Debug.Log($"score:{score},{bestMove}");
			ActualPlace(bestPos.x, bestPos.y, !humanPlayer);
		}
		yield break;
	}
	#endregion
	IEnumerator GameLoop()
	{
		InitGame();
		currentPlayer = false;//X先手
		while (true)
		{
			if (currentPlayer == humanPlayer)
			{
				Debug.Log("PlayerTurn");
				yield return HumanTurn();
			}
			else
			{
				Debug.Log("AITurn");
				yield return AITurn();
			}
			if (declared)
			{
				currentPlayer = null;
				break;
			}
			else
				currentPlayer = !currentPlayer;
		}
		Debug.Log("game over");
	}
	#endregion

	#region AI
	//计算maximizingPlayer的最高分数
	//轮到自己的回合要扩大优势，轮到对手回合要减少劣势，所有评估基于maximizingPlayer的视角结算
	int Minimax(int depth, bool maximizingPlayer, bool player, out (int x, int y) bestPos)
	{
		var isMaximizingPlayer = maximizingPlayer == player;
		int bestScore = isMaximizingPlayer ? int.MinValue : int.MaxValue;
		bestPos = (-1, -1);
		for (int i = 0; i < BOARD_SIZE; i++)
			for (int j = 0; j < BOARD_SIZE; j++)
				if (CheckChess(i, j) == null)
				{
					//Debug.Log($"[{depth}]推演位置{(i, j)}");
					Place(i, j, player);
					int score;
					var res = IsGameOver(i, j, player);
					if (res.over)
						score = EvaluateScore(res.playerWin ? player : null, maximizingPlayer) - depth;//同等结果时选择步数少的
					else
						score = Minimax(depth + 1, maximizingPlayer, !player, out _);

					if (isMaximizingPlayer ^ (score < bestScore))
					{
						bestScore = score;
						bestPos = (i, j);
					}
					Place(i, j, null);
					//Debug.Log($"[{depth}]撤回位置{(i, j)} 得分{score} {bestScore}");
				}
		return bestScore;
	}
	//评估player的分数
	int EvaluateScore(bool? winner, bool player)
	{
		if (winner == null)
			return 0;
		return winner == player ? 1000 : -100;
	}
	#endregion

	WaitForSeconds wait1s = new WaitForSeconds(1);
	private void Start()
	{
		SpriteResources.Init();
		InitManager();
	}
	void InitManager()
	{
		board = new Cell[BOARD_SIZE, BOARD_SIZE];
		for (int i = 0; i < BOARD_SIZE; i++)
			for (int j = 0; j < BOARD_SIZE; j++)
				board[i, j] = new Cell();

		//cells = new Cell[BOARD_SIZE * BOARD_SIZE];
		for (int i = 0; i < BOARD_SIZE; i++)
			for (int j = 0; j < BOARD_SIZE; j++)
			{
				var uCell = Instantiate(prefab, boardUIParent);
				uCell.Bind(board[i, j]);
				uCell.x = i;
				uCell.y = j;
				uCell.HumanPlace = TriggerHumanPlace;
				//cells[i + j * BOARD_SIZE] = uCell;

				uCell.GetComponent<RectTransform>().anchoredPosition = new Vector2((i - 1) * 100, (j - 1) * 100);
			}

		OnCurrentPlayerChanged += (b) =>
		{
			indicator.sprite = SpriteResources.GetSprite(b);
			indicator.transform.parent.gameObject.SetActive(b.HasValue);
		};

		StartGame();
	}
}

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class CellView : MonoBehaviour, IPointerClickHandler
{
	[SerializeField] Image image;

	[NonSerialized] public Action<int, int> HumanPlace;
	[NonSerialized] public int x, y;
	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		HumanPlace(x, y);
	}
	public void Bind(Cell cell)
	{
		OnChessChanged(cell.chess);
		cell.chessChanged += OnChessChanged;
	}
	void OnChessChanged(bool? chess)
	{
		Sprite res = null;
		if (chess.HasValue)
		{
			image.color = Color.white;
			res = SpriteResources.GetSprite(chess);
		}
		else
		{
			image.color = Color.clear;
		}
		image.sprite = res;
	}
}

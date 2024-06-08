using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeclareView : MonoBehaviour
{
	public Image image;
	public TextMeshProUGUI text;
	public CanvasGroup canvasGroup;
	public void Show()
	{
		canvasGroup.alpha = 1.0f;
	}
	public void Hide()
	{
		canvasGroup.alpha = 0;
	}
	public void Bind(bool? winner)
	{
		if (winner.HasValue)
		{
			image.gameObject.SetActive(true);
			image.sprite = SpriteResources.GetSprite(winner);
			text.text = "获胜";
		}
		else
		{
			image.gameObject.SetActive(false);
			text.text = "平局";
		}
		text.Rebuild(CanvasUpdate.PostLayout);
	}
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class View : MonoBehaviour
{
	CanvasGroup canvasGroup;
	private void Start()
	{
		if (!TryGetComponent(out canvasGroup))
		{
			canvasGroup = gameObject.AddComponent<CanvasGroup>();
		}
	}
	public void Show()
	{
		canvasGroup.alpha = 1.0f;
	}
	public void Hide()
	{
		canvasGroup.alpha = 0;
	}
}
public class DeclareView : View
{
	[SerializeField] Image image;
	[SerializeField] TextMeshProUGUI text;

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

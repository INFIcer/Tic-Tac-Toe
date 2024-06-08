using UnityEngine;
using UnityEngine.UI;

public class IndicatorView : View
{
	[SerializeField] Image image;
	public void Bind(bool? player)
	{
		image.sprite = SpriteResources.GetSprite(player);
		if (player.HasValue)
			Show();
		else
			Hide();
	}
}

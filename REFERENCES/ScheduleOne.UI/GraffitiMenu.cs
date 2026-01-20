using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Graffiti;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class GraffitiMenu : Singleton<GraffitiMenu>
{
	[Header("References")]
	public Canvas Canvas;

	public RectTransform ColorButtonContainer;

	public Button ClearButton;

	public Transform ConfirmPanel;

	public Button ConfirmButton;

	public Button CancelButton;

	public RectTransform RemainigPaintContainer;

	public Slider RemainingPaintSlider;

	public Image[] RemainingPaintImages;

	public TextMeshProUGUI RemainingPaintLabel;

	[Header("Prefabs")]
	public GameObject ColorButtonPrefab;

	public Action<ESprayColor> onColorSelected;

	public Action onClearClicked;

	public Action onConfirmClicked;

	private List<Button> colorButtons = new List<Button>();

	private SpraySurface activeSurface;

	protected override void Awake()
	{
		base.Awake();
		ClearButton.onClick.AddListener(ClearClicked);
		ConfirmButton.onClick.AddListener(ConfirmClicked);
		CancelButton.onClick.AddListener(CancelClicked);
		for (int i = 0; i < Enum.GetValues(typeof(ESprayColor)).Length; i++)
		{
			ESprayColor color = (ESprayColor)i;
			if (color != ESprayColor.None)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(ColorButtonPrefab, ColorButtonContainer);
				gameObject.GetComponent<Image>().color = color.GetColor();
				gameObject.GetComponent<Button>().onClick.AddListener(delegate
				{
					SelectColor(color);
				});
				colorButtons.Add(gameObject.GetComponent<Button>());
			}
		}
		RemainigPaintContainer.SetAsLastSibling();
	}

	public void Open()
	{
		SelectColor(ESprayColor.Black);
		ConfirmPanel.gameObject.SetActive(value: false);
		Canvas.enabled = true;
	}

	public void Close()
	{
		Canvas.enabled = false;
	}

	public void ShowConfirmPanel()
	{
		ConfirmPanel.gameObject.SetActive(value: true);
	}

	private void SelectColor(ESprayColor color)
	{
		if (onColorSelected != null)
		{
			onColorSelected(color);
		}
		for (int i = 0; i < colorButtons.Count; i++)
		{
			if (i + 1 == (int)color)
			{
				colorButtons[i].interactable = false;
				colorButtons[i].transform.Find("Selected").gameObject.SetActive(value: true);
			}
			else
			{
				colorButtons[i].interactable = true;
				colorButtons[i].transform.Find("Selected").gameObject.SetActive(value: false);
			}
		}
	}

	public void UpdateRemainingPaintIndicator(float remainingPaint)
	{
		RemainingPaintSlider.value = remainingPaint;
		RemainingPaintLabel.text = Mathf.RoundToInt(remainingPaint * 100f) + "%";
		Color color = new Color32(byte.MaxValue, 80, 80, byte.MaxValue);
		RemainingPaintLabel.color = ((remainingPaint > 0.001f) ? Color.white : color);
		Image[] remainingPaintImages = RemainingPaintImages;
		for (int i = 0; i < remainingPaintImages.Length; i++)
		{
			remainingPaintImages[i].color = ((remainingPaint > 0.001f) ? Color.white : color);
		}
	}

	private void ClearClicked()
	{
		if (onClearClicked != null)
		{
			onClearClicked();
		}
	}

	private void ConfirmClicked()
	{
		if (onConfirmClicked != null)
		{
			onConfirmClicked();
		}
	}

	private void CancelClicked()
	{
		ConfirmPanel.gameObject.SetActive(value: false);
	}
}

using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Messaging;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Messages;

public class ConfirmationPopup : MonoBehaviour
{
	public enum EResponse
	{
		Confirm,
		Cancel
	}

	[Header("References")]
	public GameObject Container;

	public Text TitleLabel;

	public Text MessageLabel;

	public Button ConfirmButton;

	public Button CancelButton;

	private MSGConversation conversation;

	private Action<EResponse> responseCallback;

	public bool IsOpen { get; private set; }

	private void Start()
	{
		GameInput.RegisterExitListener(Exit, 4);
		Close(EResponse.Cancel);
		ConfirmButton.onClick.AddListener(Confirm);
		CancelButton.onClick.AddListener(Cancel);
	}

	public void Exit(ExitAction action)
	{
		if (!action.Used && IsOpen)
		{
			action.Used = true;
			Cancel();
		}
	}

	public void Open(string title, string message, MSGConversation conv, Action<EResponse> callback)
	{
		IsOpen = true;
		conversation = conv;
		MSGConversation mSGConversation = conversation;
		mSGConversation.onMessageRendered = (Action)Delegate.Combine(mSGConversation.onMessageRendered, new Action(Cancel));
		responseCallback = callback;
		TitleLabel.text = title;
		MessageLabel.text = message;
		Container.gameObject.SetActive(value: true);
	}

	public void Close(EResponse outcome)
	{
		IsOpen = false;
		if (conversation != null)
		{
			MSGConversation mSGConversation = conversation;
			mSGConversation.onMessageRendered = (Action)Delegate.Remove(mSGConversation.onMessageRendered, new Action(Cancel));
		}
		if (responseCallback != null)
		{
			responseCallback(outcome);
			responseCallback = null;
		}
		Container.gameObject.SetActive(value: false);
	}

	private void Confirm()
	{
		Close(EResponse.Confirm);
	}

	private void Cancel()
	{
		Close(EResponse.Cancel);
	}
}

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Norsemen;

public class NorseGui : MonoBehaviour
{
	public class ButtonElement
	{
		public readonly GameObject go;

		public RectTransform rect;

		public readonly Button button;

		public ButtonSfx buttonSfx;

		public readonly UIGamePad uiGamePad;

		public Image glow;

		public readonly TMP_Text text;

		public UIInputHint inputHint;

		public readonly TMP_Text inputText;

		public ButtonElement(GameObject source, string name)
		{
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			go = source;
			((Object)go).name = name;
			rect = source.GetComponent<RectTransform>();
			button = source.GetComponent<Button>();
			buttonSfx = source.GetComponent<ButtonSfx>();
			uiGamePad = source.GetComponent<UIGamePad>();
			text = ((Component)source.transform.Find("Text")).GetComponent<TMP_Text>();
			inputHint = ((Component)source.transform.Find("gamepad_hint (1)")).GetComponent<UIInputHint>();
			inputText = ((Component)((Component)inputHint).transform.Find("Text")).GetComponent<TMP_Text>();
			glow = new GameObject("glow").AddComponent<Image>();
			((Transform)((Graphic)glow).rectTransform).SetParent((Transform)(object)rect);
			((Graphic)glow).rectTransform.sizeDelta = rect.sizeDelta;
			((Graphic)glow).rectTransform.anchorMax = Vector2.zero;
			((Graphic)glow).rectTransform.anchorMin = Vector2.zero;
			((Graphic)glow).rectTransform.pivot = Vector2.zero;
			((Graphic)glow).rectTransform.anchoredPosition = Vector2.zero;
			SetGlow(enabled: false);
		}

		public void SetGlow(bool enabled)
		{
			((Behaviour)glow).enabled = enabled;
		}

		public void SetupGlow(Image source)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			glow.sprite = source.sprite;
			((Graphic)glow).material = ((Graphic)source).material;
			((Graphic)glow).color = ((Graphic)source).color;
		}

		public void AddListener(UnityAction action)
		{
			((UnityEvent)button.onClick).AddListener(action);
		}

		public void SetLabel(string label)
		{
			text.text = Localization.instance.Localize(label);
		}

		public void SetGamePadKey(string key)
		{
			uiGamePad.m_zinputKey = key;
			inputText.text = Localization.instance.Localize(ZInput.instance.GetBoundKeyString(key, true));
		}
	}

	public static NorseGui instance;

	public ButtonElement behaviour = null;

	public ButtonElement patrol = null;

	public ButtonElement access = null;

	public void Awake()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Expected O, but got Unknown
		instance = this;
		InventoryGui componentInParent = ((Component)this).GetComponentInParent<InventoryGui>();
		Button stackAllButton = componentInParent.m_stackAllButton;
		HorizontalLayoutGroup val = ((Component)this).gameObject.AddComponent<HorizontalLayoutGroup>();
		((LayoutGroup)val).childAlignment = (TextAnchor)0;
		((HorizontalOrVerticalLayoutGroup)val).childForceExpandHeight = false;
		((HorizontalOrVerticalLayoutGroup)val).childForceExpandWidth = false;
		((HorizontalOrVerticalLayoutGroup)val).childControlHeight = false;
		((HorizontalOrVerticalLayoutGroup)val).childControlWidth = false;
		((HorizontalOrVerticalLayoutGroup)val).spacing = 2f;
		((LayoutGroup)val).padding.top = 10;
		behaviour = new ButtonElement(Object.Instantiate<GameObject>(((Component)stackAllButton).gameObject, ((Component)val).transform), "Norseman_Behaviour");
		behaviour.AddListener(new UnityAction(OnBehaviourChange));
		behaviour.SetLabel("$norseman_aggressive");
		behaviour.SetGamePadKey("JoyLTrigger");
		patrol = new ButtonElement(Object.Instantiate<GameObject>(((Component)stackAllButton).gameObject, ((Component)val).transform), "Norseman_Patrol");
		patrol.AddListener(new UnityAction(OnPatrolChange));
		patrol.SetLabel("$norseman_patrol");
		patrol.SetGamePadKey("JoyRTrigger");
		access = new ButtonElement(Object.Instantiate<GameObject>(((Component)stackAllButton).gameObject, ((Component)val).transform), "Norseman_Access");
		access.AddListener(new UnityAction(OnAccessChange));
		access.SetLabel("$norseman_public");
		access.SetGamePadKey("");
	}

	public void OnDestroy()
	{
		instance = null;
	}

	public void Show()
	{
		if ((Object)(object)VikingGui.m_currentViking == (Object)null)
		{
			return;
		}
		((Component)this).gameObject.SetActive(true);
		switch (VikingGui.m_currentViking.m_vikingAI.m_behaviour)
		{
		case Emotion.Passive:
			behaviour.SetLabel("$norseman_passive");
			break;
		case Emotion.Aggressive:
			behaviour.SetLabel("$norseman_aggressive");
			break;
		}
		switch (VikingGui.m_currentViking.m_vikingAI.m_moveType)
		{
		case Movement.Patrol:
			patrol.SetLabel("$norseman_patrol");
			break;
		case Movement.Guard:
			patrol.SetLabel("$norseman_guard");
			break;
		}
        long num = ZDOMan.instance
			.GetZDO(((Character)VikingGui.m_currentViking).GetZDOID())
			.GetLong(ZDOVars.s_owner, 0L);
        if (num == Player.m_localPlayer.GetPlayerID())
		{
			access.go.SetActive(true);
			if (VikingGui.m_currentViking.IsPrivate())
			{
				access.SetLabel("$norseman_private");
			}
			else
			{
				access.SetLabel("$norseman_public");
			}
		}
		else
		{
			access.go.SetActive(false);
		}
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void OnBehaviourChange()
	{
        if (VikingGui.m_currentViking != null && Player.m_localPlayer != null)
        {
			Emotion emotion = VikingGui.m_currentViking.m_vikingAI.m_behaviour;
			Emotion emotion2 = emotion;
			Emotion emotion3 = emotion2;
			string text;
			if (emotion3 != Emotion.Passive)
			{
				VikingGui.m_currentViking.m_vikingAI.SetEmotion(1);
				behaviour.SetLabel("$norseman_passive");
				string format = Localization.instance.Localize("$norseman_behaviour_msg");
				text = string.Format(format, VikingGui.m_currentViking.GetText(), "$norseman_passive");
				((Character)Player.m_localPlayer).Message((MessageHud.MessageType)2, text, 0, (Sprite)null);
			}
			else
			{
				VikingGui.m_currentViking.m_vikingAI.SetEmotion(0);
				behaviour.SetLabel("$norseman_aggressive");
				string format = Localization.instance.Localize("$norseman_behaviour_msg");
				text = string.Format(format, VikingGui.m_currentViking.GetText(), "$norseman_aggressive");
			}
			((Character)Player.m_localPlayer).Message((MessageHud.MessageType)2, text, 0, (Sprite)null);
		}
	}

	public void OnAccessChange()
	{
        if (VikingGui.m_currentViking != null && Player.m_localPlayer != null)
        {
			bool flag = VikingGui.m_currentViking.IsPrivate();
			string format = Localization.instance.Localize("$norseman_behaviour_msg");
			string text;
			if (flag)
			{
				VikingGui.m_currentViking.SetPrivate(isPrivate: false);
				access.SetLabel("$norseman_public");
				text = string.Format(format, VikingGui.m_currentViking.GetText(), "$norseman_public");
			}
			else
			{
				VikingGui.m_currentViking.SetPrivate(isPrivate: true);
				access.SetLabel("$norseman_private");
				text = string.Format(format, VikingGui.m_currentViking.GetText(), "$norseman_private");
			}
			((Character)Player.m_localPlayer).Message((MessageHud.MessageType)2, text, 0, (Sprite)null);
		}
	}

	public void OnPatrolChange()
	{
        if (VikingGui.m_currentViking != null && Player.m_localPlayer != null)
        {
			string text;
			if (VikingGui.m_currentViking.m_vikingAI.m_moveType != Movement.Patrol)
			{
				VikingGui.m_currentViking.m_vikingAI.SetMovement(0);
				patrol.SetLabel("$norseman_patrol");
				string format = Localization.instance.Localize("$norseman_behaviour_msg");
				text = string.Format(format, VikingGui.m_currentViking.GetText(), "$norseman_patrol");
			}
			else
			{
				VikingGui.m_currentViking.m_vikingAI.SetMovement(1);
				patrol.SetLabel("$norseman_guard");
				string format = Localization.instance.Localize("$norseman_behaviour_msg");
				text = string.Format(format, VikingGui.m_currentViking.GetText(), "$norseman_guard");
			}
			((Character)Player.m_localPlayer).Message((MessageHud.MessageType)2, text, 0, (Sprite)null);
		}
	}
}

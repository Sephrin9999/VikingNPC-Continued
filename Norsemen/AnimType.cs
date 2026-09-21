using System;

namespace Norsemen;

public class AnimType : Attribute
{
	public string trigger = string.Empty;

	public bool isBool;

	public bool isIndex;

	public bool isEmote;

	public bool isChain;

	public bool isSequential;

	public int chainMax;

	public float chainInterval = 1f;

	public float chargeTime = 2f;

	public int index;

	public PlayerAnims nextSequence = PlayerAnims.None;
}

namespace Norsemen;

public class AnimChain
{
	public readonly int index;

	public readonly string trigger;

	public AnimChain(string trigger, int index)
	{
		this.trigger = trigger;
		this.index = index;
	}
}

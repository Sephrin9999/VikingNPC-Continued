using BepInEx.Configuration;

namespace Norsemen;

public class Faction
{
	public readonly string name;

	public int hash;

	private readonly bool friendly;

	public readonly string hoverColor;

	public readonly bool targetTames;

    public readonly Character.Faction faction;

    public readonly ConfigEntry<Toggle> isFriendly;

	public Faction(string name, bool friendly)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		hoverColor = "white";
		targetTames = false;
		this.name = name;
		this.friendly = friendly;
		hash = StringExtensionMethods.GetStableHashCode(name);
		faction = FactionManager.GetFaction(name);
		isFriendly = ConfigManager.config(name + " Faction", "Friendly", friendly ? Toggle.On : Toggle.Off, "If on, " + name + " are friendly unless aggravated");
		FactionManager.customFactions[faction] = this;
	}

	public bool IsFriendly()
	{
		return isFriendly.Value == Toggle.On;
	}
}

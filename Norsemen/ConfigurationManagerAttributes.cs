using System;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using JetBrains.Annotations;

namespace Norsemen;

public class ConfigurationManagerAttributes
{
	[UsedImplicitly]
	public int? Order;

	[UsedImplicitly]
	public bool? Browsable;

	[UsedImplicitly]
	[_003C812f407f_002D18f8_002D4794_002Dbefe_002D7b53d43aa20f_003ENullable(2)]
	public string Category;

	[UsedImplicitly]
	[_003C812f407f_002D18f8_002D4794_002Dbefe_002D7b53d43aa20f_003ENullable(new byte[] { 2, 1 })]
	public Action<ConfigEntryBase> CustomDrawer;
}

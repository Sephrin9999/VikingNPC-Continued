using System;
using System.Collections.Generic;
using UnityEngine;

namespace Norsemen;

public class NorseCommand
{
	public readonly string m_description;

	private readonly bool m_isSecret;

	private readonly bool m_adminOnly;

	private readonly Func<Terminal.ConsoleEventArgs, bool> m_command;

	private readonly Func<List<string>> m_optionFetcher;

    public bool Run(Terminal.ConsoleEventArgs args)
    {
		return !IsAdmin() || m_command(args);
	}

	private bool IsAdmin()
	{
        if (ZNet.instance == null)
        {
			return true;
		}
        if (!m_adminOnly || ZNet.instance.LocalPlayerIsAdminOrHost())
        {
			return true;
		}
		NorsemenPlugin.LogWarning("Admin only");
		return false;
	}

	public bool IsSecret()
	{
		return m_isSecret;
	}

	public List<string> FetchOptions()
	{
		return (m_optionFetcher == null) ? new List<string>() : m_optionFetcher();
	}

	public bool HasOptions()
	{
		return m_optionFetcher != null;
	}

	public NorseCommand(string input, string description, Func<Terminal.ConsoleEventArgs, bool> command, Func<List<string>> optionsFetcher = null, bool isSecret = false, bool adminOnly = false)
	{
		m_description = description;
		m_command = command;
		m_isSecret = isSecret;
		CommandManager.commands[input] = this;
		m_optionFetcher = optionsFetcher;
		m_adminOnly = adminOnly;
	}
}

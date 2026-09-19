using Microsoft.Maui.Storage;

namespace MindSilence.Tests;

/// <summary>
/// Isolated <see cref="IPreferences"/> for unit tests. Does not touch device storage.
/// </summary>
internal sealed class MemoryPreferences : IPreferences
{
	private readonly Dictionary<(string? SharedName, string Key), object?> _values = [];

	public bool ContainsKey(string key, string? sharedName = null) =>
		_values.ContainsKey((sharedName, key));

	public void Remove(string key, string? sharedName = null) =>
		_values.Remove((sharedName, key));

	public void Clear(string? sharedName = null)
	{
		if (sharedName is null)
		{
			_values.Clear();
			return;
		}

		foreach (var key in _values.Keys.Where(entry => entry.SharedName == sharedName).ToList())
			_values.Remove(key);
	}

	public T Get<T>(string key, T defaultValue, string? sharedName = null)
	{
		if (_values.TryGetValue((sharedName, key), out var value) && value is T typed)
			return typed;

		return defaultValue;
	}

	public void Set<T>(string key, T value, string? sharedName = null) =>
		_values[(sharedName, key)] = value;
}

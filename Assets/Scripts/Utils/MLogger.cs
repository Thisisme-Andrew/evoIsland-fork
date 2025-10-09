using UnityEngine;

public class MLogger
{
    private bool isLoggingEnabled = true; // Flag to enable or disable logging
    private readonly string tag;

    private MLogger(string tag)
    {
        this.tag = tag;
    }

    public static MLogger GetLogger(string tag)
    {
        return new MLogger(tag);
    }

    public void Enable(bool enable)
    {
        isLoggingEnabled = enable;
    }

    public void Info(string message)
    {
        if (!isLoggingEnabled) return;
        Debug.Log($"[{tag}] {message}");
    }

    public void Warn(string message)
    {
        if (!isLoggingEnabled) return;
        Debug.LogWarning($"[{tag}] {message}");
    }

    public void Error(string message)
    {
        if (!isLoggingEnabled) return;
        Debug.LogError($"[{tag}] {message}");
    }
}
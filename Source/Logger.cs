﻿using BepInEx.Logging;

namespace RepoXR;

public static class Logger
{
    internal static ManualLogSource? source;

    public static void LogInfo(object message)
    {
        source?.LogInfo(message);
    }

    public static void LogWarning(object message)
    {
        source?.LogWarning(message);
    }

    public static void LogError(object message)
    {
        source?.LogError(message);
    }

    public static void LogDebug(object message)
    {
        source?.LogDebug(message);
    }
}
using Godot;
using System;
using System.Collections.Generic;
using System.Threading;

public partial class TimeTracker : Node
{
    public enum TrackingType
    {
        Increment,  // Accumulates total time
        Average     // Calculates average time
    }

    private class TrackerData
    {
        public TrackingType Type;
        public double TotalTime;
        public int Count;
        public double StartTime;

        public double GetResult()
        {
            return Type == TrackingType.Average ? (Count > 0 ? TotalTime / Count : 0) : TotalTime;
        }
    }

    private readonly Dictionary<string, TrackerData> _trackers = new();
    private readonly ReaderWriterLockSlim _lock = new();

    public static TimeTracker Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        PrintSummary();
    }

    public static void Start(string name, TrackingType type = TrackingType.Increment)
    {
        Instance?.StartTracking(name, type);
    }

    public static double End(string name)
    {
        return Instance?.EndTracking(name) ?? 0;
    }

    public static double Get(string name)
    {
        return Instance?.GetTime(name) ?? 0;
    }

    public static void Reset(string name)
    {
        Instance?.ResetTracker(name);
    }

    private void StartTracking(string name, TrackingType type)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!_trackers.ContainsKey(name))
            {
                _trackers[name] = new TrackerData { Type = type };
            }

            _trackers[name].StartTime = Time.GetUnixTimeFromSystem() * 1000;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private double EndTracking(string name)
    {
        _lock.EnterWriteLock();
        try
        {
            if (_trackers.TryGetValue(name, out var tracker))
            {
                var elapsed = Time.GetUnixTimeFromSystem() * 1000 - tracker.StartTime;
                tracker.TotalTime += elapsed;
                tracker.Count++;
                return tracker.GetResult();
            }
            return 0;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private double GetTime(string name)
    {
        _lock.EnterReadLock();
        try
        {
            return _trackers.TryGetValue(name, out var tracker) ? tracker.GetResult() : 0;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    private void ResetTracker(string name)
    {
        _lock.EnterWriteLock();
        try
        {
            if (_trackers.TryGetValue(name, out var tracker))
            {
                tracker.TotalTime = 0;
                tracker.Count = 0;
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _lock?.Dispose();
        }
        base.Dispose(disposing);
    }

    public static void PrintSummary()
    {
        Instance?.PrintTrackerSummary();
    }

    private void PrintTrackerSummary()
    {
        _lock.EnterReadLock();
        try
        {
            if (_trackers.Count == 0)
            {
                GD.Print("TimeTracker: No trackers found");
                return;
            }

            GD.Print("=== TimeTracker Summary ===");
            foreach (var kvp in _trackers)
            {
                var name = kvp.Key;
                var data = kvp.Value;
                var typeStr = data.Type == TrackingType.Average ? "AVG" : "SUM";
                var value = data.GetResult();
                var count = data.Count;

                GD.Print($"{name}: {value:F3}ms ({typeStr}, {count} calls)");
            }
            GD.Print("===========================");
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}
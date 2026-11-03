using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MoonSweepersMod.Features
{
    /// <summary>
    /// A unit of work driven by a vehicle / cleaning robot. Progress can accrue both
    /// while the game is running and (simulated) while it is closed.
    /// </summary>
    public class WorkTask
    {
        public string Id;
        public string Vehicle = "";
        public string Kind = "clean";       // clean | mine | deliver | patrol
        public double Total = 100;
        public double Done;
        public bool Active = true;

        public double Remaining => Math.Max(0, Total - Done);
        public double Fraction => Total <= 0 ? 1 : Math.Min(1, Done / Total);
    }

    /// <summary>
    /// Offline progress ledger. The game itself pauses all work when closed, but the
    /// user asked that assigned jobs keep "working". We track real wall-clock time
    /// between sessions and advance active tasks accordingly, then report what was
    /// accomplished while away. The game's own .moon saves are never touched.
    /// </summary>
    public static class OfflineProgress
    {
        public static List<WorkTask> Tasks = new List<WorkTask>();
        public static string LedgerPath { get; set; }
        public static double PendingReportHours;
        public static double PendingReportWork;
        public static bool HasPendingReport => PendingReportWork > 0.01;

        static DateTime _lastActive = DateTime.UtcNow;
        static DateTime _nextSave = DateTime.UtcNow.AddSeconds(20);

        public static void Init(string path)
        {
            LedgerPath = path;
            Load();
            var now = DateTime.UtcNow;
            var elapsedHours = (_lastActive == default ? 0 : (now - _lastActive).TotalHours);
            if (elapsedHours > 0.02)
            {
                var rate = Core.ModConfig.OfflineWorkPerHour.Value;
                var work = elapsedHours * rate;
                var active = Tasks.FindAll(t => t.Active && t.Remaining > 0);
                if (active.Count > 0 && work > 0)
                {
                    double per = work / active.Count;
                    double applied = 0;
                    foreach (var t in active)
                    {
                        var add = Math.Min(t.Remaining, per);
                        t.Done += add; applied += add;
                        if (t.Remaining <= 0) t.Active = false;
                    }
                    PendingReportHours = elapsedHours;
                    PendingReportWork = applied;
                    Core.Log.Info($"[Offline] Away {elapsedHours:F2}h -> +{applied:F1} work across {active.Count} task(s).");
                }
                else
                {
                    PendingReportHours = elapsedHours;
                    PendingReportWork = 0;
                }
            }
            _lastActive = now;
            Save();
        }

        public static WorkTask AddTask(string kind, string vehicle, double total)
        {
            var t = new WorkTask { Id = Guid.NewGuid().ToString("N").Substring(0, 8), Kind = kind, Vehicle = vehicle, Total = total, Done = 0, Active = true };
            Tasks.Add(t);
            Save();
            return t;
        }

        public static void ProgressTask(string id, double amount)
        {
            var t = Tasks.Find(x => x.Id == id);
            if (t != null && t.Active) { t.Done = Math.Min(t.Total, t.Done + amount); if (t.Remaining <= 0) t.Active = false; }
        }

        public static void Save()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("lastActiveTicks=" + _lastActive.Ticks);
                sb.AppendLine("taskCount=" + Tasks.Count);
                foreach (var t in Tasks)
                    sb.AppendLine($"task|{t.Id}|{t.Vehicle}|{t.Kind}|{t.Total}|{t.Done}|{(t.Active ? 1 : 0)}");
                Directory.CreateDirectory(Path.GetDirectoryName(LedgerPath) ?? ".");
                File.WriteAllText(LedgerPath, sb.ToString());
            }
            catch (Exception e) { Core.Log.Error("[Offline] Save: " + e); }
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists(LedgerPath)) return;
                var lines = File.ReadAllLines(LedgerPath);
                Tasks.Clear();
                foreach (var line in lines)
                {
                    if (line.StartsWith("lastActiveTicks="))
                    {
                        long v; if (long.TryParse(line.Substring(16), out v)) _lastActive = new DateTime(v, DateTimeKind.Utc);
                    }
                    else if (line.StartsWith("task|"))
                    {
                        var p = line.Split('|');
                        if (p.Length >= 7)
                        {
                            var t = new WorkTask
                            {
                                Id = p[1], Vehicle = p[2], Kind = p[3],
                                Total = double.Parse(p[4], System.Globalization.CultureInfo.InvariantCulture),
                                Done = double.Parse(p[5], System.Globalization.CultureInfo.InvariantCulture),
                                Active = p[6] == "1"
                            };
                            Tasks.Add(t);
                        }
                    }
                }
                Core.Log.Info("[Offline] Loaded " + Tasks.Count + " task(s).");
            }
            catch (Exception e) { Core.Log.Error("[Offline] Load: " + e); }
        }

        /// <summary>Call periodically; advances active tasks a little and persists.</summary>
        public static void Tick(float deltaSeconds)
        {
            if (DateTime.UtcNow < _nextSave) return;
            _nextSave = DateTime.UtcNow.AddSeconds(20);
            _lastActive = DateTime.UtcNow;
            Save();
        }

        public static string ReportString()
        {
            if (!HasPendingReport) return "";
            var sb = new StringBuilder();
            sb.AppendLine($"你离开了约 {PendingReportHours:F1} 小时。");
            sb.AppendLine($"扫地机器人 / 车辆在此期间共完成 {PendingReportWork:F0} 个工作量。");
            int finished = Tasks.FindAll(t => !t.Active && t.Remaining <= 0).Count;
            if (finished > 0) sb.AppendLine($"其中 {finished} 个任务已完成。");
            sb.AppendLine("（重新打开游戏后，分配的工作不会丢失，会按离线时长结算。）");
            return sb.ToString();
        }

        public static void ClearReport() { PendingReportHours = 0; PendingReportWork = 0; }
    }
}

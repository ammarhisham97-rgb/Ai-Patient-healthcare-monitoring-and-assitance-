// ============================================================
//  REPORT MODELS
// ============================================================

using System.Collections.Generic;

namespace Ai_healthcare_monitoring_and_assistance_system.Models
{
    // ────────────────────────────────────────────────────────
    // Daily Report
    // ────────────────────────────────────────────────────────
    public class DailyReportData
    {
        public string DeviceId { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public int TotalReadings { get; set; }
        public int ReadingsWithAlerts { get; set; }
        public int ReadingsWithAnomalies { get; set; }

        // Heart Rate Stats
        public float AvgHeartRate { get; set; }
        public float MaxHeartRate { get; set; }
        public float MinHeartRate { get; set; }
        public int HighHeartRateCount { get; set; }

        // Temperature Stats
        public float AvgTemperature { get; set; }
        public float MaxTemperature { get; set; }
        public float MinTemperature { get; set; }
        public int FeverCount { get; set; }

        // Activity Stats
        public Dictionary<string, int> ActivityBreakdown { get; set; } = new();

        // Alerts
        public List<AlertSummary> AlertsSummary { get; set; } = new();

        // Anomalies
        public List<AnomalySummary> AnomaliesSummary { get; set; } = new();

        // Sensor Status
        public SensorHealthSummary SensorHealth { get; set; } = new();
    }

    // ────────────────────────────────────────────────────────
    // Weekly Report
    // ────────────────────────────────────────────────────────
    public class WeeklyReportData
    {
        public string DeviceId { get; set; } = string.Empty;
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public int TotalReadings { get; set; }

        // Daily breakdown
        public List<DailyStats> DailyBreakdown { get; set; } = new();

        // Week aggregates
        public float AvgHeartRate { get; set; }
        public float AvgTemperature { get; set; }
        public int TotalAlertsCount { get; set; }
        public int TotalAnomaliesCount { get; set; }

        // Most common issues
        public List<AlertSummary> TopAlerts { get; set; } = new();
        public List<AnomalySummary> TopAnomalies { get; set; } = new();

        // Activity trend
        public Dictionary<string, int> ActivityTrend { get; set; } = new();
    }

    // ────────────────────────────────────────────────────────
    // Monthly Report
    // ────────────────────────────────────────────────────────
    public class MonthlyReportData
    {
        public string DeviceId { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalReadings { get; set; }

        // Monthly aggregates
        public float AvgHeartRate { get; set; }
        public float AvgTemperature { get; set; }
        public int TotalAlertsCount { get; set; }
        public int TotalAnomaliesCount { get; set; }

        // Weekly breakdown
        public List<WeeklyStats> WeeklyBreakdown { get; set; } = new();

        // Health trend
        public HealthTrend HealthTrend { get; set; } = new();

        // Critical events
        public List<CriticalEvent> CriticalEvents { get; set; } = new();

        // Recommendations
        public List<string> HealthRecommendations { get; set; } = new();
    }

    // ────────────────────────────────────────────────────────
    // Alert History Report
    // ────────────────────────────────────────────────────────
    public class AlertHistoryReportData
    {
        public string DeviceId { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<AlertRecord> Alerts { get; set; } = new();
        public Dictionary<string, int> AlertFrequency { get; set; } = new();
        public int TotalAlerts { get; set; }
    }

    // ────────────────────────────────────────────────────────
    // Anomaly Report
    // ────────────────────────────────────────────────────────
    public class AnomalyReportData
    {
        public string DeviceId { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<AnomalyRecord> Anomalies { get; set; } = new();
        public Dictionary<string, int> AnomalyFrequency { get; set; } = new();
        public Dictionary<string, List<AnomalyPattern>> AnomalyPatterns { get; set; } = new();
        public int TotalAnomalies { get; set; }
        public int ReadingsWithAnomalies { get; set; }
        public float AnomalyPercentage { get; set; }
    }

    // ────────────────────────────────────────────────────────
    // Supporting Classes
    // ────────────────────────────────────────────────────────

    public class DailyStats
    {
        public DateTime Date { get; set; }
        public int ReadingCount { get; set; }
        public float AvgHeartRate { get; set; }
        public float AvgTemperature { get; set; }
        public int AlertCount { get; set; }
        public int AnomalyCount { get; set; }
    }

    public class WeeklyStats
    {
        public int WeekNumber { get; set; }
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public int ReadingCount { get; set; }
        public float AvgHeartRate { get; set; }
        public float AvgTemperature { get; set; }
        public int AlertCount { get; set; }
        public int AnomalyCount { get; set; }
    }

    public class AlertSummary
    {
        public string AlertType { get; set; } = string.Empty;
        public int Count { get; set; }
        public float Percentage { get; set; }
        public DateTime? FirstOccurrence { get; set; }
        public DateTime? LastOccurrence { get; set; }
    }

    public class AnomalySummary
    {
        public string AnomalyType { get; set; } = string.Empty;
        public int Count { get; set; }
        public float Percentage { get; set; }
        public string MetricType { get; set; } = string.Empty;
        public float AvgValue { get; set; }
    }

    public class AlertRecord
    {
        public string AlertType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }
        public float? MetricValue { get; set; }
    }

    public class AnomalyRecord
    {
        public string AnomalyType { get; set; } = string.Empty;
        public string MetricType { get; set; } = string.Empty;
        public float Value { get; set; }
        public DateTime Timestamp { get; set; }
        public float Severity { get; set; } // 0-1
    }

    public class AnomalyPattern
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public float AvgValue { get; set; }
    }

    public class SensorHealthSummary
    {
        public bool MpuHealthy { get; set; }
        public bool MaxHealthy { get; set; }
        public bool Ds18Healthy { get; set; }
        public bool DhtHealthy { get; set; }
        public float AvgWifiSignal { get; set; }
    }

    public class HealthTrend
    {
        public string TrendDescription { get; set; } = string.Empty;
        public float HealthScore { get; set; } // 0-100
        public bool IsImproving { get; set; }
        public List<string> ConcernAreas { get; set; } = new();
    }

    public class CriticalEvent
    {
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public float Severity { get; set; } // 0-1
    }
}

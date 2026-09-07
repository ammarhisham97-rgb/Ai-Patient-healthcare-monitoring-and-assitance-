// ============================================================
//  REPORT GENERATION SERVICE
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ai_healthcare_monitoring_and_assistance_system.Data;
using Ai_healthcare_monitoring_and_assistance_system.Models;

namespace Ai_healthcare_monitoring_and_assistance_system.Services
{
    public class ReportService
    {
        private readonly HealthMonitorDbContext _context;

        public ReportService(HealthMonitorDbContext context)
        {
            _context = context;
        }

        // ────────────────────────────────────────────────────────
        // DAILY REPORT
        // ────────────────────────────────────────────────────────
        public async Task<DailyReportData> GenerateDailyReportAsync(string deviceId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            var readings = await _context.PatientReadings
                .Where(r => r.DeviceId == deviceId && r.ReceivedAt >= startOfDay && r.ReceivedAt <= endOfDay)
                .OrderBy(r => r.ReceivedAt)
                .ToListAsync();

            var report = new DailyReportData
            {
                DeviceId = deviceId,
                ReportDate = date,
                TotalReadings = readings.Count
            };

            if (readings.Count == 0)
                return report;

            // Parse reading data and aggregate stats
            var allHeartRates = new List<float>();
            var allTemperatures = new List<float>();
            var allAlerts = new List<string>();
            var allAnomalies = new List<string>();
            var activityLevels = new Dictionary<string, int>();
            var sensorStatuses = new List<(bool mpu, bool max, bool ds18, bool dht)>();

            foreach (var reading in readings)
            {
                try
                {
                    var readingData = JsonSerializer.Deserialize<PatientReading>(reading.ReadingData ?? "{}");
                    if (readingData == null) continue;

                    // Heart Rate
                    if (readingData.HeartRate?.FingerDetected == true && readingData.HeartRate.Bpm > 0)
                    {
                        allHeartRates.Add(readingData.HeartRate.Bpm);
                        if (readingData.HeartRate.HighRate)
                            report.HighHeartRateCount++;
                    }

                    // Temperature
                    if (readingData.Temperature?.SensorOk == true)
                    {
                        allTemperatures.Add(readingData.Temperature.Celsius);
                        if (readingData.Temperature.Fever)
                            report.FeverCount++;
                    }

                    // Activity
                    if (!string.IsNullOrEmpty(reading.ActivityLevel))
                    {
                        if (activityLevels.ContainsKey(reading.ActivityLevel))
                            activityLevels[reading.ActivityLevel]++;
                        else
                            activityLevels[reading.ActivityLevel] = 1;
                    }

                    // Sensor Status
                    if (readingData.SensorStatus != null)
                    {
                        sensorStatuses.Add((
                            readingData.SensorStatus.MpuOk,
                            readingData.SensorStatus.MaxOk,
                            readingData.SensorStatus.Ds18Ok,
                            readingData.SensorStatus.DhtOk
                        ));
                    }

                    // Anomalies
                    if (!string.IsNullOrEmpty(reading.DetectedAnomalies))
                    {
                        var anomalies = JsonSerializer.Deserialize<List<string>>(reading.DetectedAnomalies);
                        if (anomalies != null)
                            allAnomalies.AddRange(anomalies);
                    }
                }
                catch { /* Skip malformed readings */ }
            }

            // Calculate averages
            if (allHeartRates.Count > 0)
            {
                report.AvgHeartRate = (float)allHeartRates.Average();
                report.MaxHeartRate = allHeartRates.Max();
                report.MinHeartRate = allHeartRates.Min();
            }

            if (allTemperatures.Count > 0)
            {
                report.AvgTemperature = (float)allTemperatures.Average();
                report.MaxTemperature = allTemperatures.Max();
                report.MinTemperature = allTemperatures.Min();
            }

            report.ActivityBreakdown = activityLevels;
            report.ReadingsWithAnomalies = readings.Count(r => !string.IsNullOrEmpty(r.DetectedAnomalies));

            // Sensor health
            if (sensorStatuses.Count > 0)
            {
                report.SensorHealth = new SensorHealthSummary
                {
                    MpuHealthy = sensorStatuses.Count(s => s.mpu) > (sensorStatuses.Count / 2),
                    MaxHealthy = sensorStatuses.Count(s => s.max) > (sensorStatuses.Count / 2),
                    Ds18Healthy = sensorStatuses.Count(s => s.ds18) > (sensorStatuses.Count / 2),
                    DhtHealthy = sensorStatuses.Count(s => s.dht) > (sensorStatuses.Count / 2)
                };
            }

            // Anomalies summary
            report.AnomaliesSummary = allAnomalies
                .GroupBy(a => a)
                .Select(g => new AnomalySummary
                {
                    AnomalyType = g.Key,
                    Count = g.Count(),
                    Percentage = (float)(g.Count() * 100.0 / readings.Count)
                })
                .OrderByDescending(a => a.Count)
                .ToList();

            return report;
        }

        // ────────────────────────────────────────────────────────
        // WEEKLY REPORT
        // ────────────────────────────────────────────────────────
        public async Task<WeeklyReportData> GenerateWeeklyReportAsync(string deviceId, DateTime weekStart)
        {
            var weekEnd = weekStart.AddDays(7).AddTicks(-1);

            var readings = await _context.PatientReadings
                .Where(r => r.DeviceId == deviceId && r.ReceivedAt >= weekStart && r.ReceivedAt <= weekEnd)
                .OrderBy(r => r.ReceivedAt)
                .ToListAsync();

            var report = new WeeklyReportData
            {
                DeviceId = deviceId,
                WeekStartDate = weekStart,
                WeekEndDate = weekEnd,
                TotalReadings = readings.Count
            };

            if (readings.Count == 0)
                return report;

            // Generate daily breakdowns
            for (int i = 0; i < 7; i++)
            {
                var dayDate = weekStart.AddDays(i);
                var dailyReadings = readings.Where(r => r.ReceivedAt.Date == dayDate.Date).ToList();

                var dayStats = new DailyStats
                {
                    Date = dayDate,
                    ReadingCount = dailyReadings.Count
                };

                if (dailyReadings.Count > 0)
                {
                    var heartRates = new List<float>();
                    var temperatures = new List<float>();

                    foreach (var reading in dailyReadings)
                    {
                        try
                        {
                            var readingData = JsonSerializer.Deserialize<PatientReading>(reading.ReadingData ?? "{}");
                            if (readingData?.HeartRate?.FingerDetected == true && readingData.HeartRate.Bpm > 0)
                                heartRates.Add(readingData.HeartRate.Bpm);

                            if (readingData?.Temperature?.SensorOk == true)
                                temperatures.Add(readingData.Temperature.Celsius);
                        }
                        catch { }
                    }

                    if (heartRates.Count > 0)
                        dayStats.AvgHeartRate = (float)heartRates.Average();
                    if (temperatures.Count > 0)
                        dayStats.AvgTemperature = (float)temperatures.Average();
                }

                report.DailyBreakdown.Add(dayStats);
            }

            // Calculate week aggregates
            var allHeartRates = new List<float>();
            var allTemperatures = new List<float>();
            var allAlerts = new List<string>();
            var allAnomalies = new List<string>();

            foreach (var reading in readings)
            {
                try
                {
                    var readingData = JsonSerializer.Deserialize<PatientReading>(reading.ReadingData ?? "{}");
                    if (readingData == null) continue;

                    if (readingData.HeartRate?.FingerDetected == true && readingData.HeartRate.Bpm > 0)
                        allHeartRates.Add(readingData.HeartRate.Bpm);

                    if (readingData.Temperature?.SensorOk == true)
                        allTemperatures.Add(readingData.Temperature.Celsius);

                    if (!string.IsNullOrEmpty(reading.DetectedAnomalies))
                    {
                        var anomalies = JsonSerializer.Deserialize<List<string>>(reading.DetectedAnomalies);
                        if (anomalies != null)
                            allAnomalies.AddRange(anomalies);
                    }
                }
                catch { }
            }

            if (allHeartRates.Count > 0)
                report.AvgHeartRate = (float)allHeartRates.Average();
            if (allTemperatures.Count > 0)
                report.AvgTemperature = (float)allTemperatures.Average();

            report.TotalAlertsCount = readings.Count(r => !string.IsNullOrEmpty(r.DetectedAnomalies));
            report.TotalAnomaliesCount = allAnomalies.Count;

            // Top anomalies
            report.TopAnomalies = allAnomalies
                .GroupBy(a => a)
                .Select(g => new AnomalySummary
                {
                    AnomalyType = g.Key,
                    Count = g.Count(),
                    Percentage = (float)(g.Count() * 100.0 / readings.Count)
                })
                .OrderByDescending(a => a.Count)
                .Take(5)
                .ToList();

            return report;
        }

        // ────────────────────────────────────────────────────────
        // MONTHLY REPORT
        // ────────────────────────────────────────────────────────
        public async Task<MonthlyReportData> GenerateMonthlyReportAsync(string deviceId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddTicks(-1);

            var readings = await _context.PatientReadings
                .Where(r => r.DeviceId == deviceId && r.ReceivedAt >= startDate && r.ReceivedAt <= endDate)
                .OrderBy(r => r.ReceivedAt)
                .ToListAsync();

            var report = new MonthlyReportData
            {
                DeviceId = deviceId,
                Year = year,
                Month = month,
                TotalReadings = readings.Count
            };

            if (readings.Count == 0)
                return report;

            // Weekly breakdown
            var weeksInMonth = (int)Math.Ceiling((double)(readings.Max(r => r.ReceivedAt.Day) / 7.0)) + 1;
            for (int week = 0; week < weeksInMonth; week++)
            {
                var weekStart = startDate.AddDays(week * 7);
                if (weekStart > endDate) break;

                var weekEnd = weekStart.AddDays(7).AddTicks(-1);
                if (weekEnd > endDate) weekEnd = endDate;

                var weekReadings = readings.Where(r => r.ReceivedAt >= weekStart && r.ReceivedAt <= weekEnd).ToList();

                if (weekReadings.Count == 0) continue;

                var weekStats = new WeeklyStats
                {
                    WeekNumber = week + 1,
                    WeekStart = weekStart,
                    WeekEnd = weekEnd,
                    ReadingCount = weekReadings.Count
                };

                var heartRates = new List<float>();
                var temperatures = new List<float>();

                foreach (var reading in weekReadings)
                {
                    try
                    {
                        var readingData = JsonSerializer.Deserialize<PatientReading>(reading.ReadingData ?? "{}");
                        if (readingData?.HeartRate?.FingerDetected == true && readingData.HeartRate.Bpm > 0)
                            heartRates.Add(readingData.HeartRate.Bpm);

                        if (readingData?.Temperature?.SensorOk == true)
                            temperatures.Add(readingData.Temperature.Celsius);
                    }
                    catch { }
                }

                if (heartRates.Count > 0)
                    weekStats.AvgHeartRate = (float)heartRates.Average();
                if (temperatures.Count > 0)
                    weekStats.AvgTemperature = (float)temperatures.Average();

                report.WeeklyBreakdown.Add(weekStats);
            }

            // Monthly aggregates
            var allHeartRates = new List<float>();
            var allTemperatures = new List<float>();
            var allAnomalies = new List<string>();
            var criticalEvents = new List<CriticalEvent>();

            foreach (var reading in readings)
            {
                try
                {
                    var readingData = JsonSerializer.Deserialize<PatientReading>(reading.ReadingData ?? "{}");
                    if (readingData == null) continue;

                    if (readingData.HeartRate?.FingerDetected == true && readingData.HeartRate.Bpm > 0)
                        allHeartRates.Add(readingData.HeartRate.Bpm);

                    if (readingData.Temperature?.SensorOk == true)
                        allTemperatures.Add(readingData.Temperature.Celsius);

                    // Check for critical events
                    if (readingData.Temperature?.Fever == true)
                    {
                        criticalEvents.Add(new CriticalEvent
                        {
                            Timestamp = reading.ReceivedAt,
                            EventType = "FEVER",
                            Description = $"High temperature: {readingData.Temperature.Celsius}°C",
                            Severity = 0.8f
                        });
                    }

                    if (readingData.HeartRate?.HighRate == true)
                    {
                        criticalEvents.Add(new CriticalEvent
                        {
                            Timestamp = reading.ReceivedAt,
                            EventType = "HIGH_HR",
                            Description = $"High heart rate: {readingData.HeartRate.Bpm} BPM",
                            Severity = 0.6f
                        });
                    }

                    if (readingData.Accelerometer?.FallDetected == true)
                    {
                        criticalEvents.Add(new CriticalEvent
                        {
                            Timestamp = reading.ReceivedAt,
                            EventType = "FALL",
                            Description = "Fall detected",
                            Severity = 1.0f
                        });
                    }

                    if (!string.IsNullOrEmpty(reading.DetectedAnomalies))
                    {
                        var anomalies = JsonSerializer.Deserialize<List<string>>(reading.DetectedAnomalies);
                        if (anomalies != null)
                            allAnomalies.AddRange(anomalies);
                    }
                }
                catch { }
            }

            if (allHeartRates.Count > 0)
                report.AvgHeartRate = (float)allHeartRates.Average();
            if (allTemperatures.Count > 0)
                report.AvgTemperature = (float)allTemperatures.Average();

            report.TotalAnomaliesCount = allAnomalies.Count;
            report.TotalAlertsCount = readings.Count(r => !string.IsNullOrEmpty(r.DetectedAnomalies));
            report.CriticalEvents = criticalEvents.OrderByDescending(e => e.Severity).Take(10).ToList();

            // Health recommendations
            report.HealthRecommendations = GenerateHealthRecommendations(allHeartRates, allTemperatures, allAnomalies);

            // Health Trend
            report.HealthTrend = CalculateHealthTrend(report);

            return report;
        }

        // ────────────────────────────────────────────────────────
        // ALERT HISTORY REPORT
        // ────────────────────────────────────────────────────────
        public async Task<AlertHistoryReportData> GenerateAlertHistoryReportAsync(string deviceId, DateTime fromDate, DateTime toDate)
        {
            var readings = await _context.PatientReadings
                .Where(r => r.DeviceId == deviceId && r.ReceivedAt >= fromDate && r.ReceivedAt <= toDate)
                .OrderBy(r => r.ReceivedAt)
                .ToListAsync();

            var report = new AlertHistoryReportData
            {
                DeviceId = deviceId,
                FromDate = fromDate,
                ToDate = toDate
            };

            var allAlerts = new List<string>();

            foreach (var reading in readings)
            {
                try
                {
                    var readingData = JsonSerializer.Deserialize<PatientReading>(reading.ReadingData ?? "{}");
                    if (readingData == null) continue;

                    // Check for alerts
                    var alerts = BuildAlerts(readingData);

                    foreach (var alert in alerts)
                    {
                        allAlerts.Add(alert);
                        report.Alerts.Add(new AlertRecord
                        {
                            AlertType = alert,
                            Timestamp = reading.ReceivedAt,
                            MetricValue = GetAlertMetricValue(alert, readingData)
                        });
                    }
                }
                catch { }
            }

            // Alert frequency
            report.AlertFrequency = allAlerts
                .GroupBy(a => a)
                .ToDictionary(g => g.Key, g => g.Count());

            report.TotalAlerts = allAlerts.Count;

            return report;
        }

        // ────────────────────────────────────────────────────────
        // ANOMALY REPORT
        // ────────────────────────────────────────────────────────
        public async Task<AnomalyReportData> GenerateAnomalyReportAsync(string deviceId, DateTime fromDate, DateTime toDate)
        {
            var readings = await _context.PatientReadings
                .Where(r => r.DeviceId == deviceId && r.ReceivedAt >= fromDate && r.ReceivedAt <= toDate)
                .OrderBy(r => r.ReceivedAt)
                .ToListAsync();

            var report = new AnomalyReportData
            {
                DeviceId = deviceId,
                FromDate = fromDate,
                ToDate = toDate
            };

            var allAnomalies = new List<string>();
            var anomalyPatterns = new Dictionary<string, List<AnomalyPattern>>();

            foreach (var reading in readings)
            {
                try
                {
                    if (!string.IsNullOrEmpty(reading.DetectedAnomalies))
                    {
                        var anomalies = JsonSerializer.Deserialize<List<string>>(reading.DetectedAnomalies);
                        if (anomalies != null)
                        {
                            allAnomalies.AddRange(anomalies);
                            report.ReadingsWithAnomalies++;

                            foreach (var anomaly in anomalies)
                            {
                                report.Anomalies.Add(new AnomalyRecord
                                {
                                    AnomalyType = anomaly,
                                    Timestamp = reading.ReceivedAt,
                                    Severity = 0.5f // Default, can be enhanced
                                });

                                if (!anomalyPatterns.ContainsKey(anomaly))
                                    anomalyPatterns[anomaly] = new List<AnomalyPattern>();

                                var pattern = anomalyPatterns[anomaly].FirstOrDefault(p => p.Date.Date == reading.ReceivedAt.Date);
                                if (pattern != null)
                                    pattern.Count++;
                                else
                                    anomalyPatterns[anomaly].Add(new AnomalyPattern
                                    {
                                        Date = reading.ReceivedAt.Date,
                                        Count = 1
                                    });
                            }
                        }
                    }
                }
                catch { }
            }

            report.AnomalyPatterns = anomalyPatterns;
            report.TotalAnomalies = allAnomalies.Count;
            report.AnomalyFrequency = allAnomalies
                .GroupBy(a => a)
                .ToDictionary(g => g.Key, g => g.Count());

            if (readings.Count > 0)
                report.AnomalyPercentage = (float)(report.ReadingsWithAnomalies * 100.0 / readings.Count);

            return report;
        }

        // ────────────────────────────────────────────────────────
        // HELPER METHODS
        // ────────────────────────────────────────────────────────

        private List<string> BuildAlerts(PatientReading r)
        {
            var alerts = new List<string>();
            if (r.Temperature?.Fever == true) alerts.Add("FEVER");
            if (r.Accelerometer?.FallDetected == true) alerts.Add("FALL_DETECTED");
            if (r.HeartRate?.HighRate == true) alerts.Add("HIGH_HEART_RATE");
            if (r.Ambient?.TempCelsius > 35) alerts.Add("HIGH_AMBIENT_TEMP");
            if (r.Ambient?.Humidity > 80) alerts.Add("HIGH_HUMIDITY");
            if (r.Temperature?.SensorOk == false) alerts.Add("BODY_TEMP_SENSOR_FAIL");
            if (r.SensorStatus?.MpuOk == false) alerts.Add("MPU_FAIL");
            if (r.SensorStatus?.MaxOk == false) alerts.Add("MAX30102_FAIL");
            if (r.SensorStatus?.Ds18Ok == false) alerts.Add("DS18B20_FAIL");
            if (r.SensorStatus?.DhtOk == false) alerts.Add("DHT11_FAIL");
            return alerts;
        }

        private float? GetAlertMetricValue(string alert, PatientReading reading)
        {
            return alert switch
            {
                "FEVER" => reading.Temperature?.Celsius,
                "HIGH_HEART_RATE" => reading.HeartRate?.Bpm,
                "HIGH_AMBIENT_TEMP" => reading.Ambient?.TempCelsius,
                "HIGH_HUMIDITY" => reading.Ambient?.Humidity,
                _ => null
            };
        }

        private List<string> GenerateHealthRecommendations(List<float> heartRates, List<float> temperatures, List<string> anomalies)
        {
            var recommendations = new List<string>();

            if (heartRates.Count > 0)
            {
                var avgHR = heartRates.Average();
                if (avgHR > 100)
                    recommendations.Add("Consider monitoring heart rate more closely. Average HR is elevated.");
                if (avgHR < 60)
                    recommendations.Add("Heart rate is lower than normal. Ensure proper rest and hydration.");
            }

            if (temperatures.Count > 0)
            {
                var avgTemp = temperatures.Average();
                if (avgTemp > 37.5)
                    recommendations.Add("Body temperature is consistently elevated. Consider medical consultation.");
                if (avgTemp < 36.5)
                    recommendations.Add("Body temperature is lower than normal. Ensure adequate nutrition and rest.");
            }

            if (anomalies.Count > temperatures.Count * 0.3) // More than 30% readings have anomalies
            {
                recommendations.Add("High number of anomalies detected. Please review recent readings carefully.");
            }

            if (recommendations.Count == 0)
                recommendations.Add("Health metrics are within normal ranges. Continue monitoring regularly.");

            return recommendations;
        }

        private HealthTrend CalculateHealthTrend(MonthlyReportData report)
        {
            var trend = new HealthTrend();

            // Calculate health score (0-100)
            var score = 100f;

            if (report.AvgHeartRate > 100 || report.AvgHeartRate < 60)
                score -= 10;

            if (report.AvgTemperature > 37.5 || report.AvgTemperature < 36.5)
                score -= 10;

            if (report.TotalAnomaliesCount > (report.TotalReadings * 0.2))
                score -= 15;

            score = Math.Max(0, score);

            trend.HealthScore = score;
            trend.IsImproving = report.WeeklyBreakdown.Count > 1 &&
                GetWeeklyAnomalyCount(report.WeeklyBreakdown[report.WeeklyBreakdown.Count - 1]) <
                GetWeeklyAnomalyCount(report.WeeklyBreakdown[0]);

            if (score < 50)
                trend.TrendDescription = "Health status requires attention";
            else if (score < 75)
                trend.TrendDescription = "Health status is fair";
            else
                trend.TrendDescription = "Health status is good";

            return trend;
        }

        private int GetWeeklyAnomalyCount(WeeklyStats week)
        {
            return week.AnomalyCount;
        }
    }
}

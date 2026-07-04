// ============================================================
//  PDF REPORT GENERATOR
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Ai_healthcare_monitoring_and_assistance_system.Models;

namespace Ai_healthcare_monitoring_and_assistance_system.Services
{
    public class PdfReportGenerator
    {
        // ────────────────────────────────────────────────────────
        // Generate Daily Report PDF
        // ────────────────────────────────────────────────────────
        public static byte[] GenerateDailyReportPdf(DailyReportData report)
        {
            using (var stream = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // Header
                AddHeader(document, "Daily Health Report");
                AddDeviceAndDateInfo(document, report.DeviceId, report.ReportDate);

                // Summary Stats
                AddSectionTitle(document, "Summary Statistics");
                var summaryTable = new PdfPTable(4)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10,
                    SpacingAfter = 10
                };
                summaryTable.SetWidths(new[] { 25f, 25f, 25f, 25f });

                AddTableHeader(summaryTable, "Total Readings", "Avg HR (bpm)", "Avg Temp (°C)", "Activity Readings");
                summaryTable.AddCell(CreateCell(report.TotalReadings.ToString()));
                summaryTable.AddCell(CreateCell(report.AvgHeartRate.ToString("F1")));
                summaryTable.AddCell(CreateCell(report.AvgTemperature.ToString("F1")));
                summaryTable.AddCell(CreateCell(report.ActivityBreakdown.Sum(a => a.Value).ToString()));

                document.Add(summaryTable);

                // Heart Rate Statistics
                AddSectionTitle(document, "Heart Rate Analysis");
                var hrTable = new PdfPTable(4)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10,
                    SpacingAfter = 10
                };
                hrTable.SetWidths(new[] { 25f, 25f, 25f, 25f });

                AddTableHeader(hrTable, "Average", "Maximum", "Minimum", "High Rate Count");
                hrTable.AddCell(CreateCell(report.AvgHeartRate.ToString("F1")));
                hrTable.AddCell(CreateCell(report.MaxHeartRate.ToString("F1")));
                hrTable.AddCell(CreateCell(report.MinHeartRate.ToString("F1")));
                hrTable.AddCell(CreateCell(report.HighHeartRateCount.ToString()));

                document.Add(hrTable);

                // Temperature Statistics
                AddSectionTitle(document, "Temperature Analysis");
                var tempTable = new PdfPTable(4)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10,
                    SpacingAfter = 10
                };
                tempTable.SetWidths(new[] { 25f, 25f, 25f, 25f });

                AddTableHeader(tempTable, "Average", "Maximum", "Minimum", "Fever Count");
                tempTable.AddCell(CreateCell(report.AvgTemperature.ToString("F1")));
                tempTable.AddCell(CreateCell(report.MaxTemperature.ToString("F1")));
                tempTable.AddCell(CreateCell(report.MinTemperature.ToString("F1")));
                tempTable.AddCell(CreateCell(report.FeverCount.ToString()));

                document.Add(tempTable);

                // Activity Breakdown
                if (report.ActivityBreakdown.Count > 0)
                {
                    AddSectionTitle(document, "Activity Levels");
                    var activityTable = new PdfPTable(2)
                    {
                        WidthPercentage = 100,
                        SpacingBefore = 10,
                        SpacingAfter = 10
                    };
                    activityTable.SetWidths(new[] { 50f, 50f });

                    AddTableHeader(activityTable, "Activity Level", "Count");
                    foreach (var activity in report.ActivityBreakdown.OrderByDescending(a => a.Value))
                    {
                        activityTable.AddCell(CreateCell(activity.Key));
                        activityTable.AddCell(CreateCell(activity.Value.ToString()));
                    }

                    document.Add(activityTable);
                }

                // Anomalies Summary
                if (report.AnomaliesSummary.Count > 0)
                {
                    AddSectionTitle(document, "Detected Anomalies");
                    var anomalyTable = new PdfPTable(3)
                    {
                        WidthPercentage = 100,
                        SpacingBefore = 10,
                        SpacingAfter = 10
                    };
                    anomalyTable.SetWidths(new[] { 40f, 30f, 30f });

                    AddTableHeader(anomalyTable, "Anomaly Type", "Count", "Percentage");
                    foreach (var anomaly in report.AnomaliesSummary)
                    {
                        anomalyTable.AddCell(CreateCell(anomaly.AnomalyType));
                        anomalyTable.AddCell(CreateCell(anomaly.Count.ToString()));
                        anomalyTable.AddCell(CreateCell($"{anomaly.Percentage:F1}%"));
                    }

                    document.Add(anomalyTable);
                }

                // Sensor Health
                AddSectionTitle(document, "Sensor Health Status");
                document.Add(CreateParagraph($"MPU6050 (Accel/Gyro): {(report.SensorHealth.MpuHealthy ? "✓ Healthy" : "✗ Issues Detected")}", BaseColor.BLACK));
                document.Add(CreateParagraph($"MAX30102 (Heart Rate): {(report.SensorHealth.MaxHealthy ? "✓ Healthy" : "✗ Issues Detected")}", BaseColor.BLACK));
                document.Add(CreateParagraph($"DS18B20 (Body Temp): {(report.SensorHealth.Ds18Healthy ? "✓ Healthy" : "✗ Issues Detected")}", BaseColor.BLACK));
                document.Add(CreateParagraph($"DHT11 (Ambient): {(report.SensorHealth.DhtHealthy ? "✓ Healthy" : "✗ Issues Detected")}", BaseColor.BLACK));

                AddFooter(document);
                document.Close();

                return stream.ToArray();
            }
        }

        // ────────────────────────────────────────────────────────
        // Generate Weekly Report PDF
        // ────────────────────────────────────────────────────────
        public static byte[] GenerateWeeklyReportPdf(WeeklyReportData report)
        {
            using (var stream = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                AddHeader(document, "Weekly Health Report");
                document.Add(CreateParagraph($"Device: {report.DeviceId}", 12, Font.BOLD));
                document.Add(CreateParagraph($"Period: {report.WeekStartDate:yyyy-MM-dd} to {report.WeekEndDate:yyyy-MM-dd}", 12));

                // Summary
                AddSectionTitle(document, "Weekly Summary");
                var summaryTable = new PdfPTable(4)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10,
                    SpacingAfter = 10
                };
                summaryTable.SetWidths(new[] { 25f, 25f, 25f, 25f });

                AddTableHeader(summaryTable, "Total Readings", "Avg HR (bpm)", "Avg Temp (°C)", "Total Alerts");
                summaryTable.AddCell(CreateCell(report.TotalReadings.ToString()));
                summaryTable.AddCell(CreateCell(report.AvgHeartRate.ToString("F1")));
                summaryTable.AddCell(CreateCell(report.AvgTemperature.ToString("F1")));
                summaryTable.AddCell(CreateCell(report.TotalAlertsCount.ToString()));

                document.Add(summaryTable);

                // Daily Breakdown
                AddSectionTitle(document, "Daily Breakdown");
                var dailyTable = new PdfPTable(5)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10,
                    SpacingAfter = 10
                };
                dailyTable.SetWidths(new[] { 20f, 20f, 20f, 20f, 20f });

                AddTableHeader(dailyTable, "Date", "Readings", "Avg HR", "Avg Temp", "Anomalies");
                foreach (var day in report.DailyBreakdown)
                {
                    dailyTable.AddCell(CreateCell(day.Date.ToString("yyyy-MM-dd")));
                    dailyTable.AddCell(CreateCell(day.ReadingCount.ToString()));
                    dailyTable.AddCell(CreateCell(day.AvgHeartRate.ToString("F1")));
                    dailyTable.AddCell(CreateCell(day.AvgTemperature.ToString("F1")));
                    dailyTable.AddCell(CreateCell(day.AnomalyCount.ToString()));
                }

                document.Add(dailyTable);

                // Top Anomalies
                if (report.TopAnomalies.Count > 0)
                {
                    AddSectionTitle(document, "Top Detected Anomalies");
                    var anomalyTable = new PdfPTable(3)
                    {
                        WidthPercentage = 100,
                        SpacingBefore = 10,
                        SpacingAfter = 10
                    };
                    anomalyTable.SetWidths(new[] { 40f, 30f, 30f });

                    AddTableHeader(anomalyTable, "Anomaly Type", "Count", "Percentage");
                    foreach (var anomaly in report.TopAnomalies)
                    {
                        anomalyTable.AddCell(CreateCell(anomaly.AnomalyType));
                        anomalyTable.AddCell(CreateCell(anomaly.Count.ToString()));
                        anomalyTable.AddCell(CreateCell($"{anomaly.Percentage:F1}%"));
                    }

                    document.Add(anomalyTable);
                }

                AddFooter(document);
                document.Close();

                return stream.ToArray();
            }
        }

        // ────────────────────────────────────────────────────────
        // Generate Monthly Report PDF
        // ────────────────────────────────────────────────────────
        public static byte[] GenerateMonthlyReportPdf(MonthlyReportData report)
        {
            using (var stream = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                AddHeader(document, "Monthly Health Report");
                document.Add(CreateParagraph($"Device: {report.DeviceId}", 12, Font.BOLD));
                document.Add(CreateParagraph($"Month: {report.Year}-{report.Month:D2}", 12));

                // Summary
                AddSectionTitle(document, "Monthly Summary");
                var summaryTable = new PdfPTable(4)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10,
                    SpacingAfter = 10
                };
                summaryTable.SetWidths(new[] { 25f, 25f, 25f, 25f });

                AddTableHeader(summaryTable, "Total Readings", "Avg HR (bpm)", "Avg Temp (°C)", "Health Score");
                summaryTable.AddCell(CreateCell(report.TotalReadings.ToString()));
                summaryTable.AddCell(CreateCell(report.AvgHeartRate.ToString("F1")));
                summaryTable.AddCell(CreateCell(report.AvgTemperature.ToString("F1")));
                summaryTable.AddCell(CreateCell($"{report.HealthTrend.HealthScore:F0}/100"));

                document.Add(summaryTable);

                // Health Trend
                AddSectionTitle(document, "Health Status");
                document.Add(CreateParagraph($"Trend: {report.HealthTrend.TrendDescription}", BaseColor.BLACK));
                document.Add(CreateParagraph($"Status: {(report.HealthTrend.IsImproving ? "Improving ↑" : "Stable →")}", BaseColor.BLACK));

                // Weekly Breakdown
                if (report.WeeklyBreakdown.Count > 0)
                {
                    AddSectionTitle(document, "Weekly Breakdown");
                    var weeklyTable = new PdfPTable(5)
                    {
                        WidthPercentage = 100,
                        SpacingBefore = 10,
                        SpacingAfter = 10
                    };
                    weeklyTable.SetWidths(new[] { 15f, 20f, 20f, 20f, 25f });

                    AddTableHeader(weeklyTable, "Week", "Readings", "Avg HR", "Avg Temp", "Anomalies");
                    foreach (var week in report.WeeklyBreakdown)
                    {
                        weeklyTable.AddCell(CreateCell($"W{week.WeekNumber}"));
                        weeklyTable.AddCell(CreateCell(week.ReadingCount.ToString()));
                        weeklyTable.AddCell(CreateCell(week.AvgHeartRate.ToString("F1")));
                        weeklyTable.AddCell(CreateCell(week.AvgTemperature.ToString("F1")));
                        weeklyTable.AddCell(CreateCell(week.AnomalyCount.ToString()));
                    }

                    document.Add(weeklyTable);
                }

                // Critical Events
                if (report.CriticalEvents.Count > 0)
                {
                    AddSectionTitle(document, "Critical Events");
                    var eventsTable = new PdfPTable(3)
                    {
                        WidthPercentage = 100,
                        SpacingBefore = 10,
                        SpacingAfter = 10
                    };
                    eventsTable.SetWidths(new[] { 20f, 40f, 40f });

                    AddTableHeader(eventsTable, "Type", "Timestamp", "Description");
                    foreach (var evt in report.CriticalEvents.Take(10))
                    {
                        eventsTable.AddCell(CreateCell(evt.EventType));
                        eventsTable.AddCell(CreateCell(evt.Timestamp.ToString("yyyy-MM-dd HH:mm")));
                        eventsTable.AddCell(CreateCell(evt.Description));
                    }

                    document.Add(eventsTable);
                }

                // Health Recommendations
                if (report.HealthRecommendations.Count > 0)
                {
                    AddSectionTitle(document, "Health Recommendations");
                    foreach (var recommendation in report.HealthRecommendations)
                    {
                        document.Add(CreateParagraph($"• {recommendation}", BaseColor.BLACK));
                    }
                }

                AddFooter(document);
                document.Close();

                return stream.ToArray();
            }
        }

        // ────────────────────────────────────────────────────────
        // Generate Alert History Report PDF
        // ────────────────────────────────────────────────────────
        public static byte[] GenerateAlertHistoryReportPdf(AlertHistoryReportData report)
        {
            using (var stream = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                AddHeader(document, "Alert History Report");
                document.Add(CreateParagraph($"Device: {report.DeviceId}", 12, Font.BOLD));
                document.Add(CreateParagraph($"Period: {report.FromDate:yyyy-MM-dd} to {report.ToDate:yyyy-MM-dd}", 12));
                document.Add(CreateParagraph($"Total Alerts: {report.TotalAlerts}", 12, Font.BOLD));

                // Alert Frequency
                if (report.AlertFrequency.Count > 0)
                {
                    AddSectionTitle(document, "Alert Frequency Summary");
                    var freqTable = new PdfPTable(2)
                    {
                        WidthPercentage = 100,
                        SpacingBefore = 10,
                        SpacingAfter = 10
                    };
                    freqTable.SetWidths(new[] { 70f, 30f });

                    AddTableHeader(freqTable, "Alert Type", "Count");
                    foreach (var alert in report.AlertFrequency.OrderByDescending(a => a.Value))
                    {
                        freqTable.AddCell(CreateCell(alert.Key));
                        freqTable.AddCell(CreateCell(alert.Value.ToString()));
                    }

                    document.Add(freqTable);
                }

                // Recent Alerts
                AddSectionTitle(document, "Recent Alerts (Last 50)");
                var alertsTable = new PdfPTable(3)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10,
                    SpacingAfter = 10
                };
                alertsTable.SetWidths(new[] { 25f, 35f, 40f });

                AddTableHeader(alertsTable, "Alert Type", "Timestamp", "Value");
                foreach (var alert in report.Alerts.OrderByDescending(a => a.Timestamp).Take(50))
                {
                    alertsTable.AddCell(CreateCell(alert.AlertType));
                    alertsTable.AddCell(CreateCell(alert.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")));
                    alertsTable.AddCell(CreateCell(alert.MetricValue?.ToString("F2") ?? "N/A"));
                }

                document.Add(alertsTable);

                AddFooter(document);
                document.Close();

                return stream.ToArray();
            }
        }

        // ────────────────────────────────────────────────────────
        // Generate Anomaly Report PDF
        // ────────────────────────────────────────────────────────
        public static byte[] GenerateAnomalyReportPdf(AnomalyReportData report)
        {
            using (var stream = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                AddHeader(document, "Anomaly Detection Report");
                document.Add(CreateParagraph($"Device: {report.DeviceId}", 12, Font.BOLD));
                document.Add(CreateParagraph($"Period: {report.FromDate:yyyy-MM-dd} to {report.ToDate:yyyy-MM-dd}", 12));

                // Summary
                AddSectionTitle(document, "Anomaly Summary");
                var summaryTable = new PdfPTable(4)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10,
                    SpacingAfter = 10
                };
                summaryTable.SetWidths(new[] { 25f, 25f, 25f, 25f });

                AddTableHeader(summaryTable, "Total Anomalies", "Readings with Anomalies", "Anomaly Percentage", "Date Range Days");
                summaryTable.AddCell(CreateCell(report.TotalAnomalies.ToString()));
                summaryTable.AddCell(CreateCell(report.ReadingsWithAnomalies.ToString()));
                summaryTable.AddCell(CreateCell($"{report.AnomalyPercentage:F1}%"));
                summaryTable.AddCell(CreateCell(((int)(report.ToDate - report.FromDate).TotalDays).ToString()));

                document.Add(summaryTable);

                // Anomaly Frequency
                if (report.AnomalyFrequency.Count > 0)
                {
                    AddSectionTitle(document, "Anomaly Frequency");
                    var freqTable = new PdfPTable(2)
                    {
                        WidthPercentage = 100,
                        SpacingBefore = 10,
                        SpacingAfter = 10
                    };
                    freqTable.SetWidths(new[] { 70f, 30f });

                    AddTableHeader(freqTable, "Anomaly Type", "Count");
                    foreach (var anomaly in report.AnomalyFrequency.OrderByDescending(a => a.Value))
                    {
                        freqTable.AddCell(CreateCell(anomaly.Key));
                        freqTable.AddCell(CreateCell(anomaly.Value.ToString()));
                    }

                    document.Add(freqTable);
                }

                // Recent Anomalies
                AddSectionTitle(document, "Recent Anomalies (Last 50)");
                var anomaliesTable = new PdfPTable(4)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10,
                    SpacingAfter = 10
                };
                anomaliesTable.SetWidths(new[] { 25f, 20f, 25f, 30f });

                AddTableHeader(anomaliesTable, "Anomaly Type", "Metric", "Timestamp", "Value");
                foreach (var anomaly in report.Anomalies.OrderByDescending(a => a.Timestamp).Take(50))
                {
                    anomaliesTable.AddCell(CreateCell(anomaly.AnomalyType));
                    anomaliesTable.AddCell(CreateCell(anomaly.MetricType));
                    anomaliesTable.AddCell(CreateCell(anomaly.Timestamp.ToString("yyyy-MM-dd HH:mm")));
                    anomaliesTable.AddCell(CreateCell(anomaly.Value.ToString("F2")));
                }

                document.Add(anomaliesTable);

                AddFooter(document);
                document.Close();

                return stream.ToArray();
            }
        }

        // ────────────────────────────────────────────────────────
        // PDF Helper Methods
        // ────────────────────────────────────────────────────────

        private static void AddHeader(Document document, string title)
        {
            var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD);
            document.Add(CreateParagraph(title, 18, Font.BOLD));
            document.Add(CreateParagraph($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC", 10));
            document.Add(new Paragraph("\n"));
        }

        private static void AddSectionTitle(Document document, string title)
        {
            document.Add(new Paragraph("\n"));
            var sectionFont = FontFactory.GetFont("Arial", 14, Font.BOLD);
            var para = new Paragraph(title, sectionFont)
            {
                SpacingBefore = 10,
                SpacingAfter = 5
            };
            document.Add(para);
        }

        private static void AddDeviceAndDateInfo(Document document, string deviceId, DateTime date)
        {
            document.Add(CreateParagraph($"Device: {deviceId}", 12, Font.BOLD));
            document.Add(CreateParagraph($"Date: {date:yyyy-MM-dd}", 12));
            document.Add(new Paragraph("\n"));
        }

        private static void AddTableHeader(PdfPTable table, params string[] headers)
        {
            var headerFont = FontFactory.GetFont("Arial", 11, Font.BOLD);
            foreach (var header in headers)
            {
                var cell = new PdfPCell(new Phrase(header, headerFont))
                {
                    BackgroundColor = new BaseColor(200, 200, 200),
                    Padding = 5,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                table.AddCell(cell);
            }
        }

        private static PdfPCell CreateCell(string content)
        {
            var cell = new PdfPCell(new Phrase(content))
            {
                Padding = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            return cell;
        }

        private static Paragraph CreateParagraph(string text, BaseColor color)
        {
            var font = FontFactory.GetFont("Arial", 11, color);
            return new Paragraph(text, font) { SpacingAfter = 5 };
        }

        private static Paragraph CreateParagraph(string text, int size = 11, int style = Font.NORMAL)
        {
            var font = FontFactory.GetFont("Arial", size, style);
            var para = new Paragraph(text, font) { SpacingAfter = 5 };
            return para;
        }

        private static void AddFooter(Document document)
        {
            document.Add(new Paragraph("\n\n"));
            document.Add(new Paragraph("─────────────────────────────────────"));
            document.Add(CreateParagraph("AI Patient Health Monitor System", 9));
            document.Add(CreateParagraph("Confidential - For Authorized Use Only", 9));
        }
    }
}

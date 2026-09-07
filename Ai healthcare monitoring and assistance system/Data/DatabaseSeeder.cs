using System.Text.Json;
using Ai_healthcare_monitoring_and_assistance_system.Models;

namespace Ai_healthcare_monitoring_and_assistance_system.Data;

public static class DatabaseSeeder
{
    public static void SeedMockData(HealthMonitorDbContext db)
    {
        var baseTime = DateTime.UtcNow.AddHours(-2);
        var deviceIds = new[] { "ESP_DEVICE_001", "ESP_DEVICE_002", "ESP_DEVICE_003" };
        var anomalies = new[] { "ANOMALY_HEART_RATE", "ANOMALY_BODY_TEMP", "ANOMALY_ACCELERATION" };
        var activities = new[] { "Sedentary", "Light", "Moderate", "Vigorous" };
        var readings = new List<PatientReadingEntity>();

        for (int i = 0; i < 50; i++)
        {
            var deviceId = deviceIds[i % deviceIds.Length];
            var timestamp = baseTime.AddSeconds(i * 2.4);
            var temperature = 36.5f + (float)Random.Shared.NextDouble() * 1.5f;
            var heartRate = 60 + Random.Shared.Next(40);
            var accelX = (float)(Random.Shared.NextDouble() - 0.5) * 2;
            var accelY = (float)(Random.Shared.NextDouble() - 0.5) * 2;
            var accelZ = (float)(Random.Shared.NextDouble() - 0.5) * 2;
            var magnitude = MathF.Sqrt(accelX * accelX + accelY * accelY + accelZ * accelZ);
            var humidity = 40 + Random.Shared.Next(40);
            var ambientTemp = 22 + Random.Shared.Next(6);
            var rssi = -50 - Random.Shared.Next(30);

            var detectedAnomalies = new List<string>();
            if (Random.Shared.Next(100) < 15)
                detectedAnomalies.Add(anomalies[Random.Shared.Next(anomalies.Length)]);

            var hasFever = Random.Shared.Next(100) < 5;
            if (hasFever)
            {
                temperature = 38.5f + (float)Random.Shared.NextDouble();
                detectedAnomalies.Add("ANOMALY_BODY_TEMP");
            }

            var readingData = new PatientReading
            {
                DeviceId = deviceId,
                Timestamp = (long)timestamp.Subtract(DateTime.UnixEpoch).TotalMilliseconds,
                ReceivedAt = timestamp,
                ActivityLevel = activities[Random.Shared.Next(activities.Length)],
                DetectedAnomalies = detectedAnomalies,
                MotionDetected = Random.Shared.Next(100) < 70,
                Temperature = new TemperatureData { Celsius = temperature, Fahrenheit = temperature * 9 / 5 + 32, SensorOk = true, Fever = hasFever },
                HeartRate = new HeartRateData { Bpm = heartRate, AvgBpm = heartRate - 5 + Random.Shared.Next(10), FingerDetected = true, IrRaw = 50000 + Random.Shared.Next(10000), HighRate = heartRate > 100 },
                Accelerometer = new AccelerometerData { X = accelX, Y = accelY, Z = accelZ, Magnitude = magnitude, FallDetected = false },
                Gyroscope = new GyroscopeData
                {
                    X = (float)(Random.Shared.NextDouble() - 0.5) * 50,
                    Y = (float)(Random.Shared.NextDouble() - 0.5) * 50,
                    Z = (float)(Random.Shared.NextDouble() - 0.5) * 50
                },
                Ambient = new AmbientData { TempCelsius = ambientTemp, Humidity = humidity, SensorOk = true },
                SensorStatus = new SensorStatusData { MpuOk = true, MaxOk = true, Ds18Ok = true, DhtOk = true, WifiRssi = rssi }
            };

            readings.Add(new PatientReadingEntity
            {
                DeviceId = deviceId,
                Timestamp = readingData.Timestamp,
                ReceivedAt = timestamp,
                ActivityLevel = readingData.ActivityLevel,
                DetectedAnomalies = JsonSerializer.Serialize(detectedAnomalies),
                ReadingData = JsonSerializer.Serialize(readingData)
            });
        }

        db.PatientReadings.AddRange(readings);
        db.SaveChanges();
        Console.WriteLine($"✓ Seeded {readings.Count} mock patient readings across {deviceIds.Length} devices");
    }
}

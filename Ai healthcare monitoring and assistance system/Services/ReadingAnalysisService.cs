using Ai_healthcare_monitoring_and_assistance_system.Models;
using Microsoft.Extensions.Logging;

namespace Ai_healthcare_monitoring_and_assistance_system.Services;

public static class ReadingAnalysisService
{
    public static bool DetectFall(PatientReading reading, ILogger logger)
    {
        if (reading.Accelerometer is null || reading.Gyroscope is null) return false;
        var accelMagnitude = reading.Accelerometer.Magnitude;
        var gyroMagnitude = MathF.Sqrt(reading.Gyroscope.X * reading.Gyroscope.X + reading.Gyroscope.Y * reading.Gyroscope.Y + reading.Gyroscope.Z * reading.Gyroscope.Z);
        const float accelThreshold = 2.0f;
        const float gyroThreshold = 200f;
        const float combinedThreshold = 2.5f;
        var combinedScore = (accelMagnitude / accelThreshold) + (gyroMagnitude / gyroThreshold);
        if (combinedScore > combinedThreshold && accelMagnitude > accelThreshold)
        {
            logger.LogWarning("[FALL] Potential fall detected! Accel={A:F2}, Gyro={G:F1}, Score={S:F2}", accelMagnitude, gyroMagnitude, combinedScore);
            return true;
        }
        return false;
    }

    public static string DetectActivity(PatientReading reading)
    {
        if (reading.Accelerometer is null) return "Unknown";
        var accelMagnitude = reading.Accelerometer.Magnitude;
        var heartRate = reading.HeartRate?.Bpm ?? 0;
        if (!reading.MotionDetected && accelMagnitude < 0.3f) return "Sedentary";
        if (accelMagnitude < 0.8f || heartRate < 80) return "Light";
        if (accelMagnitude < 1.5f || heartRate < 110) return "Moderate";
        return "Vigorous";
    }

    public static List<string> BuildAlerts(PatientReading r)
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
}

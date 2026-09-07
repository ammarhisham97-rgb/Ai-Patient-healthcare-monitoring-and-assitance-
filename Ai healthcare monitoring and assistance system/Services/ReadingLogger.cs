using Ai_healthcare_monitoring_and_assistance_system.Models;
using Microsoft.Extensions.Logging;

namespace Ai_healthcare_monitoring_and_assistance_system.Services;

public static class ReadingLogger
{
    public static void Log(PatientReading reading, ILogger logger)
    {
        logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        logger.LogInformation("[{Time}] Reading from {DeviceId}", reading.ReceivedAt.ToString("HH:mm:ss"), reading.DeviceId);
        if (reading.Temperature is not null)
        {
            logger.LogInformation("[TEMP]  Body  = {C:F1} °C  /  {F:F1} °F  | SensorOk={Ok}", reading.Temperature.Celsius, reading.Temperature.Fahrenheit, reading.Temperature.SensorOk);
            if (reading.Temperature.Fever) logger.LogWarning("[TEMP]  *** FEVER DETECTED ***");
        }
        if (reading.HeartRate is not null)
        {
            if (reading.HeartRate.FingerDetected)
            {
                logger.LogInformation("[HR]    BPM={Bpm:F0}  Avg={Avg}  IR={IR}", reading.HeartRate.Bpm, reading.HeartRate.AvgBpm, reading.HeartRate.IrRaw);
                if (reading.HeartRate.HighRate) logger.LogWarning("[HR]    *** HIGH HEART RATE ***");
            }
            else logger.LogInformation("[HR]    No finger on sensor");
        }
        if (reading.Accelerometer is not null)
        {
            logger.LogInformation("[MPU]   Accel X={X:F2} Y={Y:F2} Z={Z:F2} | Mag={Mag:F2}", reading.Accelerometer.X, reading.Accelerometer.Y, reading.Accelerometer.Z, reading.Accelerometer.Magnitude);
            if (reading.Accelerometer.FallDetected) logger.LogWarning("[MPU]   *** FALL / IMPACT DETECTED ***");
        }
        if (reading.Gyroscope is not null) logger.LogInformation("[MPU]   Gyro  X={X:F1} Y={Y:F1} Z={Z:F1}", reading.Gyroscope.X, reading.Gyroscope.Y, reading.Gyroscope.Z);
        logger.LogInformation("[PIR]   Motion: {M}  Activity: {A}", reading.MotionDetected ? "YES" : "no", reading.ActivityLevel);
        if (reading.Ambient is not null)
        {
            if (reading.Ambient.SensorOk)
            {
                logger.LogInformation("[DHT]   Ambient = {T:F1} °C  |  Humidity = {H:F1} %", reading.Ambient.TempCelsius, reading.Ambient.Humidity);
                if (reading.Ambient.TempCelsius > 35) logger.LogWarning("[DHT]   *** HIGH AMBIENT TEMPERATURE ***");
                if (reading.Ambient.Humidity > 80) logger.LogWarning("[DHT]   *** HIGH HUMIDITY ***");
            }
            else logger.LogWarning("[DHT]   Sensor FAILED");
        }
        if (reading.SensorStatus is not null) logger.LogInformation("[STAT]  MPU={Mpu} MAX={Max} DS18={Ds} DHT={Dht} RSSI={Rssi} dBm", reading.SensorStatus.MpuOk, reading.SensorStatus.MaxOk, reading.SensorStatus.Ds18Ok, reading.SensorStatus.DhtOk, reading.SensorStatus.WifiRssi);
    }
}

using System.Text.Json.Serialization;

namespace Ai_healthcare_monitoring_and_assistance_system.Models;

public class RegisterRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class LoginRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class PatientReading
{
    [JsonPropertyName("deviceId")]
    public string? DeviceId { get; set; }
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string? ActivityLevel { get; set; }
    public List<string>? DetectedAnomalies { get; set; } = new();
    [JsonPropertyName("accelerometer")]
    public AccelerometerData? Accelerometer { get; set; }
    [JsonPropertyName("gyroscope")]
    public GyroscopeData? Gyroscope { get; set; }
    [JsonPropertyName("temperature")]
    public TemperatureData? Temperature { get; set; }
    [JsonPropertyName("heartRate")]
    public HeartRateData? HeartRate { get; set; }
    [JsonPropertyName("motionDetected")]
    public bool MotionDetected { get; set; }
    [JsonPropertyName("ambient")]
    public AmbientData? Ambient { get; set; }
    [JsonPropertyName("sensorStatus")]
    public SensorStatusData? SensorStatus { get; set; }
}

public class AccelerometerData
{
    [JsonPropertyName("x")] public float X { get; set; }
    [JsonPropertyName("y")] public float Y { get; set; }
    [JsonPropertyName("z")] public float Z { get; set; }
    [JsonPropertyName("magnitude")] public float Magnitude { get; set; }
    [JsonPropertyName("fallDetected")] public bool FallDetected { get; set; }
}

public class GyroscopeData
{
    [JsonPropertyName("x")] public float X { get; set; }
    [JsonPropertyName("y")] public float Y { get; set; }
    [JsonPropertyName("z")] public float Z { get; set; }
}

public class TemperatureData
{
    [JsonPropertyName("celsius")] public float Celsius { get; set; }
    [JsonPropertyName("fahrenheit")] public float Fahrenheit { get; set; }
    [JsonPropertyName("sensorOk")] public bool SensorOk { get; set; }
    [JsonPropertyName("fever")] public bool Fever { get; set; }
}

public class HeartRateData
{
    [JsonPropertyName("bpm")] public float Bpm { get; set; }
    [JsonPropertyName("avgBpm")] public int AvgBpm { get; set; }
    [JsonPropertyName("fingerDetected")] public bool FingerDetected { get; set; }
    [JsonPropertyName("irRaw")] public long IrRaw { get; set; }
    [JsonPropertyName("highRate")] public bool HighRate { get; set; }
}

public class AmbientData
{
    [JsonPropertyName("tempCelsius")] public float TempCelsius { get; set; }
    [JsonPropertyName("humidity")] public float Humidity { get; set; }
    [JsonPropertyName("sensorOk")] public bool SensorOk { get; set; }
}

public class SensorStatusData
{
    [JsonPropertyName("mpuOk")] public bool MpuOk { get; set; }
    [JsonPropertyName("maxOk")] public bool MaxOk { get; set; }
    [JsonPropertyName("ds18Ok")] public bool Ds18Ok { get; set; }
    [JsonPropertyName("dhtOk")] public bool DhtOk { get; set; }
    [JsonPropertyName("wifiRssi")] public int WifiRssi { get; set; }
}

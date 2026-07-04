# Sample Patient Reading Data for API Testing

Use these JSON samples to test the `POST /api/readings` endpoint with different scenarios.

---

## 1. NORMAL HEALTHY READING

**Status:** All sensors OK, no alerts  
**Activity:** Light activity  
**Use Case:** Baseline healthy patient data

```json
{
  "deviceId": "ESP_DEVICE_001",
  "timestamp": 1698243912000,
  "motionDetected": false,
  "accelerometer": {
    "x": 0.05,
    "y": -0.03,
    "z": 9.81,
    "magnitude": 9.82,
    "fallDetected": false
  },
  "gyroscope": {
    "x": 0.2,
    "y": -0.1,
    "z": 0.3
  },
  "temperature": {
    "celsius": 36.8,
    "fahrenheit": 98.24,
    "sensorOk": true,
    "fever": false
  },
  "heartRate": {
    "bpm": 72.0,
    "avgBpm": 70,
    "fingerDetected": true,
    "irRaw": 50000,
    "highRate": false
  },
  "ambient": {
    "tempCelsius": 23.5,
    "humidity": 45.0,
    "sensorOk": true
  },
  "sensorStatus": {
    "mpuOk": true,
    "maxOk": true,
    "ds18Ok": true,
    "dhtOk": true,
    "wifiRssi": -45
  }
}
```

---

## 2. FEVER ALERT READING

**Status:** High body temperature  
**Alert:** FEVER  
**Anomaly:** ANOMALY_BODY_TEMP  
**Use Case:** Patient with fever requiring alert

```json
{
  "deviceId": "ESP_DEVICE_001",
  "timestamp": 1698243914000,
  "motionDetected": true,
  "accelerometer": {
    "x": 0.12,
    "y": -0.08,
    "z": 9.81,
    "magnitude": 9.83,
    "fallDetected": false
  },
  "gyroscope": {
    "x": 0.5,
    "y": -0.3,
    "z": 0.4
  },
  "temperature": {
    "celsius": 38.9,
    "fahrenheit": 102.02,
    "sensorOk": true,
    "fever": true
  },
  "heartRate": {
    "bpm": 95.0,
    "avgBpm": 92,
    "fingerDetected": true,
    "irRaw": 52000,
    "highRate": false
  },
  "ambient": {
    "tempCelsius": 24.0,
    "humidity": 50.0,
    "sensorOk": true
  },
  "sensorStatus": {
    "mpuOk": true,
    "maxOk": true,
    "ds18Ok": true,
    "dhtOk": true,
    "wifiRssi": -50
  }
}
```

---

## 3. HIGH HEART RATE ALERT

**Status:** Elevated heart rate  
**Alert:** HIGH_HEART_RATE  
**Anomaly:** ANOMALY_HEART_RATE  
**Use Case:** Patient in exercise or distress

```json
{
  "deviceId": "ESP_DEVICE_001",
  "timestamp": 1698243916000,
  "motionDetected": true,
  "accelerometer": {
    "x": 0.35,
    "y": -0.42,
    "z": 9.78,
    "magnitude": 9.92,
    "fallDetected": false
  },
  "gyroscope": {
    "x": 2.5,
    "y": -1.8,
    "z": 1.2
  },
  "temperature": {
    "celsius": 37.1,
    "fahrenheit": 98.78,
    "sensorOk": true,
    "fever": false
  },
  "heartRate": {
    "bpm": 135.0,
    "avgBpm": 128,
    "fingerDetected": true,
    "irRaw": 65000,
    "highRate": true
  },
  "ambient": {
    "tempCelsius": 24.5,
    "humidity": 55.0,
    "sensorOk": true
  },
  "sensorStatus": {
    "mpuOk": true,
    "maxOk": true,
    "ds18Ok": true,
    "dhtOk": true,
    "wifiRssi": -55
  }
}
```

---

## 4. FALL DETECTION SCENARIO

**Status:** Sudden impact detected  
**Alert:** FALL_DETECTED  
**Anomaly:** ANOMALY_FALL_DETECTED, ANOMALY_ACCELERATION  
**Use Case:** Potential fall/impact event

```json
{
  "deviceId": "ESP_DEVICE_001",
  "timestamp": 1698243918000,
  "motionDetected": true,
  "accelerometer": {
    "x": 3.2,
    "y": -4.1,
    "z": -2.8,
    "magnitude": 5.65,
    "fallDetected": true
  },
  "gyroscope": {
    "x": 45.8,
    "y": -52.3,
    "z": 38.5
  },
  "temperature": {
    "celsius": 37.0,
    "fahrenheit": 98.6,
    "sensorOk": true,
    "fever": false
  },
  "heartRate": {
    "bpm": 110.0,
    "avgBpm": 105,
    "fingerDetected": true,
    "irRaw": 58000,
    "highRate": true
  },
  "ambient": {
    "tempCelsius": 22.0,
    "humidity": 40.0,
    "sensorOk": true
  },
  "sensorStatus": {
    "mpuOk": true,
    "maxOk": true,
    "ds18Ok": true,
    "dhtOk": true,
    "wifiRssi": -60
  }
}
```

---

## 5. HIGH AMBIENT TEMPERATURE ALERT

**Status:** High room temperature  
**Alert:** HIGH_AMBIENT_TEMP  
**Use Case:** Hot environment (heat wave, equipment malfunction)

```json
{
  "deviceId": "ESP_DEVICE_002",
  "timestamp": 1698243920000,
  "motionDetected": false,
  "accelerometer": {
    "x": 0.08,
    "y": -0.05,
    "z": 9.81,
    "magnitude": 9.82,
    "fallDetected": false
  },
  "gyroscope": {
    "x": 0.1,
    "y": -0.05,
    "z": 0.2
  },
  "temperature": {
    "celsius": 37.3,
    "fahrenheit": 99.14,
    "sensorOk": true,
    "fever": false
  },
  "heartRate": {
    "bpm": 78.0,
    "avgBpm": 75,
    "fingerDetected": true,
    "irRaw": 51000,
    "highRate": false
  },
  "ambient": {
    "tempCelsius": 36.5,
    "humidity": 65.0,
    "sensorOk": true
  },
  "sensorStatus": {
    "mpuOk": true,
    "maxOk": true,
    "ds18Ok": true,
    "dhtOk": true,
    "wifiRssi": -48
  }
}
```

---

## 6. HIGH HUMIDITY ALERT

**Status:** High humidity environment  
**Alert:** HIGH_HUMIDITY  
**Use Case:** Humid environment (sauna, bathroom, outdoor)

```json
{
  "deviceId": "ESP_DEVICE_002",
  "timestamp": 1698243922000,
  "motionDetected": false,
  "accelerometer": {
    "x": 0.02,
    "y": -0.01,
    "z": 9.81,
    "magnitude": 9.81,
    "fallDetected": false
  },
  "gyroscope": {
    "x": 0.0,
    "y": 0.0,
    "z": 0.1
  },
  "temperature": {
    "celsius": 37.1,
    "fahrenheit": 98.78,
    "sensorOk": true,
    "fever": false
  },
  "heartRate": {
    "bpm": 68.0,
    "avgBpm": 66,
    "fingerDetected": true,
    "irRaw": 49000,
    "highRate": false
  },
  "ambient": {
    "tempCelsius": 25.0,
    "humidity": 85.5,
    "sensorOk": true
  },
  "sensorStatus": {
    "mpuOk": true,
    "maxOk": true,
    "ds18Ok": true,
    "dhtOk": true,
    "wifiRssi": -52
  }
}
```

---

## 7. VIGOROUS ACTIVITY (EXERCISE)

**Status:** High activity level detected  
**Activity:** Vigorous  
**Use Case:** Patient exercising

```json
{
  "deviceId": "ESP_DEVICE_001",
  "timestamp": 1698243924000,
  "motionDetected": true,
  "accelerometer": {
    "x": 1.8,
    "y": 2.2,
    "z": 8.5,
    "magnitude": 4.85,
    "fallDetected": false
  },
  "gyroscope": {
    "x": 25.5,
    "y": -18.2,
    "z": 12.8
  },
  "temperature": {
    "celsius": 37.8,
    "fahrenheit": 100.04,
    "sensorOk": true,
    "fever": false
  },
  "heartRate": {
    "bpm": 155.0,
    "avgBpm": 150,
    "fingerDetected": true,
    "irRaw": 70000,
    "highRate": true
  },
  "ambient": {
    "tempCelsius": 24.0,
    "humidity": 60.0,
    "sensorOk": true
  },
  "sensorStatus": {
    "mpuOk": true,
    "maxOk": true,
    "ds18Ok": true,
    "dhtOk": true,
    "wifiRssi": -42
  }
}
```

---

## 8. SEDENTARY STATE

**Status:** No movement detected  
**Activity:** Sedentary  
**Use Case:** Patient at rest/sleeping

```json
{
  "deviceId": "ESP_DEVICE_003",
  "timestamp": 1698243926000,
  "motionDetected": false,
  "accelerometer": {
    "x": 0.01,
    "y": 0.0,
    "z": 9.81,
    "magnitude": 9.81,
    "fallDetected": false
  },
  "gyroscope": {
    "x": 0.0,
    "y": 0.0,
    "z": 0.0
  },
  "temperature": {
    "celsius": 36.9,
    "fahrenheit": 98.42,
    "sensorOk": true,
    "fever": false
  },
  "heartRate": {
    "bpm": 55.0,
    "avgBpm": 56,
    "fingerDetected": true,
    "irRaw": 45000,
    "highRate": false
  },
  "ambient": {
    "tempCelsius": 22.5,
    "humidity": 42.0,
    "sensorOk": true
  },
  "sensorStatus": {
    "mpuOk": true,
    "maxOk": true,
    "ds18Ok": true,
    "dhtOk": true,
    "wifiRssi": -38
  }
}
```

---

## 9. SENSOR FAILURE - HEART RATE SENSOR

**Status:** MAX30102 sensor not responding  
**Alert:** MAX30102_FAIL  
**Use Case:** Device malfunction

```json
{
  "deviceId": "ESP_DEVICE_001",
  "timestamp": 1698243928000,
  "motionDetected": false,
  "accelerometer": {
    "x": 0.03,
    "y": -0.02,
    "z": 9.81,
    "magnitude": 9.81,
    "fallDetected": false
  },
  "gyroscope": {
    "x": 0.1,
    "y": -0.1,
    "z": 0.0
  },
  "temperature": {
    "celsius": 37.0,
    "fahrenheit": 98.6,
    "sensorOk": true,
    "fever": false
  },
  "heartRate": {
    "bpm": 0.0,
    "avgBpm": 0,
    "fingerDetected": false,
    "irRaw": 0,
    "highRate": false
  },
  "ambient": {
    "tempCelsius": 23.0,
    "humidity": 45.0,
    "sensorOk": true
  },
  "sensorStatus": {
    "mpuOk": true,
    "maxOk": false,
    "ds18Ok": true,
    "dhtOk": true,
    "wifiRssi": -50
  }
}
```

---

## 10. SENSOR FAILURE - TEMPERATURE SENSOR

**Status:** DS18B20 temperature sensor failed  
**Alert:** DS18B20_FAIL  
**Use Case:** Temperature sensor malfunction

```json
{
  "deviceId": "ESP_DEVICE_002",
  "timestamp": 1698243930000,
  "motionDetected": true,
  "accelerometer": {
    "x": 0.15,
    "y": -0.1,
    "z": 9.8,
    "magnitude": 9.83,
    "fallDetected": false
  },
  "gyroscope": {
    "x": 0.5,
    "y": -0.3,
    "z": 0.2
  },
  "temperature": {
    "celsius": 0.0,
    "fahrenheit": 32.0,
    "sensorOk": false,
    "fever": false
  },
  "heartRate": {
    "bpm": 72.0,
    "avgBpm": 70,
    "fingerDetected": true,
    "irRaw": 50000,
    "highRate": false
  },
  "ambient": {
    "tempCelsius": 24.0,
    "humidity": 50.0,
    "sensorOk": true
  },
  "sensorStatus": {
    "mpuOk": true,
    "maxOk": true,
    "ds18Ok": false,
    "dhtOk": true,
    "wifiRssi": -55
  }
}
```

---

## 11. SENSOR FAILURE - DHT11 (AMBIENT)

**Status:** DHT11 humidity sensor failed  
**Alert:** DHT11_FAIL, HIGH_HUMIDITY  
**Use Case:** Ambient sensor malfunction

```json
{
  "deviceId": "ESP_DEVICE_003",
  "timestamp": 1698243932000,
  "motionDetected": false,
  "accelerometer": {
    "x": 0.0,
    "y": 0.0,
    "z": 9.81,
    "magnitude": 9.81,
    "fallDetected": false
  },
  "gyroscope": {
    "x": 0.0,
    "y": 0.0,
    "z": 0.0
  },
  "temperature": {
    "celsius": 36.8,
    "fahrenheit": 98.24,
    "sensorOk": true,
    "fever": false
  },
  "heartRate": {
    "bpm": 70.0,
    "avgBpm": 68,
    "fingerDetected": true,
    "irRaw": 49000,
    "highRate": false
  },
  "ambient": {
    "tempCelsius": 0.0,
    "humidity": 0.0,
    "sensorOk": false
  },
  "sensorStatus": {
    "mpuOk": true,
    "maxOk": true,
    "ds18Ok": true,
    "dhtOk": false,
    "wifiRssi": -48
  }
}
```

---

## 12. WEAK WIFI SIGNAL

**Status:** Poor WiFi connection  
**Use Case:** Device far from router

```json
{
  "deviceId": "ESP_DEVICE_001",
  "timestamp": 1698243934000,
  "motionDetected": true,
  "accelerometer": {
    "x": 0.2,
    "y": -0.15,
    "z": 9.8,
    "magnitude": 9.84,
    "fallDetected": false
  },
  "gyroscope": {
    "x": 1.0,
    "y": -0.8,
    "z": 0.5
  },
  "temperature": {
    "celsius": 37.0,
    "fahrenheit": 98.6,
    "sensorOk": true,
    "fever": false
  },
  "heartRate": {
    "bpm": 75.0,
    "avgBpm": 73,
    "fingerDetected": true,
    "irRaw": 50500,
    "highRate": false
  },
  "ambient": {
    "tempCelsius": 23.5,
    "humidity": 48.0,
    "sensorOk": true
  },
  "sensorStatus": {
    "mpuOk": true,
    "maxOk": true,
    "ds18Ok": true,
    "dhtOk": true,
    "wifiRssi": -85
  }
}
```

---

## 13. MULTIPLE ANOMALIES - CRITICAL

**Status:** Multiple issues detected  
**Alerts:** FEVER, HIGH_HEART_RATE, HIGH_HUMIDITY  
**Anomalies:** ANOMALY_BODY_TEMP, ANOMALY_HEART_RATE, ANOMALY_AMBIENT_TEMP  
**Use Case:** Patient in critical condition

```json
{
  "deviceId": "ESP_DEVICE_001",
  "timestamp": 1698243936000,
  "motionDetected": true,
  "accelerometer": {
    "x": 0.25,
    "y": -0.18,
    "z": 9.79,
    "magnitude": 9.85,
    "fallDetected": false
  },
  "gyroscope": {
    "x": 2.0,
    "y": -1.5,
    "z": 1.0
  },
  "temperature": {
    "celsius": 39.5,
    "fahrenheit": 103.1,
    "sensorOk": true,
    "fever": true
  },
  "heartRate": {
    "bpm": 142.0,
    "avgBpm": 138,
    "fingerDetected": true,
    "irRaw": 68000,
    "highRate": true
  },
  "ambient": {
    "tempCelsius": 28.5,
    "humidity": 82.0,
    "sensorOk": true
  },
  "sensorStatus": {
    "mpuOk": true,
    "maxOk": true,
    "ds18Ok": true,
    "dhtOk": true,
    "wifiRssi": -52
  }
}
```

---

## 14. NO FINGER ON SENSOR

**Status:** Finger not detected on heart rate sensor  
**Use Case:** Sensor contact lost

```json
{
  "deviceId": "ESP_DEVICE_002",
  "timestamp": 1698243938000,
  "motionDetected": false,
  "accelerometer": {
    "x": 0.05,
    "y": -0.03,
    "z": 9.81,
    "magnitude": 9.82,
    "fallDetected": false
  },
  "gyroscope": {
    "x": 0.2,
    "y": -0.1,
    "z": 0.3
  },
  "temperature": {
    "celsius": 36.7,
    "fahrenheit": 98.06,
    "sensorOk": true,
    "fever": false
  },
  "heartRate": {
    "bpm": 0.0,
    "avgBpm": 0,
    "fingerDetected": false,
    "irRaw": 0,
    "highRate": false
  },
  "ambient": {
    "tempCelsius": 23.0,
    "humidity": 45.0,
    "sensorOk": true
  },
  "sensorStatus": {
    "mpuOk": true,
    "maxOk": true,
    "ds18Ok": true,
    "dhtOk": true,
    "wifiRssi": -50
  }
}
```

---

## 15. MODERATE ACTIVITY

**Status:** Moderate activity detected  
**Activity:** Moderate  
**Use Case:** Patient walking or light exercise

```json
{
  "deviceId": "ESP_DEVICE_003",
  "timestamp": 1698243940000,
  "motionDetected": true,
  "accelerometer": {
    "x": 0.8,
    "y": -0.6,
    "z": 9.7,
    "magnitude": 3.5,
    "fallDetected": false
  },
  "gyroscope": {
    "x": 8.5,
    "y": -6.2,
    "z": 4.3
  },
  "temperature": {
    "celsius": 37.4,
    "fahrenheit": 99.32,
    "sensorOk": true,
    "fever": false
  },
  "heartRate": {
    "bpm": 105.0,
    "avgBpm": 102,
    "fingerDetected": true,
    "irRaw": 58000,
    "highRate": false
  },
  "ambient": {
    "tempCelsius": 24.5,
    "humidity": 52.0,
    "sensorOk": true
  },
  "sensorStatus": {
    "mpuOk": true,
    "maxOk": true,
    "ds18Ok": true,
    "dhtOk": true,
    "wifiRssi": -45
  }
}
```

---

## HOW TO USE IN POSTMAN

1. **Open Postman**
2. **Create a new POST request**
3. **URL:** `http://localhost:5011/api/readings`
4. **Headers:**
   ```
   Content-Type: application/json
   ```
5. **Body:** Select "raw" → "JSON" and paste one of the samples above
6. **Click Send**

### Expected Response (200 OK):
```json
{
  "status": "received",
  "alerts": ["FEVER", "HIGH_HEART_RATE"],
  "anomalies": ["ANOMALY_BODY_TEMP", "ANOMALY_HEART_RATE"],
  "activityLevel": "Vigorous",
  "stored": 42,
  "serverTime": "2024-01-15T10:30:45.123Z"
}
```

---

## TESTING SEQUENCE

Test in this order to verify all features:

1. **Sample 1** (Normal) - Baseline
2. **Sample 2** (Fever) - Alert system
3. **Sample 3** (High HR) - Another alert
4. **Sample 4** (Fall) - Critical alert
5. **Sample 8** (Sedentary) - Low activity
6. **Sample 7** (Vigorous) - High activity
7. **Sample 9** (Sensor Fail) - Error handling
8. **Sample 13** (Critical) - Multiple issues

This covers all major scenarios!

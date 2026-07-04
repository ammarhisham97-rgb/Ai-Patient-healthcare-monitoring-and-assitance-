# AI Healthcare Monitoring and Assistance System - API Documentation

## 📋 Table of Contents
- [Overview](#overview)
- [Base URL & Configuration](#base-url--configuration)
- [Authentication](#authentication)
- [API Endpoints](#api-endpoints)
  - [Authentication Endpoints](#authentication-endpoints)
  - [Readings Endpoints](#readings-endpoints)
  - [Summary Endpoints](#summary-endpoints)
  - [Reports Endpoints](#reports-endpoints)
- [Data Models & DTOs](#data-models--dtos)
- [Error Handling](#error-handling)
- [Examples](#examples)

---

## Overview

The AI Healthcare Monitoring and Assistance System is an ASP.NET Core minimal API that collects real-time health sensor data from ESP8266 devices and provides comprehensive health monitoring, anomaly detection, and reporting capabilities.

**Key Features:**
- JWT-based authentication
- Real-time sensor data collection (heart rate, temperature, motion, acceleration)
- ML-powered anomaly detection
- Smart fall detection
- Activity level classification
- Comprehensive health reports (daily, weekly, monthly)
- PDF report generation
- Database persistence

**Technology Stack:**
- ASP.NET Core 10 (.NET 10)
- Entity Framework Core (SQL Server)
- ML.NET for anomaly detection
- JWT Bearer Authentication
- BCrypt for password hashing

---

## Base URL & Configuration

```
Base URL: http://localhost:5011
Protocol: HTTP/HTTPS
Content-Type: application/json
CORS: AllowAll (all origins, methods, headers)
```

**Environment Configuration:**
```json
{
  "JwtSecret": "YourSuperSecretKeyForJWTAuthenticationMinimum32Characters!!!",
  "JwtIssuer": "PatientMonitorAPI",
  "JwtAudience": "PatientMonitorClient",
  "ConnectionStrings": {
	"DefaultConnection": "Server=...;Database=HealthMonitorDb;..."
  }
}
```

---

## Authentication

### JWT Bearer Token

All protected endpoints require a JWT Bearer token in the `Authorization` header:

```
Authorization: Bearer <your_jwt_token>
```

**Token Structure:**
- **Issuer:** PatientMonitorAPI
- **Audience:** PatientMonitorClient
- **Expiration:** 3600 seconds (1 hour)
- **Algorithm:** HS256 (HMAC with SHA-256)

**Token Claims:**
```json
{
  "sub": "username",
  "iat": 1234567890,
  "exp": 1234571490,
  "iss": "PatientMonitorAPI",
  "aud": "PatientMonitorClient"
}
```

---

## API Endpoints

### Authentication Endpoints

#### 1. Register User
**POST** `/api/auth/register`

Create a new user account.

**Request Body:**
```json
{
  "username": "john_doe",
  "password": "SecurePassword123!"
}
```

**Response (201 Created):**
```json
{
  "username": "john_doe"
}
```

**Error Responses:**
- `400 Bad Request`: Username and password required
- `400 Bad Request`: User already exists

---

#### 2. Login User
**POST** `/api/auth/login`

Authenticate user and receive JWT token.

**Request Body:**
```json
{
  "username": "john_doe",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

**Error Responses:**
- `400 Bad Request`: Username and password required
- `401 Unauthorized`: Invalid credentials

---

### Readings Endpoints

#### 3. Submit Patient Reading
**POST** `/api/readings`

Submit real-time sensor data from ESP8266 device. Called every 5 seconds by the device.

**Request Body:**
```json
{
  "deviceId": "ESP8266_001",
  "timestamp": 1234567890000,
  "motionDetected": true,
  "accelerometer": {
	"x": 0.05,
	"y": 0.02,
	"z": 9.81,
	"magnitude": 9.82,
	"fallDetected": false
  },
  "gyroscope": {
	"x": 0.1,
	"y": 0.2,
	"z": 0.3
  },
  "temperature": {
	"celsius": 36.5,
	"fahrenheit": 97.7,
	"sensorOk": true,
	"fever": false
  },
  "heartRate": {
	"bpm": 72.5,
	"avgBpm": 70,
	"fingerDetected": true,
	"irRaw": 105234,
	"highRate": false
  },
  "ambient": {
	"tempCelsius": 22.0,
	"humidity": 65.5,
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

**Response (200 OK):**
```json
{
  "status": "received",
  "alerts": [
	"HIGH_TEMPERATURE",
	"ANOMALY_FALL_DETECTED"
  ],
  "anomalies": [
	"ANOMALY_FALL_DETECTED"
  ],
  "activityLevel": "Moderate",
  "stored": 245,
  "serverTime": "2025-05-12T10:30:45.123Z"
}
```

**Error Responses:**
- `400 Bad Request`: Invalid payload (missing deviceId)

**Field Details:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| deviceId | string | ✓ | Unique device identifier |
| timestamp | long | ✓ | Unix timestamp in milliseconds from ESP |
| motionDetected | boolean | ✓ | PIR sensor motion detection |
| accelerometer | object | ✓ | MPU6050 acceleration data |
| gyroscope | object | ✓ | MPU6050 rotation data |
| temperature | object | ✓ | DS18B20 body temperature |
| heartRate | object | ✓ | MAX30102 pulse reading |
| ambient | object | ✓ | DHT22 environmental data |
| sensorStatus | object | ✓ | Sensor health status |

---

#### 4. Get All Readings
**GET** `/api/readings`

Retrieve the last 100 stored readings from all devices.

**Response (200 OK):**
```json
[
  {
	"id": 1,
	"deviceId": "ESP8266_001",
	"timestamp": 1234567890000,
	"receivedAt": "2025-05-12T10:30:45.123Z",
	"activityLevel": "Moderate",
	"detectedAnomalies": "[\"ANOMALY_HIGH_HR\"]",
	"readingData": "{...}"
  },
  {
	"id": 2,
	"deviceId": "ESP8266_001",
	"timestamp": 1234567895000,
	"receivedAt": "2025-05-12T10:30:50.456Z",
	"activityLevel": "Light",
	"detectedAnomalies": "[]",
	"readingData": "{...}"
  }
]
```

---

#### 5. Get Latest Reading
**GET** `/api/readings/latest`

Get the most recent reading from any device.

**Response (200 OK):**
```json
{
  "id": 245,
  "deviceId": "ESP8266_001",
  "timestamp": 1234567895000,
  "receivedAt": "2025-05-12T10:30:50.456Z",
  "activityLevel": "Light",
  "detectedAnomalies": "[]",
  "readingData": "{...}"
}
```

**Error Responses:**
- `404 Not Found`: No readings yet

---

#### 6. Get Readings by Device
**GET** `/api/readings/{deviceId}`

Retrieve all readings for a specific device.

**Path Parameters:**
- `deviceId` (string) - Device identifier (e.g., "ESP8266_001")

**Response (200 OK):**
```json
[
  {
	"id": 1,
	"deviceId": "ESP8266_001",
	"timestamp": 1234567890000,
	"receivedAt": "2025-05-12T10:30:45.123Z",
	"activityLevel": "Moderate",
	"detectedAnomalies": "[\"ANOMALY_HIGH_HR\"]",
	"readingData": "{...}"
  }
]
```

---

### Summary Endpoints

#### 7. Get Anomalies Summary
**GET** `/api/anomalies/summary`

Retrieve summary of detected anomalies across all readings.

**Response (200 OK):**
```json
{
  "totalReadings": 245,
  "readingsWithAnomalies": 12,
  "commonAnomalies": [
	{
	  "anomaly": "ANOMALY_HIGH_HR",
	  "count": 5
	},
	{
	  "anomaly": "ANOMALY_FALL_DETECTED",
	  "count": 3
	},
	{
	  "anomaly": "ANOMALY_FEVER",
	  "count": 4
	}
  ]
}
```

---

#### 8. Get Activity Summary
**GET** `/api/activity/summary`

Get breakdown of detected activity levels.

**Response (200 OK):**
```json
{
  "totalReadings": 245,
  "activityBreakdown": {
	"sedentary": 98,
	"light": 89,
	"moderate": 45,
	"vigorous": 13
  },
  "recentActivity": [
	{
	  "time": "2025-05-12T10:30:50.456Z",
	  "activity": "Light"
	},
	{
	  "time": "2025-05-12T10:30:45.123Z",
	  "activity": "Moderate"
	}
  ]
}
```

---

#### 9. API Health Status
**GET** `/api/status`

Check API health and statistics.

**Response (200 OK):**
```json
{
  "status": "online",
  "totalStored": 245,
  "serverTime": "2025-05-12T10:35:12.789Z"
}
```

---

### Reports Endpoints

#### 10. Get Daily Report
**GET** `/api/reports/daily/{deviceId}`

Generate comprehensive daily health report for a device.

**Path Parameters:**
- `deviceId` (string) - Device identifier

**Response (200 OK):**
```json
{
  "deviceId": "ESP8266_001",
  "reportDate": "2025-05-12",
  "totalReadings": 288,
  "readingsWithAlerts": 12,
  "readingsWithAnomalies": 8,
  "avgHeartRate": 72.5,
  "maxHeartRate": 95.0,
  "minHeartRate": 58.0,
  "highHeartRateCount": 5,
  "avgTemperature": 36.6,
  "maxTemperature": 37.2,
  "minTemperature": 36.2,
  "feverCount": 0,
  "activityBreakdown": {
	"sedentary": 100,
	"light": 120,
	"moderate": 55,
	"vigorous": 13
  },
  "alertsSummary": [
	{
	  "alertType": "HIGH_HEART_RATE",
	  "count": 5,
	  "percentage": 1.74,
	  "firstOccurrence": "2025-05-12T08:15:00Z",
	  "lastOccurrence": "2025-05-12T18:45:00Z"
	}
  ],
  "anomaliesSummary": [
	{
	  "anomalyType": "ANOMALY_HIGH_HR",
	  "count": 3,
	  "percentage": 1.04,
	  "metricType": "HeartRate",
	  "avgValue": 95.2
	}
  ],
  "sensorHealth": {
	"mpuHealthy": true,
	"maxHealthy": true,
	"ds18Healthy": true,
	"dhtHealthy": true,
	"avgWifiSignal": -45.0
  }
}
```

---

#### 11. Get Daily Report PDF
**GET** `/api/reports/daily/{deviceId}/pdf`

Download daily report as PDF file.

**Path Parameters:**
- `deviceId` (string) - Device identifier

**Response (200 OK):**
- **Content-Type:** application/pdf
- **Content-Disposition:** attachment; filename="Daily_Report_ESP8266_001_2025-05-12.pdf"
- **Body:** PDF file binary data

---

#### 12. Get Weekly Report
**GET** `/api/reports/weekly/{deviceId}`

Generate weekly health summary (last 7 days).

**Path Parameters:**
- `deviceId` (string) - Device identifier

**Response (200 OK):**
```json
{
  "deviceId": "ESP8266_001",
  "weekStartDate": "2025-05-06",
  "weekEndDate": "2025-05-12",
  "totalReadings": 2016,
  "dailyBreakdown": [
	{
	  "date": "2025-05-06",
	  "readingCount": 288,
	  "avgHeartRate": 71.2,
	  "avgTemperature": 36.5,
	  "alertCount": 1,
	  "anomalyCount": 0
	}
  ],
  "avgHeartRate": 72.1,
  "avgTemperature": 36.6,
  "totalAlertsCount": 8,
  "totalAnomaliesCount": 5,
  "topAlerts": [
	{
	  "alertType": "HIGH_HEART_RATE",
	  "count": 5,
	  "percentage": 62.5,
	  "firstOccurrence": "2025-05-06T08:00:00Z",
	  "lastOccurrence": "2025-05-12T18:45:00Z"
	}
  ],
  "topAnomalies": [
	{
	  "anomalyType": "ANOMALY_HIGH_HR",
	  "count": 3,
	  "percentage": 60.0,
	  "metricType": "HeartRate",
	  "avgValue": 94.5
	}
  ],
  "activityTrend": {
	"sedentary": 700,
	"light": 840,
	"moderate": 385,
	"vigorous": 91
  }
}
```

---

#### 13. Get Weekly Report PDF
**GET** `/api/reports/weekly/{deviceId}/pdf`

Download weekly report as PDF file.

**Path Parameters:**
- `deviceId` (string) - Device identifier

**Response (200 OK):**
- **Content-Type:** application/pdf
- **Body:** PDF file binary data

---

#### 14. Get Monthly Report
**GET** `/api/reports/monthly/{deviceId}`

Generate monthly health summary.

**Path Parameters:**
- `deviceId` (string) - Device identifier

**Response (200 OK):**
```json
{
  "deviceId": "ESP8266_001",
  "year": 2025,
  "month": 5,
  "totalReadings": 8640,
  "avgHeartRate": 72.3,
  "avgTemperature": 36.7,
  "totalAlertsCount": 35,
  "totalAnomaliesCount": 20,
  "weeklyBreakdown": [
	{
	  "weekNumber": 19,
	  "weekStart": "2025-05-05",
	  "weekEnd": "2025-05-11",
	  "readingCount": 2016,
	  "avgHeartRate": 72.1,
	  "avgTemperature": 36.6,
	  "alertCount": 8,
	  "anomalyCount": 5
	}
  ],
  "healthTrend": {
	"trendDescription": "Stable overall health with occasional elevated heart rate",
	"healthScore": 82.5,
	"isImproving": true,
	"concernAreas": [
	  "Elevated heart rate episodes"
	]
  },
  "criticalEvents": [
	{
	  "timestamp": "2025-05-10T14:30:00Z",
	  "eventType": "FALL_DETECTION",
	  "description": "Fall detected with high impact",
	  "severity": 0.95
	}
  ],
  "healthRecommendations": [
	"Monitor heart rate patterns during activity",
	"Ensure adequate rest periods",
	"Stay hydrated throughout the day"
  ]
}
```

---

#### 15. Get Monthly Report PDF
**GET** `/api/reports/monthly/{deviceId}/pdf`

Download monthly report as PDF file.

**Path Parameters:**
- `deviceId` (string) - Device identifier

**Response (200 OK):**
- **Content-Type:** application/pdf
- **Body:** PDF file binary data

---

#### 16. Get Alert History Report
**GET** `/api/reports/alerts/{deviceId}`

Retrieve alert history for specified time period.

**Path Parameters:**
- `deviceId` (string) - Device identifier

**Query Parameters:**
- `daysBack` (integer, default: 30) - Number of days to look back

**Response (200 OK):**
```json
{
  "deviceId": "ESP8266_001",
  "fromDate": "2025-04-12",
  "toDate": "2025-05-12",
  "alerts": [
	{
	  "alertType": "HIGH_HEART_RATE",
	  "timestamp": "2025-05-10T14:30:00Z",
	  "details": "Heart rate exceeded threshold",
	  "metricValue": 95.5
	},
	{
	  "alertType": "FEVER_DETECTED",
	  "timestamp": "2025-05-08T09:15:00Z",
	  "details": "Body temperature exceeds 38°C",
	  "metricValue": 38.2
	}
  ],
  "alertFrequency": {
	"HIGH_HEART_RATE": 15,
	"FEVER_DETECTED": 3,
	"FALL_DETECTED": 1
  },
  "totalAlerts": 19
}
```

**Example with Query:**
```
GET /api/reports/alerts/ESP8266_001?daysBack=7
```

---

#### 17. Get Alert History Report PDF
**GET** `/api/reports/alerts/{deviceId}/pdf`

Download alert history as PDF file.

**Path Parameters:**
- `deviceId` (string) - Device identifier

**Query Parameters:**
- `daysBack` (integer, default: 30) - Number of days to look back

**Response (200 OK):**
- **Content-Type:** application/pdf
- **Body:** PDF file binary data

---

#### 18. Get Anomalies Report
**GET** `/api/reports/anomalies/{deviceId}`

Retrieve detailed anomaly patterns and frequency.

**Path Parameters:**
- `deviceId` (string) - Device identifier

**Query Parameters:**
- `daysBack` (integer, default: 30) - Number of days to look back

**Response (200 OK):**
```json
{
  "deviceId": "ESP8266_001",
  "fromDate": "2025-04-12",
  "toDate": "2025-05-12",
  "anomalies": [
	{
	  "anomalyType": "ANOMALY_HIGH_HR",
	  "metricType": "HeartRate",
	  "value": 95.5,
	  "timestamp": "2025-05-10T14:30:00Z",
	  "severity": 0.75
	}
  ],
  "anomalyFrequency": {
	"ANOMALY_HIGH_HR": 12,
	"ANOMALY_FEVER": 3,
	"ANOMALY_FALL_DETECTED": 1
  },
  "anomalyPatterns": {
	"ANOMALY_HIGH_HR": [
	  {
		"date": "2025-05-10",
		"count": 3,
		"avgValue": 94.5
	  }
	]
  },
  "totalAnomalies": 16,
  "readingsWithAnomalies": 14,
  "anomalyPercentage": 8.54
}
```

---

#### 19. Get Anomalies Report PDF
**GET** `/api/reports/anomalies/{deviceId}/pdf`

Download anomaly report as PDF file.

**Path Parameters:**
- `deviceId` (string) - Device identifier

**Query Parameters:**
- `daysBack` (integer, default: 30) - Number of days to look back

**Response (200 OK):**
- **Content-Type:** application/pdf
- **Body:** PDF file binary data

---

## Data Models & DTOs

### Request DTOs

#### RegisterRequest
```typescript
interface RegisterRequest {
  username: string;        // Required, unique
  password: string;        // Required, minimum 8 characters recommended
}
```

#### LoginRequest
```typescript
interface LoginRequest {
  username: string;        // Required
  password: string;        // Required
}
```

### Sensor Data DTOs

#### AccelerometerData
```typescript
interface AccelerometerData {
  x: number;              // X-axis acceleration (g)
  y: number;              // Y-axis acceleration (g)
  z: number;              // Z-axis acceleration (g)
  magnitude: number;      // Calculated acceleration magnitude
  fallDetected: boolean;  // Smart fall detection flag
}
```

#### GyroscopeData
```typescript
interface GyroscopeData {
  x: number;              // X-axis rotation (deg/s)
  y: number;              // Y-axis rotation (deg/s)
  z: number;              // Z-axis rotation (deg/s)
}
```

#### TemperatureData
```typescript
interface TemperatureData {
  celsius: number;        // Body temperature in Celsius
  fahrenheit: number;     // Body temperature in Fahrenheit
  sensorOk: boolean;      // DS18B20 sensor status
  fever: boolean;         // Fever alert (>37.5°C)
}
```

#### HeartRateData
```typescript
interface HeartRateData {
  bpm: number;            // Current beats per minute
  avgBpm: number;         // Average BPM
  fingerDetected: boolean;// Finger on sensor detection
  irRaw: number;          // Raw IR reading
  highRate: boolean;      // High heart rate alert (>100 BPM)
}
```

#### AmbientData
```typescript
interface AmbientData {
  tempCelsius: number;    // Ambient temperature
  humidity: number;       // Relative humidity (%)
  sensorOk: boolean;      // DHT22 sensor status
}
```

#### SensorStatusData
```typescript
interface SensorStatusData {
  mpuOk: boolean;         // MPU6050 status
  maxOk: boolean;         // MAX30102 status
  ds18Ok: boolean;        // DS18B20 status
  dhtOk: boolean;         // DHT22 status
  wifiRssi: number;       // WiFi signal strength (dBm)
}
```

#### PatientReading (Full Request)
```typescript
interface PatientReading {
  deviceId: string;                    // Required, unique device ID
  timestamp: number;                   // Unix timestamp (milliseconds)
  motionDetected: boolean;             // PIR motion sensor
  accelerometer: AccelerometerData;    // Required
  gyroscope: GyroscopeData;            // Required
  temperature: TemperatureData;        // Required
  heartRate: HeartRateData;            // Required
  ambient: AmbientData;                // Required
  sensorStatus: SensorStatusData;      // Required
}
```

### Report Response DTOs

#### AlertSummary
```typescript
interface AlertSummary {
  alertType: string;          // Alert type name (HIGH_HEART_RATE, etc.)
  count: number;              // Occurrence count
  percentage: number;         // Percentage of total readings
  firstOccurrence?: string;   // ISO datetime of first occurrence
  lastOccurrence?: string;    // ISO datetime of last occurrence
}
```

#### AnomalySummary
```typescript
interface AnomalySummary {
  anomalyType: string;        // Anomaly type (ANOMALY_HIGH_HR, etc.)
  count: number;              // Occurrence count
  percentage: number;         // Percentage of total readings
  metricType: string;         // Metric affected (HeartRate, Temperature, etc.)
  avgValue: number;           // Average value when anomaly occurred
}
```

#### DailyStats
```typescript
interface DailyStats {
  date: string;               // Date in YYYY-MM-DD format
  readingCount: number;       // Total readings for day
  avgHeartRate: number;       // Average heart rate
  avgTemperature: number;     // Average temperature
  alertCount: number;         // Number of alerts
  anomalyCount: number;       // Number of anomalies
}
```

#### WeeklyStats
```typescript
interface WeeklyStats {
  weekNumber: number;         // ISO week number
  weekStart: string;          // Week start date
  weekEnd: string;            // Week end date
  readingCount: number;       // Total readings
  avgHeartRate: number;       // Average heart rate
  avgTemperature: number;     // Average temperature
  alertCount: number;         // Number of alerts
  anomalyCount: number;       // Number of anomalies
}
```

#### SensorHealthSummary
```typescript
interface SensorHealthSummary {
  mpuHealthy: boolean;        // MPU6050 health
  maxHealthy: boolean;        // MAX30102 health
  ds18Healthy: boolean;       // DS18B20 health
  dhtHealthy: boolean;        // DHT22 health
  avgWifiSignal: number;      // Average WiFi signal (dBm)
}
```

#### HealthTrend
```typescript
interface HealthTrend {
  trendDescription: string;   // Human-readable description
  healthScore: number;        // 0-100 health score
  isImproving: boolean;       // Trend direction
  concernAreas: string[];     // List of concern areas
}
```

#### CriticalEvent
```typescript
interface CriticalEvent {
  timestamp: string;          // ISO datetime
  eventType: string;          // Event type (FALL_DETECTION, etc.)
  description: string;        // Event description
  severity: number;           // 0-1 severity score
}
```

#### AlertRecord
```typescript
interface AlertRecord {
  alertType: string;          // Alert type
  timestamp: string;          // ISO datetime
  details?: string;           // Details
  metricValue?: number;       // Related metric value
}
```

#### AnomalyRecord
```typescript
interface AnomalyRecord {
  anomalyType: string;        // Anomaly type
  metricType: string;         // Metric type (HeartRate, Temperature)
  value: number;              // Anomalous value
  timestamp: string;          // ISO datetime
  severity: number;           // 0-1 severity score
}
```

#### AnomalyPattern
```typescript
interface AnomalyPattern {
  date: string;               // Date in YYYY-MM-DD
  count: number;              // Count on this date
  avgValue: number;           // Average value
}
```

---

## Error Handling

### Standard Error Response Format

```json
{
  "error": "Description of the error"
}
```

### HTTP Status Codes

| Code | Meaning | Description |
|------|---------|-------------|
| 200 | OK | Request successful |
| 201 | Created | Resource created successfully |
| 400 | Bad Request | Invalid request parameters or payload |
| 401 | Unauthorized | Missing or invalid JWT token |
| 404 | Not Found | Resource not found |
| 500 | Internal Server Error | Server-side error |

### Common Error Messages

| Status | Error Message | Solution |
|--------|---------------|----------|
| 400 | Invalid payload | Verify required fields in request |
| 400 | Username and password required | Provide both username and password |
| 400 | User already exists | Use different username |
| 401 | Unauthorized | Provide valid JWT token |
| 404 | No readings yet | Wait for device to send first reading |

---

## Examples

### Complete JavaScript/Vanilla JS Examples

#### 1. User Registration
```javascript
async function registerUser(username, password) {
  try {
	const response = await fetch('http://localhost:5011/api/auth/register', {
	  method: 'POST',
	  headers: {
		'Content-Type': 'application/json'
	  },
	  body: JSON.stringify({
		username: username,
		password: password
	  })
	});

	if (!response.ok) {
	  const error = await response.json();
	  throw new Error(error.error);
	}

	const data = await response.json();
	console.log('User registered:', data);
	return data;
  } catch (error) {
	console.error('Registration failed:', error);
	throw error;
  }
}

// Usage
registerUser('john_doe', 'SecurePassword123!');
```

---

#### 2. User Login & Get Token
```javascript
async function loginUser(username, password) {
  try {
	const response = await fetch('http://localhost:5011/api/auth/login', {
	  method: 'POST',
	  headers: {
		'Content-Type': 'application/json'
	  },
	  body: JSON.stringify({
		username: username,
		password: password
	  })
	});

	if (!response.ok) {
	  throw new Error('Login failed');
	}

	const data = await response.json();
	// Store token in localStorage
	localStorage.setItem('authToken', data.token);
	localStorage.setItem('tokenExpiry', Date.now() + (data.expiresIn * 1000));

	console.log('Login successful');
	return data;
  } catch (error) {
	console.error('Login error:', error);
	throw error;
  }
}

// Usage
loginUser('john_doe', 'SecurePassword123!');
```

---

#### 3. Get Authorization Header with Token
```javascript
function getAuthHeaders() {
  const token = localStorage.getItem('authToken');

  if (!token) {
	throw new Error('No authentication token found');
  }

  return {
	'Authorization': `Bearer ${token}`,
	'Content-Type': 'application/json'
  };
}
```

---

#### 4. Submit Sensor Reading
```javascript
async function submitReading(deviceId, sensorData) {
  try {
	const response = await fetch('http://localhost:5011/api/readings', {
	  method: 'POST',
	  headers: {
		'Content-Type': 'application/json'
	  },
	  body: JSON.stringify({
		deviceId: deviceId,
		timestamp: Date.now(),
		motionDetected: sensorData.motion,
		accelerometer: {
		  x: sensorData.accelX,
		  y: sensorData.accelY,
		  z: sensorData.accelZ,
		  magnitude: Math.sqrt(
			sensorData.accelX ** 2 + 
			sensorData.accelY ** 2 + 
			sensorData.accelZ ** 2
		  ),
		  fallDetected: false
		},
		gyroscope: {
		  x: sensorData.gyroX,
		  y: sensorData.gyroY,
		  z: sensorData.gyroZ
		},
		temperature: {
		  celsius: sensorData.tempC,
		  fahrenheit: (sensorData.tempC * 9/5) + 32,
		  sensorOk: true,
		  fever: sensorData.tempC > 37.5
		},
		heartRate: {
		  bpm: sensorData.heartRate,
		  avgBpm: sensorData.avgHeartRate,
		  fingerDetected: sensorData.fingerDetected,
		  irRaw: sensorData.irValue,
		  highRate: sensorData.heartRate > 100
		},
		ambient: {
		  tempCelsius: sensorData.ambientTemp,
		  humidity: sensorData.humidity,
		  sensorOk: true
		},
		sensorStatus: {
		  mpuOk: true,
		  maxOk: true,
		  ds18Ok: true,
		  dhtOk: true,
		  wifiRssi: -45
		}
	  })
	});

	if (!response.ok) {
	  throw new Error('Failed to submit reading');
	}

	const result = await response.json();
	console.log('Reading submitted:', result);
	return result;
  } catch (error) {
	console.error('Submission error:', error);
	throw error;
  }
}
```

---

#### 5. Get Latest Reading
```javascript
async function getLatestReading() {
  try {
	const response = await fetch('http://localhost:5011/api/readings/latest', {
	  method: 'GET',
	  headers: {
		'Content-Type': 'application/json'
	  }
	});

	if (!response.ok) {
	  if (response.status === 404) {
		console.log('No readings available yet');
		return null;
	  }
	  throw new Error('Failed to fetch reading');
	}

	const data = await response.json();
	console.log('Latest reading:', data);
	return data;
  } catch (error) {
	console.error('Fetch error:', error);
	throw error;
  }
}

// Usage
getLatestReading();
```

---

#### 6. Get Readings by Device
```javascript
async function getDeviceReadings(deviceId) {
  try {
	const response = await fetch(
	  `http://localhost:5011/api/readings/${deviceId}`, 
	  {
		method: 'GET',
		headers: {
		  'Content-Type': 'application/json'
		}
	  }
	);

	if (!response.ok) {
	  throw new Error('Failed to fetch readings');
	}

	const data = await response.json();
	console.log(`Readings for ${deviceId}:`, data);
	return data;
  } catch (error) {
	console.error('Fetch error:', error);
	throw error;
  }
}

// Usage
getDeviceReadings('ESP8266_001');
```

---

#### 7. Get Anomalies Summary
```javascript
async function getAnomaliesSummary() {
  try {
	const response = await fetch('http://localhost:5011/api/anomalies/summary', {
	  method: 'GET',
	  headers: {
		'Content-Type': 'application/json'
	  }
	});

	if (!response.ok) {
	  throw new Error('Failed to fetch anomalies');
	}

	const data = await response.json();
	console.log('Anomalies summary:', data);

	// Display summary
	console.log(`Total readings: ${data.totalReadings}`);
	console.log(`Readings with anomalies: ${data.readingsWithAnomalies}`);

	// Display common anomalies
	data.commonAnomalies.forEach(anomaly => {
	  console.log(`  ${anomaly.anomaly}: ${anomaly.count} occurrences`);
	});

	return data;
  } catch (error) {
	console.error('Fetch error:', error);
	throw error;
  }
}

// Usage
getAnomaliesSummary();
```

---

#### 8. Get Activity Summary
```javascript
async function getActivitySummary() {
  try {
	const response = await fetch('http://localhost:5011/api/activity/summary', {
	  method: 'GET',
	  headers: {
		'Content-Type': 'application/json'
	  }
	});

	if (!response.ok) {
	  throw new Error('Failed to fetch activity summary');
	}

	const data = await response.json();
	console.log('Activity summary:', data);

	// Display breakdown
	console.log('Activity breakdown:');
	console.log(`  Sedentary: ${data.activityBreakdown.sedentary}`);
	console.log(`  Light: ${data.activityBreakdown.light}`);
	console.log(`  Moderate: ${data.activityBreakdown.moderate}`);
	console.log(`  Vigorous: ${data.activityBreakdown.vigorous}`);

	return data;
  } catch (error) {
	console.error('Fetch error:', error);
	throw error;
  }
}

// Usage
getActivitySummary();
```

---

#### 9. Get Daily Report
```javascript
async function getDailyReport(deviceId) {
  try {
	const response = await fetch(
	  `http://localhost:5011/api/reports/daily/${deviceId}`, 
	  {
		method: 'GET',
		headers: {
		  'Content-Type': 'application/json'
		}
	  }
	);

	if (!response.ok) {
	  throw new Error('Failed to fetch daily report');
	}

	const report = await response.json();
	console.log('Daily Report:', report);

	// Display key metrics
	console.log(`Device: ${report.deviceId}`);
	console.log(`Date: ${report.reportDate}`);
	console.log(`Total readings: ${report.totalReadings}`);
	console.log(`Avg Heart Rate: ${report.avgHeartRate} BPM`);
	console.log(`Avg Temperature: ${report.avgTemperature}°C`);
	console.log(`Heart Rate Range: ${report.minHeartRate} - ${report.maxHeartRate}`);
	console.log(`Temperature Range: ${report.minTemperature}°C - ${report.maxTemperature}°C`);

	return report;
  } catch (error) {
	console.error('Fetch error:', error);
	throw error;
  }
}

// Usage
getDailyReport('ESP8266_001');
```

---

#### 10. Download Daily Report as PDF
```javascript
async function downloadDailyReportPDF(deviceId) {
  try {
	const response = await fetch(
	  `http://localhost:5011/api/reports/daily/${deviceId}/pdf`, 
	  {
		method: 'GET'
	  }
	);

	if (!response.ok) {
	  throw new Error('Failed to download PDF');
	}

	// Create blob from response
	const blob = await response.blob();

	// Create download link
	const url = window.URL.createObjectURL(blob);
	const link = document.createElement('a');
	link.href = url;
	link.download = `Daily_Report_${deviceId}_${new Date().toISOString().split('T')[0]}.pdf`;

	// Trigger download
	document.body.appendChild(link);
	link.click();
	document.body.removeChild(link);
	window.URL.revokeObjectURL(url);

	console.log('PDF downloaded successfully');
  } catch (error) {
	console.error('Download error:', error);
	throw error;
  }
}

// Usage
downloadDailyReportPDF('ESP8266_001');
```

---

#### 11. Get Weekly Report
```javascript
async function getWeeklyReport(deviceId) {
  try {
	const response = await fetch(
	  `http://localhost:5011/api/reports/weekly/${deviceId}`, 
	  {
		method: 'GET',
		headers: {
		  'Content-Type': 'application/json'
		}
	  }
	);

	if (!response.ok) {
	  throw new Error('Failed to fetch weekly report');
	}

	const report = await response.json();
	console.log('Weekly Report:', report);

	console.log(`Week: ${report.weekStartDate} to ${report.weekEndDate}`);
	console.log(`Total Readings: ${report.totalReadings}`);
	console.log(`Week Avg Heart Rate: ${report.avgHeartRate} BPM`);
	console.log(`Total Alerts: ${report.totalAlertsCount}`);
	console.log(`Total Anomalies: ${report.totalAnomaliesCount}`);

	// Activity trend for the week
	console.log('Activity Trend:');
	console.log(`  Sedentary: ${report.activityTrend.sedentary}`);
	console.log(`  Light: ${report.activityTrend.light}`);
	console.log(`  Moderate: ${report.activityTrend.moderate}`);
	console.log(`  Vigorous: ${report.activityTrend.vigorous}`);

	return report;
  } catch (error) {
	console.error('Fetch error:', error);
	throw error;
  }
}

// Usage
getWeeklyReport('ESP8266_001');
```

---

#### 12. Get Monthly Report
```javascript
async function getMonthlyReport(deviceId) {
  try {
	const response = await fetch(
	  `http://localhost:5011/api/reports/monthly/${deviceId}`, 
	  {
		method: 'GET',
		headers: {
		  'Content-Type': 'application/json'
		}
	  }
	);

	if (!response.ok) {
	  throw new Error('Failed to fetch monthly report');
	}

	const report = await response.json();
	console.log('Monthly Report:', report);

	console.log(`Month: ${report.month}/${report.year}`);
	console.log(`Total Readings: ${report.totalReadings}`);
	console.log(`Avg Heart Rate: ${report.avgHeartRate}`);
	console.log(`Total Alerts: ${report.totalAlertsCount}`);

	// Health trend
	console.log('Health Trend:');
	console.log(`  Description: ${report.healthTrend.trendDescription}`);
	console.log(`  Health Score: ${report.healthTrend.healthScore}/100`);
	console.log(`  Improving: ${report.healthTrend.isImproving}`);
	console.log(`  Concern Areas: ${report.healthTrend.concernAreas.join(', ')}`);

	// Critical events
	if (report.criticalEvents.length > 0) {
	  console.log('Critical Events:');
	  report.criticalEvents.forEach(event => {
		console.log(`  ${event.eventType} at ${event.timestamp}: ${event.description}`);
	  });
	}

	return report;
  } catch (error) {
	console.error('Fetch error:', error);
	throw error;
  }
}

// Usage
getMonthlyReport('ESP8266_001');
```

---

#### 13. Get Alert History Report
```javascript
async function getAlertHistory(deviceId, daysBack = 30) {
  try {
	const response = await fetch(
	  `http://localhost:5011/api/reports/alerts/${deviceId}?daysBack=${daysBack}`, 
	  {
		method: 'GET',
		headers: {
		  'Content-Type': 'application/json'
		}
	  }
	);

	if (!response.ok) {
	  throw new Error('Failed to fetch alert history');
	}

	const report = await response.json();
	console.log('Alert History Report:', report);

	console.log(`Device: ${report.deviceId}`);
	console.log(`Period: ${report.fromDate} to ${report.toDate}`);
	console.log(`Total Alerts: ${report.totalAlerts}`);

	// Alert frequency
	console.log('Alert Frequency:');
	Object.entries(report.alertFrequency).forEach(([alertType, count]) => {
	  console.log(`  ${alertType}: ${count}`);
	});

	// Recent alerts
	console.log('Recent Alerts:');
	report.alerts.slice(0, 5).forEach(alert => {
	  console.log(`  ${alert.alertType} at ${alert.timestamp}: ${alert.details}`);
	});

	return report;
  } catch (error) {
	console.error('Fetch error:', error);
	throw error;
  }
}

// Usage
getAlertHistory('ESP8266_001', 7);  // Last 7 days
```

---

#### 14. Get Anomalies Report
```javascript
async function getAnomaliesReport(deviceId, daysBack = 30) {
  try {
	const response = await fetch(
	  `http://localhost:5011/api/reports/anomalies/${deviceId}?daysBack=${daysBack}`, 
	  {
		method: 'GET',
		headers: {
		  'Content-Type': 'application/json'
		}
	  }
	);

	if (!response.ok) {
	  throw new Error('Failed to fetch anomalies report');
	}

	const report = await response.json();
	console.log('Anomalies Report:', report);

	console.log(`Device: ${report.deviceId}`);
	console.log(`Total Anomalies: ${report.totalAnomalies}`);
	console.log(`Readings with Anomalies: ${report.readingsWithAnomalies}`);
	console.log(`Anomaly Percentage: ${report.anomalyPercentage.toFixed(2)}%`);

	// Anomaly frequency
	console.log('Anomaly Frequency:');
	Object.entries(report.anomalyFrequency).forEach(([anomalyType, count]) => {
	  console.log(`  ${anomalyType}: ${count}`);
	});

	return report;
  } catch (error) {
	console.error('Fetch error:', error);
	throw error;
  }
}

// Usage
getAnomaliesReport('ESP8266_001', 14);  // Last 2 weeks
```

---

#### 15. API Health Check
```javascript
async function checkAPIHealth() {
  try {
	const response = await fetch('http://localhost:5011/api/status', {
	  method: 'GET',
	  headers: {
		'Content-Type': 'application/json'
	  }
	});

	if (!response.ok) {
	  throw new Error('API is not responding');
	}

	const status = await response.json();
	console.log('API Status:', status);

	console.log(`Status: ${status.status}`);
	console.log(`Total Readings Stored: ${status.totalStored}`);
	console.log(`Server Time: ${status.serverTime}`);

	return status;
  } catch (error) {
	console.error('Health check failed:', error);
	throw error;
  }
}

// Usage
checkAPIHealth();
```

---

### Complete HTML Dashboard Example

```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Health Monitoring Dashboard</title>
  <style>
	* {
	  margin: 0;
	  padding: 0;
	  box-sizing: border-box;
	}

	body {
	  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
	  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
	  min-height: 100vh;
	  padding: 20px;
	}

	.container {
	  max-width: 1200px;
	  margin: 0 auto;
	}

	.header {
	  background: white;
	  padding: 30px;
	  border-radius: 10px;
	  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
	  margin-bottom: 30px;
	}

	.header h1 {
	  color: #333;
	  margin-bottom: 10px;
	}

	.auth-section {
	  display: flex;
	  gap: 10px;
	  margin: 20px 0;
	}

	.auth-section input {
	  padding: 10px;
	  border: 1px solid #ddd;
	  border-radius: 5px;
	  flex: 1;
	}

	.auth-section button {
	  padding: 10px 20px;
	  background: #667eea;
	  color: white;
	  border: none;
	  border-radius: 5px;
	  cursor: pointer;
	  transition: background 0.3s;
	}

	.auth-section button:hover {
	  background: #764ba2;
	}

	.dashboard {
	  display: grid;
	  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
	  gap: 20px;
	}

	.card {
	  background: white;
	  padding: 20px;
	  border-radius: 10px;
	  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
	}

	.card h2 {
	  color: #333;
	  margin-bottom: 15px;
	  font-size: 1.2em;
	}

	.metric {
	  display: flex;
	  justify-content: space-between;
	  align-items: center;
	  padding: 10px 0;
	  border-bottom: 1px solid #eee;
	}

	.metric:last-child {
	  border-bottom: none;
	}

	.metric label {
	  color: #666;
	  font-weight: 500;
	}

	.metric value {
	  color: #333;
	  font-size: 1.1em;
	  font-weight: bold;
	}

	.status-good {
	  color: #27ae60;
	}

	.status-warning {
	  color: #f39c12;
	}

	.status-danger {
	  color: #e74c3c;
	}

	.button-group {
	  display: flex;
	  gap: 10px;
	  margin-top: 15px;
	  flex-wrap: wrap;
	}

	.button-group button {
	  padding: 8px 15px;
	  background: #667eea;
	  color: white;
	  border: none;
	  border-radius: 5px;
	  cursor: pointer;
	  font-size: 0.9em;
	  transition: background 0.3s;
	}

	.button-group button:hover {
	  background: #764ba2;
	}

	.alert {
	  padding: 10px;
	  margin: 10px 0;
	  border-radius: 5px;
	  background: #fff3cd;
	  border-left: 4px solid #ffc107;
	  color: #856404;
	}

	.error {
	  background: #f8d7da;
	  border-left-color: #dc3545;
	  color: #721c24;
	}

	.success {
	  background: #d4edda;
	  border-left-color: #28a745;
	  color: #155724;
	}
  </style>
</head>
<body>
  <div class="container">
	<div class="header">
	  <h1>🏥 Health Monitoring Dashboard</h1>
	  <div class="auth-section">
		<input type="text" id="username" placeholder="Username">
		<input type="password" id="password" placeholder="Password">
		<button onclick="register()">Register</button>
		<button onclick="login()">Login</button>
		<button onclick="logout()">Logout</button>
	  </div>
	  <div id="message"></div>
	</div>

	<div class="dashboard">
	  <!-- Latest Reading Card -->
	  <div class="card">
		<h2>📊 Latest Reading</h2>
		<div id="latest-reading">
		  <p>Click to load latest reading...</p>
		</div>
		<button onclick="loadLatestReading()" style="width: 100%; margin-top: 10px;">Load Latest</button>
	  </div>

	  <!-- Anomalies Summary -->
	  <div class="card">
		<h2>⚠️ Anomalies Summary</h2>
		<div id="anomalies-summary">
		  <p>Click to load anomalies...</p>
		</div>
		<button onclick="loadAnomaliesSummary()" style="width: 100%; margin-top: 10px;">Load Anomalies</button>
	  </div>

	  <!-- Activity Summary -->
	  <div class="card">
		<h2>🏃 Activity Summary</h2>
		<div id="activity-summary">
		  <p>Click to load activity data...</p>
		</div>
		<button onclick="loadActivitySummary()" style="width: 100%; margin-top: 10px;">Load Activity</button>
	  </div>

	  <!-- Daily Report -->
	  <div class="card">
		<h2>📅 Daily Report</h2>
		<input type="text" id="deviceId" placeholder="Device ID (e.g., ESP8266_001)" style="width: 100%; padding: 8px; margin-bottom: 10px;">
		<div id="daily-report">
		  <p>Enter device ID and click to load...</p>
		</div>
		<div class="button-group">
		  <button onclick="loadDailyReport()">Load Report</button>
		  <button onclick="downloadDailyPDF()">Download PDF</button>
		</div>
	  </div>

	  <!-- API Status -->
	  <div class="card">
		<h2>🔌 API Status</h2>
		<div id="api-status">
		  <p>Click to check API status...</p>
		</div>
		<button onclick="checkHealth()" style="width: 100%; margin-top: 10px;">Check Status</button>
	  </div>
	</div>
  </div>

  <script>
	const API_BASE = 'http://localhost:5011';

	// ===== Authentication Functions =====
	function showMessage(message, type = 'info') {
	  const msgDiv = document.getElementById('message');
	  msgDiv.textContent = message;
	  msgDiv.className = 'alert ' + type;
	  setTimeout(() => msgDiv.textContent = '', 5000);
	}

	async function register() {
	  const username = document.getElementById('username').value;
	  const password = document.getElementById('password').value;

	  if (!username || !password) {
		showMessage('Please enter username and password', 'error');
		return;
	  }

	  try {
		const response = await fetch(`${API_BASE}/api/auth/register`, {
		  method: 'POST',
		  headers: { 'Content-Type': 'application/json' },
		  body: JSON.stringify({ username, password })
		});

		if (!response.ok) {
		  const error = await response.json();
		  throw new Error(error.error);
		}

		showMessage(`User ${username} registered successfully!`, 'success');
		document.getElementById('username').value = '';
		document.getElementById('password').value = '';
	  } catch (error) {
		showMessage('Registration failed: ' + error.message, 'error');
	  }
	}

	async function login() {
	  const username = document.getElementById('username').value;
	  const password = document.getElementById('password').value;

	  if (!username || !password) {
		showMessage('Please enter username and password', 'error');
		return;
	  }

	  try {
		const response = await fetch(`${API_BASE}/api/auth/login`, {
		  method: 'POST',
		  headers: { 'Content-Type': 'application/json' },
		  body: JSON.stringify({ username, password })
		});

		if (!response.ok) {
		  throw new Error('Login failed');
		}

		const data = await response.json();
		localStorage.setItem('authToken', data.token);
		localStorage.setItem('tokenExpiry', Date.now() + (data.expiresIn * 1000));

		showMessage('Login successful!', 'success');
		document.getElementById('username').value = '';
		document.getElementById('password').value = '';
	  } catch (error) {
		showMessage('Login failed: ' + error.message, 'error');
	  }
	}

	function logout() {
	  localStorage.removeItem('authToken');
	  localStorage.removeItem('tokenExpiry');
	  showMessage('Logged out successfully', 'success');
	}

	// ===== Data Loading Functions =====
	async function loadLatestReading() {
	  try {
		const response = await fetch(`${API_BASE}/api/readings/latest`);

		if (!response.ok) {
		  throw new Error('No readings available');
		}

		const data = await response.json();
		const html = `
		  <div class="metric">
			<label>Device ID:</label>
			<value>${data.deviceId}</value>
		  </div>
		  <div class="metric">
			<label>Time:</label>
			<value>${new Date(data.receivedAt).toLocaleString()}</value>
		  </div>
		  <div class="metric">
			<label>Activity:</label>
			<value>${data.activityLevel || 'N/A'}</value>
		  </div>
		  <div class="metric">
			<label>Anomalies:</label>
			<value>${data.detectedAnomalies ? JSON.parse(data.detectedAnomalies).length : 0}</value>
		  </div>
		`;
		document.getElementById('latest-reading').innerHTML = html;
	  } catch (error) {
		document.getElementById('latest-reading').innerHTML = `<p class="alert error">${error.message}</p>`;
	  }
	}

	async function loadAnomaliesSummary() {
	  try {
		const response = await fetch(`${API_BASE}/api/anomalies/summary`);

		if (!response.ok) {
		  throw new Error('Failed to load anomalies');
		}

		const data = await response.json();
		let html = `
		  <div class="metric">
			<label>Total Readings:</label>
			<value>${data.totalReadings}</value>
		  </div>
		  <div class="metric">
			<label>With Anomalies:</label>
			<value>${data.readingsWithAnomalies}</value>
		  </div>
		`;

		if (data.commonAnomalies.length > 0) {
		  html += '<div style="margin-top: 10px;"><strong>Common Anomalies:</strong>';
		  data.commonAnomalies.forEach(a => {
			html += `<div class="metric"><label>${a.anomaly}:</label><value>${a.count}</value></div>`;
		  });
		  html += '</div>';
		}

		document.getElementById('anomalies-summary').innerHTML = html;
	  } catch (error) {
		document.getElementById('anomalies-summary').innerHTML = `<p class="alert error">${error.message}</p>`;
	  }
	}

	async function loadActivitySummary() {
	  try {
		const response = await fetch(`${API_BASE}/api/activity/summary`);

		if (!response.ok) {
		  throw new Error('Failed to load activity');
		}

		const data = await response.json();
		const ab = data.activityBreakdown;
		const html = `
		  <div class="metric">
			<label>Sedentary:</label>
			<value>${ab.sedentary}</value>
		  </div>
		  <div class="metric">
			<label>Light:</label>
			<value>${ab.light}</value>
		  </div>
		  <div class="metric">
			<label>Moderate:</label>
			<value>${ab.moderate}</value>
		  </div>
		  <div class="metric">
			<label>Vigorous:</label>
			<value>${ab.vigorous}</value>
		  </div>
		`;
		document.getElementById('activity-summary').innerHTML = html;
	  } catch (error) {
		document.getElementById('activity-summary').innerHTML = `<p class="alert error">${error.message}</p>`;
	  }
	}

	async function loadDailyReport() {
	  const deviceId = document.getElementById('deviceId').value;

	  if (!deviceId) {
		showMessage('Please enter Device ID', 'error');
		return;
	  }

	  try {
		const response = await fetch(`${API_BASE}/api/reports/daily/${deviceId}`);

		if (!response.ok) {
		  throw new Error('Failed to load report');
		}

		const report = await response.json();
		const html = `
		  <div class="metric">
			<label>Total Readings:</label>
			<value>${report.totalReadings}</value>
		  </div>
		  <div class="metric">
			<label>Avg Heart Rate:</label>
			<value class="status-good">${report.avgHeartRate.toFixed(1)} BPM</value>
		  </div>
		  <div class="metric">
			<label>Avg Temperature:</label>
			<value class="status-good">${report.avgTemperature.toFixed(1)}°C</value>
		  </div>
		  <div class="metric">
			<label>Alerts:</label>
			<value>${report.readingsWithAlerts}</value>
		  </div>
		  <div class="metric">
			<label>Anomalies:</label>
			<value>${report.readingsWithAnomalies}</value>
		  </div>
		`;
		document.getElementById('daily-report').innerHTML = html;
	  } catch (error) {
		document.getElementById('daily-report').innerHTML = `<p class="alert error">${error.message}</p>`;
	  }
	}

	async function downloadDailyPDF() {
	  const deviceId = document.getElementById('deviceId').value;

	  if (!deviceId) {
		showMessage('Please enter Device ID', 'error');
		return;
	  }

	  try {
		const response = await fetch(`${API_BASE}/api/reports/daily/${deviceId}/pdf`);

		if (!response.ok) {
		  throw new Error('Failed to download PDF');
		}

		const blob = await response.blob();
		const url = window.URL.createObjectURL(blob);
		const link = document.createElement('a');
		link.href = url;
		link.download = `Daily_Report_${deviceId}_${new Date().toISOString().split('T')[0]}.pdf`;
		document.body.appendChild(link);
		link.click();
		document.body.removeChild(link);
		window.URL.revokeObjectURL(url);

		showMessage('PDF downloaded successfully!', 'success');
	  } catch (error) {
		showMessage('Download failed: ' + error.message, 'error');
	  }
	}

	async function checkHealth() {
	  try {
		const response = await fetch(`${API_BASE}/api/status`);

		if (!response.ok) {
		  throw new Error('API is offline');
		}

		const status = await response.json();
		const html = `
		  <div class="metric">
			<label>Status:</label>
			<value class="status-good">${status.status}</value>
		  </div>
		  <div class="metric">
			<label>Total Stored:</label>
			<value>${status.totalStored}</value>
		  </div>
		  <div class="metric">
			<label>Server Time:</label>
			<value>${new Date(status.serverTime).toLocaleString()}</value>
		  </div>
		`;
		document.getElementById('api-status').innerHTML = html;
	  } catch (error) {
		document.getElementById('api-status').innerHTML = `<p class="alert error">${error.message}</p>`;
	  }
	}
  </script>
</body>
</html>
```

---

## Notes for Frontend Developers

1. **CORS is enabled** - All origins, methods, and headers are allowed
2. **All timestamps** should be in ISO 8601 format (e.g., `2025-05-12T10:30:45.123Z`)
3. **Sessions** - JWT tokens expire in 1 hour, implement token refresh if needed
4. **Error handling** - Always check response status before parsing JSON
5. **Pagination** - Currently returns up to 100 readings; for more data, use specific endpoints like `/api/readings/{deviceId}`
6. **Database storage** - All readings are persisted to SQL Server for historical analysis
7. **PDF generation** - Server-side generated PDFs; client just needs to handle download
8. **Anomaly types** - Common anomalies: `ANOMALY_HIGH_HR`, `ANOMALY_FEVER`, `ANOMALY_FALL_DETECTED`, `ANOMALY_ABNORMAL_ACCELERATION`
9. **Activity levels** - Valid values: `Sedentary`, `Light`, `Moderate`, `Vigorous`
10. **Device IDs** - Should be unique identifiers for each ESP8266 device (e.g., `ESP8266_001`)

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| 401 Unauthorized | Ensure JWT token is valid and not expired. Re-login to get new token. |
| CORS errors | API has CORS enabled. Check browser console for specific error. |
| 404 Not Found | Verify endpoint path and parameters are correct. |
| PDF download issues | Check browser download settings. PDF generation happens server-side. |
| No readings appearing | Wait for device to send first reading (typically ~5 seconds after boot). |
| Token expiry | Implement token refresh logic for long-running single-page apps. |

---

Generated: May 12, 2025
API Version: 1.0

// ==============================================================================
// Project: Kestrel IoT Edge Gateway (Lab 8-4)
// Student ID: 67030281 - Chanin Khamwongsa (ชนินทร์ คำวงศ์ษา)
// Course: Application of Internet of Things (Week 08)
// Architecture: Minimal API + BackgroundService + USB Serial Bridge + SVG Dashboard
// ==============================================================================

using System.IO.Ports;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// 1. ลงทะเบียนบริการใน Dependency Injection Container
builder.Services.AddSingleton<TelemetryStateStore>();
builder.Services.AddHostedService<SerialBridgeWorker>();

var app = builder.Build();

// 2. เปิดใช้งานการเสิร์ฟไฟล์ Static (wwwroot/index.html)
app.UseDefaultFiles();
app.UseStaticFiles();

// ==============================================================================
// 3. MINIMAL API ENDPOINTS
// ==============================================================================

// Endpoint หน้าแรก
app.MapGet("/api/info", () => Results.Ok(new
{
    gatewayName = "Kestrel-ESP32-EdgeGateway",
    version = "1.0.0",
    studentId = "67030281",
    studentName = "Chanin Khamwongsa",
    architecture = ".NET 10 / High-Performance Kestrel",
    uptimeSeconds = Environment.TickCount64 / 1000,
    isOnline = true,
    serverTime = DateTime.UtcNow.ToString("o")
}));

// Endpoint ดึงข้อมูล Telemetry เซนเซอร์ล่าสุดสำหรับ Dashboard
app.MapGet("/api/telemetry", (TelemetryStateStore state) =>
{
    var snapshot = state.GetSnapshot();

    // กำหนดระดับแจ้งเตือนตามสเกลเปอร์เซ็นต์ (Micro-Challenge)
    string alert = snapshot.percent switch
    {
        >= 85.0 => "DANGER (HIGH)",
        >= 70.0 => "WARNING",
        _ => "NORMAL"
    };

    return Results.Ok(new
    {
        sensor = "ESP32_Potentiometer",
        pin = "GPIO34 (ADC1_CH6)",
        rawValue = snapshot.raw,
        voltage = snapshot.voltage,
        percentage = snapshot.percent,
        alertLevel = alert,
        dataSource = snapshot.source,
        lastUpdated = snapshot.updated.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
    });
});

app.Run();

// ==============================================================================
// 4. THREAD-SAFE STATE STORE (คลังเก็บข้อมูลส่วนกลาง)
// ==============================================================================
public class TelemetryStateStore
{
    private readonly object _lock = new();
    private int _rawValue = 0;
    private string _source = "Initializing";
    private DateTime _lastUpdated = DateTime.UtcNow;

    public void Update(int rawValue, string source)
    {
        lock (_lock)
        {
            _rawValue = Math.Clamp(rawValue, 0, 4095);
            _source = source;
            _lastUpdated = DateTime.UtcNow;
        }
    }

    public (int raw, double voltage, double percent, string source, DateTime updated) GetSnapshot()
    {
        lock (_lock)
        {
            double voltage = Math.Round((_rawValue / 4095.0) * 3.3, 2);
            double percent = Math.Round((_rawValue / 4095.0) * 100.0, 1);
            return (_rawValue, voltage, percent, _source, _lastUpdated);
        }
    }
}

// ==============================================================================
// 5. BACKGROUND WORKER: SERIAL BRIDGE & SIMULATOR
// ==============================================================================
public class SerialBridgeWorker : BackgroundService
{
    private readonly TelemetryStateStore _stateStore;
    private readonly ILogger<SerialBridgeWorker> _logger;

    public SerialBridgeWorker(TelemetryStateStore stateStore, ILogger<SerialBridgeWorker> logger)
    {
        _stateStore = stateStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        _logger.LogInformation("🚀 [Worker] Serial Bridge Background Service Started");

        while (!stoppingToken.IsCancellationRequested)
        {
            string[] availablePorts = SerialPort.GetPortNames();

            if (availablePorts.Length > 0)
            {
                _logger.LogInformation("📋 [Worker] รายการ COM Ports: [{Ports}]", string.Join(", ", availablePorts));

                string targetPort = availablePorts.FirstOrDefault(p => !p.Equals("COM1", StringComparison.OrdinalIgnoreCase)) ?? availablePorts[0];
                _logger.LogInformation("🔌 [Worker] กำลังเชื่อมต่อไปยังพอร์ต {Port}...", targetPort);

                try
                {
                    using var serial = new SerialPort(targetPort, 115200)
                    {
                        ReadTimeout = 2000,
                        DtrEnable = true,
                        RtsEnable = true
                    };

                    serial.Open();
                    serial.DiscardInBuffer();
                    _logger.LogInformation("✅ [Worker] เชื่อมต่อสำเร็จ! สตรีมข้อมูลสดจาก {Port}", targetPort);

                    while (!stoppingToken.IsCancellationRequested && serial.IsOpen)
                    {
                        try
                        {
                            if (serial.BytesToRead > 0)
                            {
                                string line = serial.ReadLine().Trim();
                                if (int.TryParse(line, out int adcVal))
                                {
                                    _stateStore.Update(adcVal, $"Live Hardware ({targetPort})");
                                }
                            }
                            else
                            {
                                await Task.Delay(50, stoppingToken);
                            }
                        }
                        catch (TimeoutException)
                        {
                            await Task.Delay(50, stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("⚠️ [Worker] ไม่สามารถเปิด {Port} ({Error}) -> สลับเข้าโหมดจำลอง", targetPort, ex.Message);
                }
            }
            else
            {
                _logger.LogInformation("🔍 ไม่พบพอร์ต USB -> ทำงานในโหมดจำลอง (Simulation Mode)");
            }

            // Fallback Simulation Mode (Sine Wave)
            for (int i = 0; i < 20 && !stoppingToken.IsCancellationRequested; i++)
            {
                double timeSec = Environment.TickCount64 / 1000.0;
                int simulatedAdc = (int)((Math.Sin(timeSec * 1.5) + 1.0) / 2.0 * 4095);
                _stateStore.Update(simulatedAdc, "Auto Simulator (Sine Wave)");
                await Task.Delay(100, stoppingToken);
            }
        }
    }
}


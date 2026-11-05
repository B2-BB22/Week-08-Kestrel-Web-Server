var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", () => "Welcome to IoT Edge Gateway by Chanin Khamwongsa (67030281)");
app.MapGet("/api/status", () => new {
    gateway = "ESP32-EdgeGateway",
    status = "Online",
    uptimeSeconds = Environment.TickCount64 / 1000,
    isHealthy = true
});

app.MapGet("/api/led/{state}", (string state) => {
    string action = state.ToLower() == "on" ? "TURN ON 💡" : "TURN OFF 🌑";
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] LED Control: {state}");
    return Results.Ok(new {
        device = "LED_D2",
        requestedState = state,
        actionResult = action,
        serverTime = DateTime.Now.ToString("HH:mm:ss")
    });
});

// Micro-Challenge Endpoint
app.MapGet("/api/student", () => Results.Ok(new {
    studentId = "67030281",
    studentName = "Chanin Khamwongsa",
    faculty = "คณะวิศวกรรมศาสตร์ สาขาวิศวกรรมคอมพิวเตอร์",
    targetSensor = "Potentiometer (10k) & LDR",
    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
}));

app.Run();
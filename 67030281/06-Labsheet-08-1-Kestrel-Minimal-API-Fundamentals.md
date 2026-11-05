# ใบงานการทดลองที่ 8.1 (Labsheet 8.1) - ฉบับบันทึกผลและตอบคำถาม
## พื้นฐาน Kestrel Web Server และ Minimal API

**รหัสนักศึกษา:** 67030281  
**ชื่อ-นามสกุล:** นายชนินทร์ คำวงศ์ษา 

---

## วัตถุประสงค์การทดลอง (Objectives)
1. สามารถติดตั้งและตรวจสอบสภาพแวดล้อมการทำงานของ .NET 8 / 10 SDK ได้
2. สามารถสร้างและรัน Kestrel Web Server ด้วยคำสั่ง .NET CLI ได้อย่างถูกต้อง
3. เข้าใจโครงสร้างของไฟล์โปรเจกต์ `.csproj` และไฟล์โค้ดหลัก `Program.cs`
4. สามารถสร้าง Minimal API Endpoint ที่ส่งคืนข้อมูลแบบสตริงธรรมดาและออบเจกต์ JSON อัตโนมัติได้
5. สามารถเขียน Endpoint รับค่าพารามิเตอร์ผ่าน URL Path ได้ด้วยตนเอง

---

## สรุปขั้นตอนและการทดลองจริง

### กิจกรรมที่ 1: การตรวจสอบความพร้อมของระบบและสร้างโปรเจกต์
1. ตรวจสอบเวอร์ชันของ .NET SDK
   ```powershell
   dotnet --version
   # ผลลัพธ์: 10.0.400
   ```
2. สร้างโปรเจกต์ Web เปล่าในโฟลเดอร์ `Lab8-1`
   ```powershell
   mkdir Lab8-1 && cd Lab8-1
   dotnet new web -o .
   ```
3. สั่งรันเซิร์ฟเวอร์
   ```powershell
   dotnet run
   ```
   **ผลลัพธ์บน Terminal:**
   ```text
   info: Microsoft.Hosting.Lifetime[14]
         Now listening on: http://localhost:5000
   info: Microsoft.Hosting.Lifetime[0]
         Application started. Press Ctrl+C to shut down.
   ```
4. เปิดเว็บเบราว์เซอร์ไปที่ `http://localhost:5000`  
   **สิ่งที่เห็น:** แสดงข้อความ `"Hello World!"`

---

### [Checkpoint 1.1 ทดสอบความเข้าใจ]

1. ในหน้าจอ Terminal ขณะที่เซิร์ฟเวอร์กำลังรันอยู่ ให้กดปุ่ม `Ctrl + C` เพื่อหยุดโปรแกรม
2. กลับไปที่หน้าเบราว์เซอร์แล้วกดปุ่ม **Refresh (F5)** สังเกตว่าเกิดอะไรขึ้น และอธิบายสั้นๆ ว่าทำไมจึงเป็นเช่นนั้น
   - **คำตอบ:**  
     - **สิ่งที่เกิดขึ้น:** บนหน้าจอเว็บเบราว์เซอร์จะแสดงข้อผิดพลาดว่า **"This site can’t be reached" (ERR_CONNECTION_REFUSED)** หรือไม่สามารถเข้าถึงเว็บไซต์ได้
     - **คำอธิบายสาเหตุ:** เนื่องจาก Kestrel Web Server ทำงานในลักษณะ In-Process Self-Hosted Web Server เมื่อเรากด `Ctrl + C` จะเป็นการส่งสัญญาณ `SIGINT` เพื่อยุติโปรเซส (Process Termination) ของแอปพลิเคชัน ส่งผลให้ TCP Socket Listener ที่คอยเปิดพอร์ตดักรับ HTTP Request (เช่น พอร์ต 5000) ถูกปิดตัวลง เมื่อเบราว์เซอร์ส่ง HTTP GET Request ไปยัง `localhost:5000` จึงไม่มีโปรแกรมใดรับการเชื่อมต่อ ระบบปฏิบัติการจึงปฏิเสธการเชื่อมต่อกลับมาทันที

---

### กิจกรรมที่ 2: การแก้ไขข้อความทักทายใน Program.cs
แก้ไขใน `Program.cs` ให้แสดงชื่อของตนเอง:
```csharp
app.MapGet("/", () => "Welcome to IoT Edge Gateway by Chanin Khamwongsa (ชนินทร์ คำวงศ์ษา) - 67030281!");
```
เมื่อ Refresh หน้าเว็บ ผลลัพธ์แสดงข้อความต้อนรับพร้อมชื่อและรหัสนักศึกษาอย่างถูกต้อง

---

### กิจกรรมที่ 3: การแปลง C# Object เป็น JSON อัตโนมัติ
เพิ่ม Endpoint `/api/status`:
```csharp
app.MapGet("/api/status", () => new {
    gateway = "ESP32-EdgeGateway",
    status = "Online",
    uptimeSeconds = Environment.TickCount64 / 1000,
    isHealthy = true
});
```
เมื่อเปิด URL `http://localhost:5000/api/status` จะได้รับ JSON Payload ดังนี้:
```json
{
  "gateway": "ESP32-EdgeGateway",
  "status": "Online",
  "uptimeSeconds": 15,
  "isHealthy": true
}
```
**ข้อสังเกต:** .NET ทำการ Serialize C# Anonymous Object เป็น JSON String และใส่ Response Header `Content-Type: application/json; charset=utf-8` ให้โดยอัตโนมัติ

---

### กิจกรรมที่ 4: การรับค่าผ่าน URL Path (Route Parameters)
เพิ่ม Endpoint ควบคุม LED ผ่าน URL:
```csharp
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
```
**การทดสอบ:**
- เรียก `http://localhost:5000/api/led/on`
  - คืนค่า:
    ```json
    {
      "device": "LED_D2",
      "requestedState": "on",
      "actionResult": "TURN ON 💡",
      "serverTime": "20:45:10"
    }
    ```
  - บน Terminal แสดง: `[20:45:10] LED Control: on`
- เรียก `http://localhost:5000/api/led/off`
  - คืนค่า:
    ```json
    {
      "device": "LED_D2",
      "requestedState": "off",
      "actionResult": "TURN OFF 🌑",
      "serverTime": "20:45:18"
    }
    ```
  - บน Terminal แสดง: `[20:45:18] LED Control: off`

---

## ภารกิจท้าทาย (Micro-Challenge)

### 1. โค้ด Endpoint `/api/student` ที่พัฒนาเพิ่ม
```csharp
// Micro-Challenge Endpoint: ข้อมูลประจำตัวนักศึกษา
app.MapGet("/api/student", () => Results.Ok(new {
    studentId = "67030281",
    studentName = "Chanin Khamwongsa",
    faculty = "คณะวิศวกรรมศาสตร์ สาขาวิศวกรรมคอมพิวเตอร์",
    targetSensor = "Potentiometer (10k) & LDR",
    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
}));
```

### 2. ผลลัพธ์ที่ได้จากการเรียกทดสอบ (`http://localhost:5000/api/student`)
```json
{
  "studentId": "67030281",
  "studentName": "Chanin Khamwongsa",
  "faculty": "คณะวิศวกรรมศาสตร์ สาขาวิศวกรรมคอมพิวเตอร์",
  "targetSensor": "Potentiometer (10k) & LDR",
  "timestamp": "2026-11-05 20:49:00"
}
```

---

## คำถามท้ายการทดลอง (Review Questions)

### ข้อ 1: ในสถาปัตยกรรมของ Kestrel ตัวแปร `builder` ทำหน้าที่อะไร และตัวแปร `app` ทำหน้าที่อะไร
**คำตอบ:**
1. **ตัวแปร `builder` (ประเภท `WebApplicationBuilder`):**
   - ทำหน้าที่ในการกำหนดค่าคอนฟิกูเรชันของระบบ (Application Configuration) เช่น การโหลดการตั้งค่าจาก `appsettings.json`, ตัวแปรสภาพแวดล้อม (Environment Variables) หรือ Command-line Arguments
   - ทำหน้าที่จัดการและลงทะเบียนบริการ (Services) ต่างๆ ลงใน **Dependency Injection (DI) Container** เช่น `builder.Services.AddSingleton<...>()`, `builder.Services.AddHostedService<...>()`
   - เตรียมความพร้อมของ Web Host และ Kestrel Server Options ก่อนจะสร้างเป็น Web Application
2. **ตัวแปร `app` (ประเภท `WebApplication`):**
   - สร้างขึ้นจากการเรียกคำสั่ง `builder.Build()` ซึ่งถือว่าเป็นการสร้างอินสแตนซ์ของ Web Application ที่ประกอบ DI Container และ Configuration เรียบร้อยแล้ว
   - ทำหน้าที่กำหนดโครงสร้าง **HTTP Request Processing Pipeline (Middleware)** เช่น การสั่ง `app.UseStaticFiles()`, `app.UseRouting()`
   - ทำหน้าที่กำหนดเส้นทางคำขอ (Endpoint Routing) เช่น `app.MapGet()`, `app.MapPost()`
   - ทำหน้าที่เริ่มต้นเปิดพอร์ตและดักรับฟังคำขอเครือข่ายด้วยคำสั่ง `app.Run()`

---

### ข้อ 2: เปรียบเทียบความสะดวกระหว่างการสร้าง Web Server บน .NET Minimal API กับการรันผ่าน LAMP Stack (Apache + PHP) ว่ามีข้อดีข้อเสียต่างกันอย่างไรในมุมมองของงาน IoT Gateway
**คำตอบ:**

| หัวข้อเปรียบเทียบ | .NET Minimal API (Kestrel Self-Hosted) | LAMP Stack (Linux, Apache, MySQL, PHP) |
| :--- | :--- | :--- |
| **สถาปัตยกรรม (Architecture)** | **Single Binary / In-Process Host:** Kestrel ฝังอยู่ในตัวโปรแกรม ทำงานเป็นโปรเซสเดียวจบ ไม่ต้องพึ่งพา Web Server ภายนอก | **Multi-tier Modular:** ต้องติดตั้งและคอนฟิกแยกกันระหว่าง Apache (Web Server), PHP-FPM/Module, และ MySQL (Database) |
| **การเชื่อมต่อ Hardware I/O / Serial Port** | **ดีเยี่ยมมาก:** สามารถเปิด Serial Port (`System.IO.Ports`) ค้างไว้ในหน่วยความจำผ่าน `BackgroundService` ได้ตลอดเวลา และแชร์ค่าสู่ API ผ่าน Thread-Safe State ในกระบวนการเดียวกัน | **ทำได้ยากและไม่เหมาะสม:** PHP มีโมเดลการทำงานแบบ Request-Response (Stateless / Shared-Nothing) เมื่อ Request จบ โปรเซส/เธรดจะถูกรีเซ็ต ไม่สามารถถือ Persistent Connection ไปยัง Serial Port ค้างไว้ได้โดยตรง (ต้องเขียน Service ภายนอกมารับแทน) |
| **ประสิทธิภาพและความเร็ว (Performance)** | **สูงมาก (High Throughput, Low Latency):** รันบน .NET Core Runtime ที่มี Non-blocking Asynchronous I/O กินแรมและ CPU ต่ำมาก เหมาะกับอุปกรณ์ Edge IoT เช่น Raspberry Pi | **ค่อนข้างหนัก:** Apache และ PHP มี Overhead ของการสร้าง Context และแปล Script ในแต่ละ Request ใช้แรมมากกว่า |
| **การติดตั้งและ Deploy** | **สะดวกมาก:** เพียงก็อปปี้โฟลเดอร์หรือ Single Executable File ไปวาง สามารถสั่งรันได้ทันที (`dotnet run` หรือรันไฟล์ไบนารีเดี่ยว) | **ซับซ้อนกว่า:** ต้องติดตั้ง Apache, PHP Runtime, Extensions, คอนฟิก Virtual Host และโฟลเดอร์ `/var/www/html` |
| **ข้อจำกัด** | ต้องคอมไพล์โค้ดก่อนรัน เหมาะกับงาน Microservices / IoT Web Gateway | เหมาะสำหรับเว็บไซต์ทั่วไปที่มีเนื้อหาแบบ CMS หรือ Dynamic HTML หน้าเยอะๆ มากกว่างาน Edge I/O |

---

### ข้อ 3: นักศึกษาคิดว่าการเพิ่ม `/api/` เข้าไปใน route นั้นมีประโยชน์อย่างไรบ้าง ถ้าไม่ใส่จะเกิดปัญหาอะไรบ้าง
**คำตอบ:**
**1. ประโยชน์ของการใส่ `/api/`:**
- **การแยกส่วนความรับผิดชอบ (Separation of Concerns):** ช่วยแบ่งแยกอย่างชัดเจนระหว่าง Endpoint ที่ให้บริการข้อมูลดิบ (Data Service / JSON APIs) กับเส้นทางที่ใช้เสิร์ฟหน้าเว็บหรือทรัพยากรคงที่ (Static Web Assets เช่น HTML, CSS, JS, Images)
- **การกำหนดค่า Reverse Proxy & Gateway:** ทำให้การตั้งค่าพร็อกซีย้อนกลับ (เช่น Nginx, YARP, Cloudflare Gateway) ทำได้สะดวกมาก โดยสามารถตั้งกฎ (Rule) ได้ง่าย เช่น ถ้าเส้นทางขึ้นต้นด้วย `/api/*` ให้ส่งต่อไปยัง Backend Server หรือ Edge Service ส่วนเส้นทางอื่นให้ดึงจาก Web Cache/CDN
- **การจัดการความปลอดภัยและ Middleware (Security & Policies):** สามารถเขียน Middleware กรองคำขอเฉพาะกลุ่ม `/api/*` ได้อย่างเป็นระบบ เช่น การตรวจ API Key, การทำ Rate Limiting หรือการกำหนด CORS Policy สำหรับข้อมูล
- **การรองรับ Versioning ในอนาคต:** สามารถขยายเป็น `/api/v1/telemetry`, `/api/v2/telemetry` ได้ง่ายโดยไม่กระทบโครงสร้างหน้าเว็บ

**2. ปัญหาที่อาจเกิดขึ้นหากไม่ใส่ `/api/`:**
- **Route Collision (การชนกันของเส้นทาง):** ชื่อของ API อาจไปทับซ้อนกับไฟล์หรือหน้าเพจของเว็บไซต์ เช่น หากตั้ง Endpoint ว่า `/status` หรือ `/led` แล้วในโฟลเดอร์ `wwwroot` มีไฟล์ชื่อ `status.html` หรือโฟลเดอร์ `led` อยู่ Kestrel อาจสับสนว่าจะเสิร์ฟไฟล์สถิตหรือส่งต่อไปยัง API Controller
- **ความสับสนในการพัฒนาระบบ Single Page Application (SPA):** ในระบบเว็บที่ใช้ Client-side Routing (เช่น React, Vue) การเรียก URL ที่ไม่มี `/api/` นำหน้าอาจทำให้ Web Server ส่งหน้า `index.html` กลับไปแทนที่จะส่งข้อมูล JSON ทำให้แอปพลิเคชันทำงานผิดพลาด


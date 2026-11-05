# ใบงานการทดลองที่ 8.1 (Labsheet 8.1)
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
     - **สิ่งที่เกิดขึ้น:** บนหน้าเว็บจะเข้าไม่ได้ แล้วขึ้นข้อความว่า **"This site can’t be reached"** หรือมี error code ว่า **`ERR_CONNECTION_REFUSED`** ครับ
     - **คำอธิบายสาเหตุ:** เพราะว่า Kestrel มันเป็น In-Process Web Server ที่รันอยู่ในตัวโปรเซสของโปรแกรมเลย พอเรากด `Ctrl + C` ก็คือการสั่งหยุดโปรแกรม ทำให้ตัวเว็บเซิร์ฟเวอร์ปิดตัวลงและพอร์ต 5000 ก็ปิดไปด้วย พอเราไปกด Refresh เบราว์เซอร์มันพยายามส่ง HTTP Request ไปหา `localhost:5000` เหมือนเดิม แต่ตอนนี้ไม่มีโปรแกรมอะไรรอรับอยู่ที่พอร์ตนี้แล้ว ระบบปฏิบัติการก็เลยปฏิเสธการเชื่อมต่อกลับมาครับ

---

### กิจกรรมที่ 2: การแก้ไขข้อความทักทายใน Program.cs
แก้ไขใน `Program.cs` ให้แสดงชื่อของตนเอง:
```csharp
app.MapGet("/", () => "Welcome to IoT Edge Gateway by Chanin Khamwongsa (67030281)");
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
**ข้อสังเกต:** .NET ทำการแปลง (Serialize) Anonymous Object ให้เป็น JSON String พร้อมตั้งค่า Header `Content-Type: application/json; charset=utf-8` ให้อัตโนมัติ

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
- **ตัวแปร `builder` (`WebApplicationBuilder`):** พูดง่ายๆ คือเอาไว้เตรียมของและตั้งค่าคอนฟิกต่างๆ ก่อนเริ่มรันแอปครับ เช่น ดึงค่าคอนฟิกจากไฟล์ `appsettings.json` หรือพวก Environment และใช้ลงทะเบียนพวก Services เข้าไปใน DI Container (เช่น ใส่ `builder.Services.AddSingleton<...>()` หรือ `AddHostedService<...>()`) เพื่อเตรียมไว้ให้ส่วนอื่นเรียกใช้ตอนโปรแกรมทำงานครับ
- **ตัวแปร `app` (`WebApplication`):** เป็นตัวแอปพลิเคชันที่สร้างเสร็จแล้วหลังจากที่เราสั่ง `builder.Build()` ครับ ตัวนี้จะเอาไว้คุมการทำงานจริง เช่น ตั้งค่า Middleware (อย่างพวก `app.UseStaticFiles()` เพื่อให้เปิดไฟล์หน้าเว็บได้), ใช้แมป Routing ของ API ต่างๆ ว่าถ้าเปิด Path นี้เข้ามาต้องทำอะไรต่อ (`app.MapGet()`, `app.MapPost()`) แล้วสุดท้ายก็ใช้สั่ง `app.Run()` เพื่อเริ่มรันเซิร์ฟเวอร์เปิดพอร์ตรับ Request ครับ

---

### ข้อ 2: เปรียบเทียบความสะดวกระหว่างการสร้าง Web Server บน .NET Minimal API กับการรันผ่าน LAMP Stack (Apache + PHP) ว่ามีข้อดีข้อเสียต่างกันอย่างไรในมุมมองของงาน IoT Gateway
**คำตอบ:**
- **ฝั่ง .NET Minimal API (Kestrel):**
  - **ข้อดี:** สะดวกมากตรงที่ตัว Web Server มันฝังมาในโปรแกรมเราเลย (In-Process Self-Hosted) ทำให้เราสามารถเขียน `BackgroundService` เพื่อเปิดพอร์ต Serial ค้างไว้ในหน่วยความจำได้ตลอดเวลา พอมีข้อมูลจาก ESP32 วิ่งเข้ามาทางสาย USB ก็เก็บไว้ในแรม แล้วให้ตัว API ดึงไปส่งให้หน้าเว็บดูได้ทันทีแบบ real-time ไม่ต้องคอยเขียนลงฐานข้อมูลก่อน ทำงานเร็วมาก ไม่กินสเปกเครื่อง เหมาะกับการเอามาทำพวก Edge IoT Gateway ที่สุดครับ
  - **ข้อเสีย:** ต้องคอมไพล์โค้ดก่อนนำไปรัน และเครื่องปลายทางต้องมี .NET Runtime ติดตั้งอยู่ครับ
- **ฝั่ง LAMP Stack (Apache + PHP):**
  - **ข้อดี:** เหมาะสำหรับการทำเว็บไซต์แบบดั้งเดิมหรือพวกเว็บบอร์ด เว็บ CMS ทั่วไปที่มีหน้าเว็บเยอะๆ เพราะติดตั้งง่ายและมีเครื่องมือจัดการฐานข้อมูลพร้อมใช้งานใน Linux ครับ
  - **ข้อเสีย:** ถ้าเอามาทำเป็น IoT Gateway จะยุ่งยากมากครับ เพราะธรรมชาติของ PHP มันทำงานแบบ Request-Response จบแต่ละครั้งก็เคลียร์หน่วยความจำทิ้ง (Stateless) มันไม่สามารถเปิด Serial Port ค้างไว้ในแรมเพื่อรอรับค่าตลอดเวลาได้ ถ้าจะทำจริงๆ ต้องไปเขียน service ตัวอื่นแยกต่างหากมารับค่าจากบอร์ดลง database แล้วค่อยให้ PHP ไปคิวรีมาตอบอีกที ทำให้ทำงานช้าและกินทรัพยากรเครื่องมากกว่าเยอะครับ

---

### ข้อ 3: นักศึกษาคิดว่าการเพิ่ม `/api/` เข้าไปใน route นั้นมีประโยชน์อย่างไรบ้าง ถ้าไม่ใส่จะเกิดปัญหาอะไรบ้าง
**คำตอบ:**
- **ประโยชน์ของการใส่ `/api/`:**
  1. **แยกแยะง่าย:** ช่วยให้แบ่งแยกชัดเจนตั้งแต่เห็น URL เลยครับว่า เส้นทางไหนเป็นการขอข้อมูลดิบ (พวก JSON API) และเส้นทางไหนคือการขอเปิดไฟล์หน้าเว็บทั่วไป (เช่น HTML, CSS, JavaScript หรือรูปภาพ)
  2. **คอนฟิก Proxy ง่าย:** เวลาเอาไปต่อหลัง Reverse Proxy (อย่าง Nginx) จะตั้งค่าง่ายมากครับ แค่เขียนกฎบอกว่าถ้า URL ขึ้นต้นด้วย `/api/*` ให้โยนไปหาหลังบ้าน ส่วนถ้าไม่มีก็ให้ไปดึงไฟล์หน้าเว็บปกติ
  3. **จัดการเรื่องความปลอดภัยสะดวก:** เราสามารถเขียน Middleware กรองเฉพาะกลุ่ม `/api/*` ได้ง่ายขึ้น เช่น การเช็ค API Key, การทำ Rate Limiting หรือการกำหนดสิทธิ์ CORS
- **ปัญหาถ้าไม่ยอมใส่ `/api/`:**
  1. **ชื่อ Route ชนกัน (Route Collision):** ชื่อ API อาจจะไปตรงกับชื่อไฟล์หรือโฟลเดอร์หน้าเว็บได้ครับ เช่น ถ้าเรามีหน้าเว็บชื่อ `status.html` หรือมีโฟลเดอร์ `led` อยู่ แล้วเราดันไปตั้ง API ว่า `/status` หรือ `/led` ตัวเซิร์ฟเวอร์มันอาจจะงงว่าจะเปิดไฟล์ให้ดูหรือจะรันคำสั่งของ API
  2. **ปัญหากับเว็บแบบ SPA:** ในเว็บพวก React หรือ Vue ที่ใช้ Client-side Routing ถ้าไม่มี `/api/` นำหน้า เซิร์ฟเวอร์อาจจะเข้าใจผิดแล้วส่งหน้า `index.html` กลับมาแทนที่จะคืนค่าข้อมูล JSON ทำให้หน้าเว็บเพี้ยนและทำงานต่อไม่ได้ครับ

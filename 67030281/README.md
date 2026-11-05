# เอกสารสรุปใบงานคำตอบประจำสัปดาห์ที่ 8 (Week 08 Lab Submission)
## Kestrel Web Server & Minimal API Fundamentals

---

### 👤 ข้อมูลประจำตัวนักศึกษา
- **รหัสนักศึกษา:** `67030281`
- **ชื่อ-นามสกุล:** นายชนินทร์ คำวงศ์ษา (Chanin Khamwongsa)
- **คณะ:** คณะวิศวกรรมศาสตร์
- **สาขาวิชา:** วิศวกรรมคอมพิวเตอร์ (Computer Engineering)
- **วิชา:** Application of Internet of Things (Week 08)

---

## 📑 รายการใบงานคำตอบ (Labsheets) ในโฟลเดอร์ `67030281`

ในโฟลเดอร์นี้รวบรวมเฉพาะใบงานที่มีการตอบคำถาม (Checkpoints, Review Questions และ Micro-Challenges) ครบถ้วน 100%:

| ลำดับใบงาน | ชื่อเอกสารใบงาน (Labsheet) | หัวข้อการทดลอง | สถานะคำตอบ |
| :---: | :--- | :--- | :---: |
| **Lab 8.1** | [06-Labsheet-08-1-Kestrel-Minimal-API-Fundamentals.md](06-Labsheet-08-1-Kestrel-Minimal-API-Fundamentals.md) | พื้นฐาน Kestrel Web Server, Minimal API, JSON Serialization, Checkpoint 1.1, คำถามท้ายบท 3 ข้อ และ Micro-Challenge `/api/student` | ✅ ตอบครบถ้วน 100% |
| **Lab 8.2** | [07-Labsheet-08-2-ESP32-ADC-and-Serial-Stream.md](07-Labsheet-08-2-ESP32-ADC-and-Serial-Stream.md) | การสตรีมสัญญาณ ADC 12-bit (GPIO 34) ออก UART @ 115200 bps ด้วย ESP-IDF v6.x, ฟิลเตอร์ EMA และ Checkpoint 2.1 | ✅ ตอบครบถ้วน 100% |
| **Lab 8.3** | [08-Labsheet-08-3-Hardware-Serial-Bridge-to-Web.md](08-Labsheet-08-3-Hardware-Serial-Bridge-to-Web.md) | สะพานเชื่อม USB Serial Bridge สู่ Kestrel ด้วย `BackgroundService`, Thread-Safe State, Checkpoint 3.1 และ Micro-Challenge `alertLevel` | ✅ ตอบครบถ้วน 100% |

> 📌 **หมายเหตุ:** ใบงาน Lab 8-4 และ Lab 8-5 ที่มีข้อกำหนดการบันทึกคลิปวิดีโอ ได้ถูกนำออกจากโฟลเดอร์คำตอบ `67030281` นี้ตามคำสั่ง แต่ตัวซอร์สโค้ดและใบงานต้นฉบับยังคงถูกจัดเก็บรักษาไว้ที่โฟลเดอร์หลัก (Root Workspace) ของโปรเจกต์อย่างสมบูรณ์

---

## 🌐 สรุปตาราง REST API Endpoints ของระบบ

| โปรเจกต์ | Method | Path | คำอธิบายและข้อมูลที่คืนค่า |
| :--- | :---: | :--- | :--- |
| **Lab 8-1** | `GET` | `/` | ข้อความต้อนรับระบุชื่อนักศึกษาและรหัส `67030281` |
| **Lab 8-1** | `GET` | `/api/status` | สถานะของ Gateway, Uptime และ Health check |
| **Lab 8-1** | `GET` | `/api/led/{state}` | รับพารามิเตอร์ `on`/`off` สั่งงานอุปกรณ์จำลองและแสดงผลบน Console |
| **Lab 8-1** | `GET` | `/api/student` | **(Micro-Challenge)** ข้อมูลประจำตัวนักศึกษา ชนินทร์ คำวงศ์ษา รหัส `67030281` |
| **Lab 8-3** | `GET` | `/api/telemetry` | ข้อมูลโทรมาตรเซนเซอร์ (`rawValue`, `voltage`, `percentage`, `alertLevel`, `dataSource`) |

---

## 💡 สรุปคำตอบสำคัญในใบงาน

### 1. Lab 8.1 - พื้นฐาน Kestrel Web Server
- **Checkpoint 1.1:** เมื่อกด `Ctrl + C` แล้วกด Refresh หน้าเว็บจะขึ้น `ERR_CONNECTION_REFUSED` เพราะ Kestrel เป็น In-Process Server เมื่อโปรเซสหยุดทำงาน TCP Socket Listener จะปิดตัวลง ทำให้ไม่มีเซอร์วิสรับคำขอ HTTP
- **คำถามข้อ 1 (builder vs app):**
  - `builder`: จัดการ Configuration, โหลดการตั้งค่า และลงทะเบียน Service ใน DI Container
  - `app`: จัดการ Middleware Pipeline, Routing Endpoint (`MapGet`) และสั่งรันเซิร์ฟเวอร์เปิดรับฟัง Request (`Run`)
- **คำถามข้อ 2 (Minimal API vs LAMP):**
  - Minimal API บน Kestrel เป็น Single Binary ทำงานแบบ Non-blocking I/O ประสิทธิภาพสูงมาก สามารถรัน `BackgroundService` ถือพอร์ต Serial และแชร์ Thread-Safe State ใน RAM ได้โดยตรง
  - PHP บน LAMP มีรอบการทำงานแบบ Stateless ตัดจบในแต่ละคำขอ ไม่สามารถเปิด Serial Port ค้างไว้ในหน่วยความจำได้โดยตรง
- **คำถามข้อ 3 (ประโยชน์ของ `/api/`):** แยกแยะ API Service ออกจาก Static Files ป้องกัน Route Collision และช่วยให้ Reverse Proxy / Load Balancer กรองเส้นทางได้ง่าย
- **Micro-Challenge `/api/student`:** คืนค่า JSON ระบุรหัส `67030281`, ชื่อ `Chanin Khamwongsa`, คณะวิศวกรรมศาสตร์, เซนเซอร์ที่สนใจ และ Timestamp

### 2. Lab 8.2 - ESP32 ADC & Serial Stream
- **Checkpoint 2.1 (ข้อ 1):** ฟังก์ชัน `adc_oneshot_read()` คืนค่าเป็นตัวเลขจำนวนเต็ม 12 บิต ในช่วง **`0 ถึง 4095`** ($2^{12} - 1$)
- **Checkpoint 2.1 (ข้อ 2):** ต้องกด `Ctrl + ]` ออกจาก `idf.py monitor` เพื่อปลดปล่อย Handle ของพอร์ต COM คืนแก่ระบบปฏิบัติการ มิฉะนั้นโปรแกรม C# Kestrel จะเปิดพอร์ตไม่ได้เนื่องจากติดสถานะ Exclusive Access
- **Micro-Challenge LDR:** วงจรแบ่งแรงดัน (Voltage Divider) เมื่อส่องไฟ ความต้านทาน LDR ลดลง แรงดันเพิ่มขึ้น ค่า ADC พุ่งสูงขึ้นเข้าใกล้ 4095 เมื่อบังแสง ค่า ADC ลดต่ำลงเข้าใกล้ 0

### 3. Lab 8.3 - Hardware Serial Bridge
- **Checkpoint 3.1:** เมื่อถอดสาย USB ค่า `dataSource` เปลี่ยนเป็น `"Simulation Mode (Sine Wave)"` อัตโนมัติ โดยค่า `rawValue` ยังคงแกว่งตัวขึ้น-ลงตามคลื่นไซน์อย่างต่อเนื่อง (Fault-Tolerant Fallback)
- **Micro-Challenge `alertLevel`:** เพิ่มฟิลด์ตรวจสอบระดับเตือนภัย:
  - $\text{percentage} > 85.0\% \rightarrow \textbf{"DANGER (HIGH)"}$
  - $70.0\% \le \text{percentage} \le 85.0\% \rightarrow \textbf{"WARNING"}$
  - $\text{percentage} < 70.0\% \rightarrow \textbf{"NORMAL"}$

---

## 🚀 วิธีการคอมไพล์และทดสอบรัน

### การรัน Lab 8-1 (Minimal API Fundamentals)
```powershell
cd Lab8-1
dotnet run
# เปิดเบราว์เซอร์ที่: http://localhost:5000/api/student
```

### การรัน Lab 8-2 (ESP32 ADC Stream)
```powershell
cd Lab8-2/ESP32_ADC_Stream
# คอมไพล์และแฟลชลง ESP32
idf.py -p COM24 flash monitor
# เมื่อตรวจสอบเสร็จสิ้น ให้กด Ctrl + ] เพื่อปลดล็อกพอร์ต COM
```

### การรัน Lab 8-3 (Hardware Serial Bridge to Web)
```powershell
cd Lab8-3/Kestrel_Serial_Gateway
dotnet run
# เปิดเบราว์เซอร์ที่: http://localhost:5000/api/telemetry
```

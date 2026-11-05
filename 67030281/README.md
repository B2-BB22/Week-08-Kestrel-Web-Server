# สรุปการส่งงาน Week 08: Kestrel Web Server & Minimal API

### ข้อมูลนักศึกษา
- **รหัสนักศึกษา:** 67030281
- **ชื่อ-นามสกุล:** นายชนินทร์ คำวงศ์ษา
- **วิชา:** Application of Internet of Things (สัปดาห์ที่ 8)

---

## รายการใบงานที่ส่งในโฟลเดอร์ `67030281`

| ใบงาน | ไฟล์เอกสารใบงาน | เนื้อหาการทดลอง | สถานะ |
| :---: | :--- | :--- | :---: |
| **Lab 8.1** | [06-Labsheet-08-1-Kestrel-Minimal-API-Fundamentals.md](06-Labsheet-08-1-Kestrel-Minimal-API-Fundamentals.md) | พื้นฐาน Kestrel Web Server, Minimal API, JSON Serialization, Checkpoint 1.1, Review Questions และ Micro-Challenge `/api/student` | เรียบร้อย |
| **Lab 8.2** | [07-Labsheet-08-2-ESP32-ADC-and-Serial-Stream.md](07-Labsheet-08-2-ESP32-ADC-and-Serial-Stream.md) | ESP32 ADC 12-bit (GPIO 34), สตรีมข้อมูล UART 115200 bps ด้วย ESP-IDF, EMA Filter และ Checkpoint 2.1 | เรียบร้อย |
| **Lab 8.3** | [08-Labsheet-08-3-Hardware-Serial-Bridge-to-Web.md](08-Labsheet-08-3-Hardware-Serial-Bridge-to-Web.md) | Serial Bridge ด้วย BackgroundService บน Kestrel, Thread-Safe State, Checkpoint 3.1 และ Micro-Challenge `alertLevel` | เรียบร้อย |

---

## สรุป REST API Endpoints ของระบบ

| โปรเจกต์ | Method | Path | การทำงาน / ข้อมูลที่คืนค่า |
| :--- | :---: | :--- | :--- |
| **Lab 8-1** | `GET` | `/` | ข้อความต้อนรับระบุชื่อนักศึกษาและรหัส 67030281 |
| **Lab 8-1** | `GET` | `/api/status` | ข้อมูลสถานะ Gateway, Uptime และ Health check |
| **Lab 8-1** | `GET` | `/api/led/{state}` | รับพารามิเตอร์ `on` หรือ `off` จำลองการสั่งงานอุปกรณ์ |
| **Lab 8-1** | `GET` | `/api/student` | (Micro-Challenge) ข้อมูลนักศึกษาและเซนเซอร์ที่สนใจ |
| **Lab 8-3** | `GET` | `/api/telemetry` | ข้อมูลเซนเซอร์ล่าสุด (`rawValue`, `voltage`, `percentage`, `alertLevel`, `dataSource`) |

---

## สรุปคำตอบ Checkpoint และ Challenge

### Lab 8.1
- **Checkpoint 1.1:** พอเรากด `Ctrl + C` แล้วไปกด Refresh หน้าเว็บจะขึ้น `ERR_CONNECTION_REFUSED` เพราะ Kestrel มันรันอยู่ในตัวโปรแกรมเลย พอสั่งหยุด โปรเซสก็ดับและพอร์ต 5000 ก็ปิดตามไปด้วย ทำให้ไม่มีเซิร์ฟเวอร์มารับ Request ครับ
- **คำถามข้อ 1 (`builder` vs `app`):**
  - `builder`: ใช้เตรียมของและตั้งค่าคอนฟิกต่างๆ ก่อนเริ่ม เช่น โหลดไฟล์การตั้งค่า และลงทะเบียน Services เข้า DI Container
  - `app`: ตัวเว็บแอปที่สร้างเสร็จแล้ว เอาไว้คุม Middleware, กำหนดพวก Route Endpoint (`MapGet`) และสั่งรันเซิร์ฟเวอร์ด้วย `Run()`
- **คำถามข้อ 2 (Minimal API vs LAMP):**
  - Minimal API บน Kestrel เป็นโปรแกรมเดียวจบ สามารถรัน BackgroundService เปิดพอร์ต Serial ค้างไว้ในแรมแล้วส่งให้ API ได้ทันทีแบบ real-time ทำงานเร็วและไม่กินสเปกเครื่อง
  - LAMP (PHP) ทำงานแบบขอมาตอบไปแล้วเคลียร์แรมทิ้ง (Stateless) ทำให้เปิดพอร์ต Serial ค้างไว้ในแรมไม่ได้ ถ้าจะทำต้องเขียนระบบอื่นมารับค่าช่วยซึ่งยุ่งยากกว่ามาก
- **คำถามข้อ 3 (ประโยชน์ของ `/api/`):** ช่วยแยกทางให้ชัดเจนระหว่าง API ข้อมูลดิบกับหน้าเว็บทั่วไป ป้องกันชื่อไปชนกับไฟล์เว็บ และช่วยให้ตั้งค่า Reverse Proxy ได้ง่ายครับ
- **Micro-Challenge `/api/student`:** คืนค่า JSON รหัส 67030281, ชื่อ Chanin Khamwongsa, คณะวิศวกรรมศาสตร์, เซนเซอร์ที่สนใจ และเวลาเซิร์ฟเวอร์

### Lab 8.2
- **Checkpoint 2.1 (ข้อ 1):** ฟังก์ชัน `adc_oneshot_read()` คืนค่าเป็นตัวเลขจำนวนเต็ม (int) ในช่วง 0 ถึง 4095 ตามความละเอียด 12 บิต
- **Checkpoint 2.1 (ข้อ 2):** ต้องกด `Ctrl + ]` ออกจากตัว monitor ก่อน เพื่อปล่อยพอร์ต COM คืนให้ระบบ ไม่อย่างนั้นโปรแกรม C# Kestrel จะเปิดพอร์ตไม่ได้เพราะพอร์ตถูกล็อกไว้ (Exclusive Access)
- **Micro-Challenge LDR:** เมื่อต่อวงจรแบ่งแรงดัน ตอนเอาไฟส่อง ความต้านทาน LDR จะลดลง ทำให้แรงดันสูงขึ้น ค่า ADC พุ่งไปใกล้ 4095 พอเอามือบังแสง ความต้านทานจะสูงขึ้น ทำให้แรงดันลดลง ค่า ADC ดิ่งลงมาใกล้ 0

### Lab 8.3
- **Checkpoint 3.1:** พอถอดสาย USB ค่า `dataSource` จะสลับไปเป็น `"Simulation Mode (Sine Wave)"` ทันที และค่า `rawValue` จะแกว่งขึ้นลงตามลูกคลื่นไซน์เรื่อยๆ อัตโนมัติ เพื่อจำลองข้อมูลเซนเซอร์ทำให้ระบบไม่ล่มแม้ไม่มีฮาร์ดแวร์ต่ออยู่
- **Micro-Challenge `alertLevel`:** เพิ่มฟิลด์ตรวจสอบระดับแจ้งเตือน:
  - percentage > 85.0% -> "DANGER (HIGH)"
  - 70.0% ถึง 85.0% -> "WARNING"
  - ต่ำกว่า 70.0% -> "NORMAL"

---

## ขั้นตอนการทดสอบรันแต่ละแล็บ

### Lab 8-1
```powershell
cd Lab8-1
dotnet run
# เข้าทดสอบที่: http://localhost:5000/api/student
```

### Lab 8-2
```powershell
cd Lab8-2/ESP32_ADC_Stream
idf.py -p COM24 flash monitor
# เมื่อดูผลเสร็จ ให้กด Ctrl + ] เพื่อปลดล็อกพอร์ต COM
```

### Lab 8-3
```powershell
cd Lab8-3/Kestrel_Serial_Gateway
dotnet run
# เข้าทดสอบที่: http://localhost:5000/api/telemetry
```

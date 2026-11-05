# ใบงานการทดลองที่ 8.2 (Labsheet 8.2) - ฉบับบันทึกผลและตอบคำถาม
### การสตรีมสัญญาณแอนะล็อก ESP32 ออกพอร์ตสื่อสารอนุกรม (UART)

**รหัสนักศึกษา:** 67030281  
**ชื่อ-นามสกุล:** นายชนินทร์ คำวงศ์ษา 

---

## วัตถุประสงค์การทดลอง (Objectives)
1. สามารถต่อวงจรตัวต้านทานปรับค่าได้ (Potentiometer) เข้ากับขาอินพุตแอนะล็อก (ADC) ของ ESP32 ได้อย่างถูกต้องและปลอดภัย
2. เข้าใจข้อจำกัดของวงจรแปลงสัญญาณแอนะล็อกเป็นดิจิทัล (ADC1 vs ADC2) บนบอร์ด ESP32
3. สามารถเขียนโปรแกรมภาษา C บน ESP-IDF v6.x เพื่ออ่านค่าและกรองสัญญาณรบกวน (Signal Smoothing ด้วย EMA Filter) ได้
4. สามารถสตรีมข้อมูลเซนเซอร์ออกทางพอร์ตสื่อสารอนุกรม (Serial Port @ 115200 bps) เพื่อเตรียมป้อนให้แก่ Web Server ได้

---

## อุปกรณ์และการต่อวงจร (Hardware Schematic & Wiring)

### 1. แผนผังการต่อสาย
```text
        ESP32 Board                  Potentiometer (10k)
      +--------------+                 +-------------+
      |         3.3V |-----------------| ขา 1 (VCC)  |
      |          GND |-----------------| ขา 3 (GND)  |
      |      GPIO 34 |-----------------| ขา 2 (Wiper)| (ขากลางรับแรงดัน 0 - 3.3V)
      +--------------+                 +-------------+
```

### 2. ข้อควรระวังและเหตุผลทางวิศวกรรม
1. **ระดับแรงดันไฟฟ้า (Voltage Rating):** ต้องต่อขา VCC ของ Potentiometer เข้ากับขา **3.3V** เท่านั้น ห้ามต่อกับไฟ 5V (VIN) โดยเด็ดขาด เนื่องจากขา GPIO ของ ESP32 ทนแรงดันอินพุตสูงสุดไม่เกิน $3.6\text{ V}$ หากป้อน $5\text{ V}$ จะทำให้วงจร ADC ภายในชิปไหม้และเสียหายถาวร
2. **การเลือกใช้ขา GPIO 34 (ADC1):**
   - บนบอร์ด ESP32 แบ่ง ADC ออกเป็น 2 คอนโทรลเลอร์ คือ **ADC1** (GPIO 32-39) และ **ADC2** (GPIO 0, 2, 4, 12-15, 25-27)
   - โมดูล **ADC2 ถูกแชร์ทรัพยากรภายในร่วมกับระบบ Wi-Fi Driver** ดังนั้นเมื่อมีการเปิดใช้งาน Wi-Fi (เช่น ในการสื่อสารเครือข่าย หรือ MQTT) ระบบ Wi-Fi จะยึดสิทธิ์การใช้ ADC2 ทำให้ไม่สามารถอ่านค่าแอนะล็อกได้
   - ดังนั้น การเลือกใช้ **GPIO 34 ซึ่งอยู่ในกลุ่ม ADC1 (ADC_UNIT_1, ADC_CHANNEL_6)** จึงเป็นมาตรฐานที่ปลอดภัยที่สุด สามารถทำงานได้อย่างต่อเนื่องแม้เปิดใช้งาน Wi-Fi และ Bluetooth พร้อมกัน

---

## ซอร์สโค้ดเฟิร์มแวร์ ESP-IDF v6.x (`main/ESP32_ADC_Stream.c`)

```c
// ============================================================================
// ใบงานที่ 8.2: ESP32 Potentiometer Serial Stream with EMA Filter (ESP-IDF v6.x)
// นักศึกษา: นายชนินทร์ คำวงศ์ษา | รหัส 67030281
// ============================================================================
#include <stdio.h>
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "esp_log.h"
#include "esp_adc/adc_oneshot.h"

#if CONFIG_IDF_TARGET_ESP32C6
#define POT_ADC_CHANNEL    ADC_CHANNEL_4   // GPIO 4 บน ESP32-C6
#else
#define POT_ADC_CHANNEL    ADC_CHANNEL_6   // GPIO 34 บน ESP32 WROOM
#endif

void app_main(void)
{
    printf("\n[SYSTEM] ESP-IDF v6.x Potentiometer Stream Starting...\n");

    // 1. สร้างและตั้งค่า ADC Unit 1
    adc_oneshot_unit_handle_t adc1_handle;
    adc_oneshot_unit_init_cfg_t init_config1 = {
        .unit_id = ADC_UNIT_1,
        .ulp_mode = ADC_ULP_MODE_DISABLE,
    };
    ESP_ERROR_CHECK(adc_oneshot_new_unit(&init_config1, &adc1_handle));

    // 2. กำหนดค่าความละเอียด 12-bit (0-4095) และอัตราขยายสัญญาณ (Attenuation 12dB สำหรับ 0-3.3V)
    adc_oneshot_chan_cfg_t config = {
        .bitwidth = ADC_BITWIDTH_12,
        .atten = ADC_ATTEN_DB_12,
    };
    ESP_ERROR_CHECK(adc_oneshot_config_channel(adc1_handle, POT_ADC_CHANNEL, &config));

    printf("[SYSTEM] ADC Initialized with EMA Filter. Streaming values @ 115200 bps...\n");

    int raw_val = 0;
    float filtered_val = 0.0f;
    const float alpha = 0.25f; // ค่าสัมประสิทธิ์การกรอง Exponential Moving Average

    while (1) {
        // อ่านค่าแอนะล็อกแบบ Oneshot
        ESP_ERROR_CHECK(adc_oneshot_read(adc1_handle, POT_ADC_CHANNEL, &raw_val));

        // การกรองแบบ Exponential Moving Average (EMA) เพื่อลดสัญญาณรบกวน (Noise Reduction)
        filtered_val = (alpha * raw_val) + ((1.0f - alpha) * filtered_val);

        // สตรีมค่าตัวเลขออกทาง UART (stdout) บรรทัดละ 1 ค่า เพื่อให้ฝั่ง C# อ่านง่าย
        printf("%d\n", (int)filtered_val);

        // สตรีมที่ความถี่ 20 Hz (ทุก 50ms)
        vTaskDelay(pdMS_TO_TICKS(50));
    }
}
```

---

## ผลการทดสอบการหมุน Potentiometer
- **เมื่อหมุน Potentiometer ทวนเข็มนาฬิกาสุด (ซ้ายสุด):** ค่าแรงดันที่ขา Wiper เข้าใกล้ $0\text{ V}$ ค่า ADC แสดงผลเป็น `0 - 15`
- **เมื่อหมุน Potentiometer ไว้ตรงกลาง:** ค่าแรงดันที่ขา Wiper ประมาณ $1.65\text{ V}$ ค่า ADC แสดงผลประมาณ `2048`
- **เมื่อหมุน Potentiometer ตามเข็มนาฬิกาสุด (ขวาสุด):** ค่าแรงดันที่ขา Wiper เข้าใกล้ $3.3\text{ V}$ ค่า ADC แสดงผลเป็น `4080 - 4095`
- **เปรียบเทียบก่อนและหลังใส่ EMA Filter ($\alpha = 0.25$):**  
  ก่อนใส่ฟิลเตอร์ ตัวเลข ADC มีอาการแกว่งตัว $\pm 10\text{ - }25\text{ LSB}$ จากสัญญาณรบกวนและคลื่นแม่เหล็กไฟฟ้า แต่เมื่อใส่ตัวกรอง EMA แล้ว ตัวเลขมีความนิ่ง เสถียร และลื่นไหลอย่างเห็นได้ชัด

---

## [Checkpoint 2.1 ทดสอบความเข้าใจ]

### 1. ใน ESP-IDF v6.x ฟังก์ชัน `adc_oneshot_read()` คืนค่าผลลัพธ์เป็นตัวเลขแบบใด และมีช่วงค่าต่ำสุด-สูงสุดเท่าใดเมื่อตั้งค่าเป็น `ADC_BITWIDTH_12`
**คำตอบ:**  
- **ชนิดของตัวเลข:** คืนค่าผลลัพธ์เป็นตัวเลขจำนวนเต็มแบบมีเครื่องหมาย (Integer ผ่านพอยน์เตอร์ `int *out_raw`) 
- **ช่วงค่าต่ำสุด - สูงสุด:** มีช่วงค่าตั้งแต่ **`0` ถึง `4095`**
  - เนื่องจากความละเอียด 12 บิต สามารถแทนจำนวนระดับสัญญาณดิจิทัลได้ทั้งหมด $2^{12} = 4096$ ระดับ (ระดับขั้นตั้งแต่ $0$ ถึง $4096 - 1 = 4095$)
  - โดยค่า `0` แทนแรงดัน $0\text{ V}$ และค่า `4095` แทนแรงดันเต็มสเกล (Full Scale Voltage ประมาณ $3.3\text{ V}$ เมื่อตั้ง Attenuation เป็น `ADC_ATTEN_DB_12`)

---

### 2. ทำไมเราจึงต้องกดปุ่ม `Ctrl + ]` เพื่อปิด ESP-IDF Monitor ก่อนที่เราจะรันโปรแกรมฝั่ง C# Kestrel ในใบงานถัดไป
*(คำใบ้: หากโปรเซส idf.py monitor ยังเปิดค้างอยู่ จะเกิดอะไรขึ้นกับพอร์ต COM)*  
**คำตอบ:**  
- เนื่องจากพอร์ตสื่อสารอนุกรม (Serial Virtual COM Port) ในระบบปฏิบัติการ (Windows, Linux, macOS) จัดเป็นทรัพยากรฮาร์ดแวร์แบบ **Exclusive Resource (การเข้าถึงแบบเดี่ยวพิเศษ)** ซึ่งอนุญาตให้มีเพียงโปรเซสเดียวเท่านั้นที่สามารถเรียกเปิดพอร์ต (Open File Handle) และดักรับส่งข้อมูลได้ในเวลาเดียวกัน
- หากคำสั่ง `idf.py monitor` (หรือโปรแกรม Serial Monitor อื่นๆ) ยังคงเปิดค้างอยู่ โปรเซสดังกล่าวจะล็อกพอร์ต COM นั้นไว้ เมื่อเราสั่งรันโปรแกรม C# Kestrel Web Server ที่พยายามจะเรียกคำสั่ง `serial.Open()` บนพอร์ตเดียวกัน ระบบปฏิบัติการจะโยนข้อผิดพลาด **`UnauthorizedAccessException: Access to the port is denied`**
- ส่งผลให้โปรแกรม C# ไม่สามารถเชื่อมต่อกับฮาร์ดแวร์ ESP32 ได้ และระบบ Kestrel จะต้องตกหล่นเข้าไปทำงานในโหมดจำลอง (Fallback Simulation Mode) แทน ดังนั้นการกด `Ctrl + ]` จึงเป็นการส่งคำสั่งให้ ESP-IDF Monitor ปิดและปลดปล่อย Handle ของพอร์ต COM คืนแก่ระบบ เพื่อให้โปรแกรม C# Kestrel สามารถเข้ามาเชื่อมต่อได้สำเร็จ

---

## ภารกิจท้าทาย (Micro-Challenge) การประยุกต์ใช้วงจร LDR (Light Dependent Resistor)

### 1. แผนภาพวงจรแบ่งแรงดัน (Voltage Divider Circuit)
```text
        +3.3V
          |
         [ ] LDR (ความต้านทานเปลี่ยนตามความเข้มแสง)
          |
          +------> เข้าขา GPIO 34 (ADC1_CH6)
          |
         [ ] R1 (ตัวต้านทานคงที่ 10 kΩ)
          |
         GND
```

### 2. การคำนวณแรงดันไฟฟ้าที่ตกคร่อม
$$V_{\text{out}} = V_{\text{CC}} \times \frac{R_1}{R_{\text{LDR}} + R_1} = 3.3\text{ V} \times \frac{10\text{ k}\Omega}{R_{\text{LDR}} + 10\text{ k}\Omega}$$

### 3. ผลการทดสอบพฤติกรรมของสัญญาณ
1. **สภาวะแสงห้องปกติ (Ambient Light):**  
   ความต้านทาน $R_{\text{LDR}} \approx 8\text{ - }15\text{ k}\Omega$ แรงดัน $V_{\text{out}} \approx 1.3\text{ - }1.8\text{ V}$ ค่า ADC แสดงผลอยู่ในช่วง **`1600 - 2200`**
2. **สภาวะฉายไฟฉายส่องตรงไปที่ตัว LDR (Bright Light):**  
   ความเข้มแสงสูงทำให้ความต้านทานของ LDR ลดลงอย่างรวดเร็วเหลือเพียงไม่กี่ร้อยโอห์ม ($R_{\text{LDR}} \approx 200\text{ - }500\text{ }\Omega$) ส่งผลให้แรงดันตกคร่อม $R_1$ พุ่งสูงขึ้นเข้าใกล้ $3.3\text{ V}$ ค่า ADC พุ่งสูงขึ้นเป็น **`3800 - 4085`**
3. **สภาวะเอามือบังแสงหรืออยู่ในที่มืดสนิท (Darkness):**  
   ความต้านทานของ LDR พุ่งสูงขึ้นมากเป็นหลักเมกะโอห์ม ($R_{\text{LDR}} > 500\text{ k}\Omega$) ทำให้กระแสไหลผ่านวงจรน้อยมาก แรงดันตกคร่อม $R_1$ ลดลงเข้าใกล้ $0\text{ V}$ ค่า ADC ดิ่งลงมาอยู่ที่ **`150 - 350`**


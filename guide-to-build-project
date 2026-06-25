# 🎟️ Event Ticket Booking - QR Payment Project

## 🎯 Objectives
- พัฒนาระบบจองตั๋วอีเวนต์ขนาดเล็กเพื่อฝึกเชื่อมต่อ QR Code Payment (PromptPay)
- ฝึกใช้งานระบบ Webhook สำหรับเปลี่ยนสถานะคำสั่งซื้อแบบอัตโนมัติ

## 🗃️ Database Schema (3 Tables)
- [ ] **Events**: `id` (PK) | `title` | `price` | `total_seats` | `available_seats`
- [ ] **Orders**: `id` (PK) | `event_id` (FK) | `customer_email` | `quantity` | `total_amount` | `status` (pending, paid, expired) | `charge_id`
- [ ] **Tickets**: `id` (PK) | `order_id` (FK) | `ticket_code`

## 🛠️ API Endpoints Backend
- [ ] `POST /api/orders` : รับข้อมูลการจอง -> สร้าง Order (Pending) -> ยิงขอ QR จาก Gateway -> ส่ง QR กลับไปหน้าบ้าน
- [ ] `GET /api/orders/:id` : หน้าบ้านยิงเช็กสถานะออเดอร์ (Polling ทุกๆ 3 วินาที)
- [ ] `POST /api/webhook` : รับข้อมูลยืนยันการจ่ายเงินจาก Gateway -> อัปเดตตาราง Orders เป็น paid -> สร้างตั๋วลงตาราง Tickets

## 💻 Frontend Pages & Features
- [ ] **Page 1: Booking Form** - เลือกจำนวนตั๋ว, กรอกอีเมล, คำนวณราคารวม, ปุ่มกดยืนยัน
- [ ] **Page 2: Checkout Screen** - แสดง Dynamic QR Code, สรุปยอดเงิน, นาฬิกานับเวลาถอยหลัง (5 นาที)
- [ ] **Page 3: Success Screen** - แสดงตั๋วจำลองและรหัส Ticket Code หลังจ่ายเงินสำเร็จ

## 🚀 Steps to Complete
- [ ] สมัครบัญชี Sandbox/Test ของ Payment Gateway (เช่น Omise) เพื่อรับ API Key
- [ ] ออกแบบฐานข้อมูลและตั้งค่าเซิร์ฟเวอร์หลังบ้าน
- [ ] เขียน API สื่อสารกับ Gateway และทำระบบสร้าง QR Code
- [ ] เขียนระบบ Webhook และทดสอบผ่านเครื่องมือจำลอง (เช่น ngrok หรือ Simulate tool ของ Gateway)
- [ ] พัฒนาหน้าเว็บ Frontend เชื่อมต่อกับ Backend
- [ ] ทดสอบ Flow การใช้งานจริงตั้งแต่ต้นจนจบ

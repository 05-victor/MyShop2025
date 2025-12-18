-- ============================================
-- MyShop E-Commerce Seed Data Script
-- For PostgreSQL (Supabase)
-- ============================================
-- Run this script directly in Supabase SQL Editor
-- Password hash: $2a$11$d0BwzfQ9JQTqvdGgnuZw.uZlIXdGFxuGrMovH350BtToEfDY9ZlGe
-- ============================================

-- ============================================
-- 1. CATEGORIES - Electronics & Tech Categories
-- ============================================
INSERT INTO categories (id, name, description, created_at, updated_at) VALUES
('a1000000-0000-0000-0000-000000000001', 'Smartphones', 'Mobile phones and smartphones from top brands', NOW(), NOW()),
('a1000000-0000-0000-0000-000000000002', 'Laptops', 'Laptops and notebooks for work and gaming', NOW(), NOW()),
('a1000000-0000-0000-0000-000000000003', 'Tablets', 'Tablets and e-readers for portable computing', NOW(), NOW()),
('a1000000-0000-0000-0000-000000000004', 'Audio', 'Headphones, earbuds, and speakers', NOW(), NOW()),
('a1000000-0000-0000-0000-000000000005', 'Smartwatches', 'Wearable devices and fitness trackers', NOW(), NOW()),
('a1000000-0000-0000-0000-000000000006', 'Accessories', 'Cases, chargers, cables and accessories', NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- ============================================
-- 2. USERS - Admin, Sales Agents, Customers
-- ============================================

-- Admin User
INSERT INTO users (id, username, password, email, created_at, is_trial_active, is_email_verified) VALUES
('b1000000-0000-0000-0000-000000000001', 'admin', '$2a$11$d0BwzfQ9JQTqvdGgnuZw.uZlIXdGFxuGrMovH350BtToEfDY9ZlGe', 'admin@myshop.com', NOW() - INTERVAL '6 months', false, true)
ON CONFLICT (id) DO NOTHING;

-- Sales Agents
INSERT INTO users (id, username, password, email, created_at, is_trial_active, is_email_verified) VALUES
('b2000000-0000-0000-0000-000000000001', 'agent_michael', '$2a$11$d0BwzfQ9JQTqvdGgnuZw.uZlIXdGFxuGrMovH350BtToEfDY9ZlGe', 'michael.chen@myshop.com', NOW() - INTERVAL '5 months', false, true),
('b2000000-0000-0000-0000-000000000002', 'agent_sarah', '$2a$11$d0BwzfQ9JQTqvdGgnuZw.uZlIXdGFxuGrMovH350BtToEfDY9ZlGe', 'sarah.johnson@myshop.com', NOW() - INTERVAL '4 months', false, true),
('b2000000-0000-0000-0000-000000000003', 'agent_david', '$2a$11$d0BwzfQ9JQTqvdGgnuZw.uZlIXdGFxuGrMovH350BtToEfDY9ZlGe', 'david.nguyen@myshop.com', NOW() - INTERVAL '3 months', false, true)
ON CONFLICT (id) DO NOTHING;

-- Customers
INSERT INTO users (id, username, password, email, created_at, is_trial_active, is_email_verified) VALUES
('c1000000-0000-0000-0000-000000000001', 'john_doe', '$2a$11$d0BwzfQ9JQTqvdGgnuZw.uZlIXdGFxuGrMovH350BtToEfDY9ZlGe', 'john.doe@gmail.com', NOW() - INTERVAL '2 months', false, true),
('c1000000-0000-0000-0000-000000000002', 'jane_smith', '$2a$11$d0BwzfQ9JQTqvdGgnuZw.uZlIXdGFxuGrMovH350BtToEfDY9ZlGe', 'jane.smith@gmail.com', NOW() - INTERVAL '2 months', false, true),
('c1000000-0000-0000-0000-000000000003', 'robert_wilson', '$2a$11$d0BwzfQ9JQTqvdGgnuZw.uZlIXdGFxuGrMovH350BtToEfDY9ZlGe', 'robert.wilson@gmail.com', NOW() - INTERVAL '1 month', false, true),
('c1000000-0000-0000-0000-000000000004', 'emily_brown', '$2a$11$d0BwzfQ9JQTqvdGgnuZw.uZlIXdGFxuGrMovH350BtToEfDY9ZlGe', 'emily.brown@gmail.com', NOW() - INTERVAL '3 weeks', false, true),
('c1000000-0000-0000-0000-000000000005', 'alex_tran', '$2a$11$d0BwzfQ9JQTqvdGgnuZw.uZlIXdGFxuGrMovH350BtToEfDY9ZlGe', 'alex.tran@gmail.com', NOW() - INTERVAL '2 weeks', false, true)
ON CONFLICT (id) DO NOTHING;

-- ============================================
-- 3. PROFILES - User profile details
-- ============================================
INSERT INTO profiles (user_id, full_name, phone_number, address, avatar, created_at, updated_at) VALUES
-- Admin
('b1000000-0000-0000-0000-000000000001', 'System Administrator', '0901234567', '123 Admin Street, District 1, HCMC', NULL, NOW(), NOW()),
-- Sales Agents
('b2000000-0000-0000-0000-000000000001', 'Michael Chen', '0912345678', '456 Nguyen Hue, District 1, HCMC', NULL, NOW(), NOW()),
('b2000000-0000-0000-0000-000000000002', 'Sarah Johnson', '0923456789', '789 Le Loi, District 3, HCMC', NULL, NOW(), NOW()),
('b2000000-0000-0000-0000-000000000003', 'David Nguyen', '0934567890', '321 Hai Ba Trung, District 1, HCMC', NULL, NOW(), NOW()),
-- Customers
('c1000000-0000-0000-0000-000000000001', 'John Doe', '0945678901', '111 Pham Ngu Lao, District 1, HCMC', NULL, NOW(), NOW()),
('c1000000-0000-0000-0000-000000000002', 'Jane Smith', '0956789012', '222 Bui Vien, District 1, HCMC', NULL, NOW(), NOW()),
('c1000000-0000-0000-0000-000000000003', 'Robert Wilson', '0967890123', '333 Vo Van Tan, District 3, HCMC', NULL, NOW(), NOW()),
('c1000000-0000-0000-0000-000000000004', 'Emily Brown', '0978901234', '444 Nguyen Thi Minh Khai, District 3, HCMC', NULL, NOW(), NOW()),
('c1000000-0000-0000-0000-000000000005', 'Alex Tran', '0989012345', '555 Cach Mang Thang 8, District 10, HCMC', NULL, NOW(), NOW())
ON CONFLICT (user_id) DO NOTHING;

-- ============================================
-- 4. USER ROLES - Assign roles to users
-- ============================================
-- Admin role
INSERT INTO user_roles (roles_name, users_id) VALUES
('Admin', 'b1000000-0000-0000-0000-000000000001')
ON CONFLICT (roles_name, users_id) DO NOTHING;

-- Sales Agent roles
INSERT INTO user_roles (roles_name, users_id) VALUES
('SalesAgent', 'b2000000-0000-0000-0000-000000000001'),
('SalesAgent', 'b2000000-0000-0000-0000-000000000002'),
('SalesAgent', 'b2000000-0000-0000-0000-000000000003')
ON CONFLICT (roles_name, users_id) DO NOTHING;

-- ============================================
-- 5. PRODUCTS - Electronics & Tech Products
-- ============================================

-- Smartphones (Category: a1000000-0000-0000-0000-000000000001)
INSERT INTO products (id, sku, name, manufacturer, device_type, import_price, selling_price, quantity, commission_rate, status, description, image_url, category_id, sale_agent_id, created_at, updated_at) VALUES
('d1000000-0000-0000-0000-000000000001', 'IP15PRO-256', 'iPhone 15 Pro 256GB', 'Apple', 'Smartphone', 25000000, 29990000, 50, 0.05, 0, 'Apple iPhone 15 Pro with A17 Pro chip, titanium design, 48MP camera system', 'https://images.unsplash.com/photo-1695048133142-1a20484d2569', 'a1000000-0000-0000-0000-000000000001', 'b2000000-0000-0000-0000-000000000001', NOW() - INTERVAL '3 months', NOW()),
('d1000000-0000-0000-0000-000000000002', 'IP15-128', 'iPhone 15 128GB', 'Apple', 'Smartphone', 18000000, 22990000, 75, 0.05, 0, 'Apple iPhone 15 with Dynamic Island, 48MP camera, USB-C', 'https://images.unsplash.com/photo-1695048133142-1a20484d2569', 'a1000000-0000-0000-0000-000000000001', 'b2000000-0000-0000-0000-000000000001', NOW() - INTERVAL '3 months', NOW()),
('d1000000-0000-0000-0000-000000000003', 'SS-S24U-256', 'Samsung Galaxy S24 Ultra 256GB', 'Samsung', 'Smartphone', 26000000, 31990000, 40, 0.06, 0, 'Samsung Galaxy S24 Ultra with Snapdragon 8 Gen 3, S Pen, 200MP camera', 'https://images.unsplash.com/photo-1610945415295-d9bbf067e59c', 'a1000000-0000-0000-0000-000000000001', 'b2000000-0000-0000-0000-000000000002', NOW() - INTERVAL '2 months', NOW()),
('d1000000-0000-0000-0000-000000000004', 'SS-S24-128', 'Samsung Galaxy S24 128GB', 'Samsung', 'Smartphone', 17000000, 21990000, 60, 0.06, 0, 'Samsung Galaxy S24 with Galaxy AI, 50MP camera, Dynamic AMOLED 2X', 'https://images.unsplash.com/photo-1610945415295-d9bbf067e59c', 'a1000000-0000-0000-0000-000000000001', 'b2000000-0000-0000-0000-000000000002', NOW() - INTERVAL '2 months', NOW()),
('d1000000-0000-0000-0000-000000000005', 'XM-14PRO-256', 'Xiaomi 14 Pro 256GB', 'Xiaomi', 'Smartphone', 15000000, 18990000, 45, 0.07, 0, 'Xiaomi 14 Pro with Leica optics, Snapdragon 8 Gen 3', 'https://images.unsplash.com/photo-1574944985070-8f3ebc6b79d2', 'a1000000-0000-0000-0000-000000000001', 'b2000000-0000-0000-0000-000000000003', NOW() - INTERVAL '1 month', NOW())
ON CONFLICT (id) DO NOTHING;

-- Laptops (Category: a1000000-0000-0000-0000-000000000002)
INSERT INTO products (id, sku, name, manufacturer, device_type, import_price, selling_price, quantity, commission_rate, status, description, image_url, category_id, sale_agent_id, created_at, updated_at) VALUES
('d2000000-0000-0000-0000-000000000001', 'MBP14-M3P-512', 'MacBook Pro 14" M3 Pro 512GB', 'Apple', 'Laptop', 45000000, 52990000, 25, 0.04, 0, 'MacBook Pro 14-inch with M3 Pro chip, 18GB RAM, Space Black', 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8', 'a1000000-0000-0000-0000-000000000002', 'b2000000-0000-0000-0000-000000000001', NOW() - INTERVAL '2 months', NOW()),
('d2000000-0000-0000-0000-000000000002', 'MBA15-M3-256', 'MacBook Air 15" M3 256GB', 'Apple', 'Laptop', 30000000, 35990000, 35, 0.04, 0, 'MacBook Air 15-inch with M3 chip, 8GB RAM, Midnight', 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8', 'a1000000-0000-0000-0000-000000000002', 'b2000000-0000-0000-0000-000000000001', NOW() - INTERVAL '2 months', NOW()),
('d2000000-0000-0000-0000-000000000003', 'DELL-XPS15-I7', 'Dell XPS 15 i7 16GB 512GB', 'Dell', 'Laptop', 35000000, 42990000, 20, 0.05, 0, 'Dell XPS 15 with Intel i7-13700H, 16GB RAM, OLED Display', 'https://images.unsplash.com/photo-1593642632559-0c6d3fc62b89', 'a1000000-0000-0000-0000-000000000002', 'b2000000-0000-0000-0000-000000000002', NOW() - INTERVAL '1 month', NOW()),
('d2000000-0000-0000-0000-000000000004', 'ASUS-ROG-16', 'ASUS ROG Zephyrus G16 RTX 4070', 'ASUS', 'Laptop', 42000000, 49990000, 15, 0.05, 0, 'ASUS ROG Zephyrus G16 with Intel i9, RTX 4070, 32GB RAM', 'https://images.unsplash.com/photo-1603302576837-37561b2e2302', 'a1000000-0000-0000-0000-000000000002', 'b2000000-0000-0000-0000-000000000003', NOW() - INTERVAL '3 weeks', NOW()),
('d2000000-0000-0000-0000-000000000005', 'LNV-TP-X1C', 'Lenovo ThinkPad X1 Carbon Gen 11', 'Lenovo', 'Laptop', 38000000, 45990000, 18, 0.05, 0, 'ThinkPad X1 Carbon Gen 11 with Intel i7, 16GB RAM, 512GB SSD', 'https://images.unsplash.com/photo-1588872657578-7efd1f1555ed', 'a1000000-0000-0000-0000-000000000002', 'b2000000-0000-0000-0000-000000000002', NOW() - INTERVAL '1 month', NOW())
ON CONFLICT (id) DO NOTHING;

-- Tablets (Category: a1000000-0000-0000-0000-000000000003)
INSERT INTO products (id, sku, name, manufacturer, device_type, import_price, selling_price, quantity, commission_rate, status, description, image_url, category_id, sale_agent_id, created_at, updated_at) VALUES
('d3000000-0000-0000-0000-000000000001', 'IPADPRO-11-M4', 'iPad Pro 11" M4 256GB', 'Apple', 'Tablet', 22000000, 27990000, 30, 0.05, 0, 'iPad Pro 11-inch with M4 chip, Ultra Retina XDR display', 'https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0', 'a1000000-0000-0000-0000-000000000003', 'b2000000-0000-0000-0000-000000000001', NOW() - INTERVAL '1 month', NOW()),
('d3000000-0000-0000-0000-000000000002', 'IPADAIR-11-M2', 'iPad Air 11" M2 128GB', 'Apple', 'Tablet', 14000000, 17990000, 40, 0.05, 0, 'iPad Air 11-inch with M2 chip, Liquid Retina display', 'https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0', 'a1000000-0000-0000-0000-000000000003', 'b2000000-0000-0000-0000-000000000001', NOW() - INTERVAL '1 month', NOW()),
('d3000000-0000-0000-0000-000000000003', 'SS-TABS9U', 'Samsung Galaxy Tab S9 Ultra', 'Samsung', 'Tablet', 24000000, 29990000, 20, 0.06, 0, 'Galaxy Tab S9 Ultra with 14.6" AMOLED, S Pen included', 'https://images.unsplash.com/photo-1561154464-82e9adf32764', 'a1000000-0000-0000-0000-000000000003', 'b2000000-0000-0000-0000-000000000002', NOW() - INTERVAL '2 weeks', NOW())
ON CONFLICT (id) DO NOTHING;

-- Audio (Category: a1000000-0000-0000-0000-000000000004)
INSERT INTO products (id, sku, name, manufacturer, device_type, import_price, selling_price, quantity, commission_rate, status, description, image_url, category_id, sale_agent_id, created_at, updated_at) VALUES
('d4000000-0000-0000-0000-000000000001', 'APP-PRO2', 'AirPods Pro 2nd Gen', 'Apple', 'Earbuds', 5000000, 6490000, 100, 0.08, 0, 'AirPods Pro with USB-C, Active Noise Cancellation, Adaptive Audio', 'https://images.unsplash.com/photo-1606220945770-b5b6c2c55bf1', 'a1000000-0000-0000-0000-000000000004', 'b2000000-0000-0000-0000-000000000001', NOW() - INTERVAL '2 months', NOW()),
('d4000000-0000-0000-0000-000000000002', 'APP-MAX', 'AirPods Max', 'Apple', 'Headphones', 12000000, 14990000, 30, 0.06, 0, 'AirPods Max with high-fidelity audio, Active Noise Cancellation', 'https://images.unsplash.com/photo-1613040809024-b4ef7ba99bc3', 'a1000000-0000-0000-0000-000000000004', 'b2000000-0000-0000-0000-000000000001', NOW() - INTERVAL '2 months', NOW()),
('d4000000-0000-0000-0000-000000000003', 'SONY-WH1000XM5', 'Sony WH-1000XM5', 'Sony', 'Headphones', 7000000, 8990000, 45, 0.07, 0, 'Sony WH-1000XM5 with industry-leading noise cancellation', 'https://images.unsplash.com/photo-1618366712010-f4ae9c647dcb', 'a1000000-0000-0000-0000-000000000004', 'b2000000-0000-0000-0000-000000000002', NOW() - INTERVAL '1 month', NOW()),
('d4000000-0000-0000-0000-000000000004', 'SS-BUDS2PRO', 'Samsung Galaxy Buds2 Pro', 'Samsung', 'Earbuds', 3500000, 4490000, 80, 0.08, 0, 'Galaxy Buds2 Pro with 360 Audio, ANC, Hi-Fi 24-bit audio', 'https://images.unsplash.com/photo-1590658268037-6bf12165a8df', 'a1000000-0000-0000-0000-000000000004', 'b2000000-0000-0000-0000-000000000002', NOW() - INTERVAL '1 month', NOW())
ON CONFLICT (id) DO NOTHING;

-- Smartwatches (Category: a1000000-0000-0000-0000-000000000005)
INSERT INTO products (id, sku, name, manufacturer, device_type, import_price, selling_price, quantity, commission_rate, status, description, image_url, category_id, sale_agent_id, created_at, updated_at) VALUES
('d5000000-0000-0000-0000-000000000001', 'AW-S9-45', 'Apple Watch Series 9 45mm', 'Apple', 'Smartwatch', 9500000, 11990000, 55, 0.06, 0, 'Apple Watch Series 9 with S9 chip, Double Tap gesture', 'https://images.unsplash.com/photo-1434493907317-a46b5bbe7834', 'a1000000-0000-0000-0000-000000000005', 'b2000000-0000-0000-0000-000000000001', NOW() - INTERVAL '2 months', NOW()),
('d5000000-0000-0000-0000-000000000002', 'AW-ULTRA2', 'Apple Watch Ultra 2', 'Apple', 'Smartwatch', 18000000, 21990000, 25, 0.05, 0, 'Apple Watch Ultra 2 with titanium case, 36-hour battery', 'https://images.unsplash.com/photo-1434493907317-a46b5bbe7834', 'a1000000-0000-0000-0000-000000000005', 'b2000000-0000-0000-0000-000000000001', NOW() - INTERVAL '2 months', NOW()),
('d5000000-0000-0000-0000-000000000003', 'SS-GW6-44', 'Samsung Galaxy Watch6 44mm', 'Samsung', 'Smartwatch', 7000000, 8990000, 40, 0.07, 0, 'Galaxy Watch6 with BioActive Sensor, Sapphire Crystal', 'https://images.unsplash.com/photo-1579586337278-3befd40fd17a', 'a1000000-0000-0000-0000-000000000005', 'b2000000-0000-0000-0000-000000000002', NOW() - INTERVAL '1 month', NOW())
ON CONFLICT (id) DO NOTHING;

-- Accessories (Category: a1000000-0000-0000-0000-000000000006)
INSERT INTO products (id, sku, name, manufacturer, device_type, import_price, selling_price, quantity, commission_rate, status, description, image_url, category_id, sale_agent_id, created_at, updated_at) VALUES
('d6000000-0000-0000-0000-000000000001', 'APP-MAGSAFE', 'MagSafe Charger', 'Apple', 'Charger', 800000, 1090000, 150, 0.10, 0, 'Apple MagSafe Charger for iPhone and AirPods', 'https://images.unsplash.com/photo-1609091839311-d5365f9ff1c5', 'a1000000-0000-0000-0000-000000000006', 'b2000000-0000-0000-0000-000000000003', NOW() - INTERVAL '2 months', NOW()),
('d6000000-0000-0000-0000-000000000002', 'ANK-65W-GAN', 'Anker 65W GaN Charger', 'Anker', 'Charger', 600000, 890000, 200, 0.12, 0, 'Anker 65W GaN II USB-C charger, ultra-compact', 'https://images.unsplash.com/photo-1583863788434-e58a36330cf0', 'a1000000-0000-0000-0000-000000000006', 'b2000000-0000-0000-0000-000000000003', NOW() - INTERVAL '1 month', NOW()),
('d6000000-0000-0000-0000-000000000003', 'CASE-IP15PRO', 'iPhone 15 Pro Silicone Case', 'Apple', 'Case', 900000, 1290000, 120, 0.10, 0, 'Apple Silicone Case with MagSafe for iPhone 15 Pro', 'https://images.unsplash.com/photo-1601784551446-20c9e07cdbdb', 'a1000000-0000-0000-0000-000000000006', 'b2000000-0000-0000-0000-000000000003', NOW() - INTERVAL '3 weeks', NOW()),
('d6000000-0000-0000-0000-000000000004', 'USB-C-2M', 'USB-C Cable 2m Braided', 'Anker', 'Cable', 200000, 350000, 300, 0.15, 0, 'Anker USB-C to USB-C cable, 100W, braided nylon', 'https://images.unsplash.com/photo-1600490722773-35753aea6332', 'a1000000-0000-0000-0000-000000000006', 'b2000000-0000-0000-0000-000000000003', NOW() - INTERVAL '3 weeks', NOW())
ON CONFLICT (id) DO NOTHING;

-- ============================================
-- 6. ORDERS - Sample orders with various statuses
-- ============================================

-- Orders from different periods with various statuses
-- Order 1: Delivered order from 2 weeks ago
INSERT INTO orders (id, order_date, status, payment_status, total_amount, discount_amount, shipping_fee, tax_amount, grand_total, note, customer_id, sale_agent_id, created_at, updated_at) VALUES
('e1000000-0000-0000-0000-000000000001', NOW() - INTERVAL '14 days', 4, 1, 36480000, 0, 30000, 3648000, 40158000, 'Delivered to customer address', 'c1000000-0000-0000-0000-000000000001', 'b2000000-0000-0000-0000-000000000001', NOW() - INTERVAL '14 days', NOW() - INTERVAL '12 days')
ON CONFLICT (id) DO NOTHING;

-- Order 2: Shipped order from 5 days ago
INSERT INTO orders (id, order_date, status, payment_status, total_amount, discount_amount, shipping_fee, tax_amount, grand_total, note, customer_id, sale_agent_id, created_at, updated_at) VALUES
('e1000000-0000-0000-0000-000000000002', NOW() - INTERVAL '5 days', 3, 1, 52990000, 2000000, 0, 5099000, 56089000, 'Express shipping requested', 'c1000000-0000-0000-0000-000000000002', 'b2000000-0000-0000-0000-000000000001', NOW() - INTERVAL '5 days', NOW() - INTERVAL '3 days')
ON CONFLICT (id) DO NOTHING;

-- Order 3: Processing order from 2 days ago
INSERT INTO orders (id, order_date, status, payment_status, total_amount, discount_amount, shipping_fee, tax_amount, grand_total, note, customer_id, sale_agent_id, created_at, updated_at) VALUES
('e1000000-0000-0000-0000-000000000003', NOW() - INTERVAL '2 days', 2, 1, 31990000, 0, 30000, 3199000, 35219000, 'Gift wrapping requested', 'c1000000-0000-0000-0000-000000000003', 'b2000000-0000-0000-0000-000000000002', NOW() - INTERVAL '2 days', NOW() - INTERVAL '1 day')
ON CONFLICT (id) DO NOTHING;

-- Order 4: Confirmed order from today
INSERT INTO orders (id, order_date, status, payment_status, total_amount, discount_amount, shipping_fee, tax_amount, grand_total, note, customer_id, sale_agent_id, created_at, updated_at) VALUES
('e1000000-0000-0000-0000-000000000004', NOW(), 1, 1, 8990000, 500000, 30000, 849000, 9369000, NULL, 'c1000000-0000-0000-0000-000000000004', 'b2000000-0000-0000-0000-000000000002', NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- Order 5: Pending order from today
INSERT INTO orders (id, order_date, status, payment_status, total_amount, discount_amount, shipping_fee, tax_amount, grand_total, note, customer_id, sale_agent_id, created_at, updated_at) VALUES
('e1000000-0000-0000-0000-000000000005', NOW(), 0, 0, 11990000, 0, 30000, 1199000, 13219000, 'Waiting for payment confirmation', 'c1000000-0000-0000-0000-000000000005', 'b2000000-0000-0000-0000-000000000003', NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- Order 6: Delivered order from 1 month ago (historical)
INSERT INTO orders (id, order_date, status, payment_status, total_amount, discount_amount, shipping_fee, tax_amount, grand_total, note, customer_id, sale_agent_id, created_at, updated_at) VALUES
('e1000000-0000-0000-0000-000000000006', NOW() - INTERVAL '30 days', 4, 1, 29990000, 1500000, 0, 2849000, 31339000, 'Repeat customer - VIP', 'c1000000-0000-0000-0000-000000000001', 'b2000000-0000-0000-0000-000000000001', NOW() - INTERVAL '30 days', NOW() - INTERVAL '27 days')
ON CONFLICT (id) DO NOTHING;

-- ============================================
-- 7. ORDER ITEMS - Products in each order
-- ============================================

-- Order 1 items: iPhone 15 Pro + AirPods Pro
INSERT INTO order_items (id, order_id, product_id, quantity, unit_sale_price, total_price, created_at, updated_at) VALUES
('f1000000-0000-0000-0000-000000000001', 'e1000000-0000-0000-0000-000000000001', 'd1000000-0000-0000-0000-000000000001', 1, 29990000, 29990000, NOW() - INTERVAL '14 days', NOW() - INTERVAL '14 days'),
('f1000000-0000-0000-0000-000000000002', 'e1000000-0000-0000-0000-000000000001', 'd4000000-0000-0000-0000-000000000001', 1, 6490000, 6490000, NOW() - INTERVAL '14 days', NOW() - INTERVAL '14 days')
ON CONFLICT (id) DO NOTHING;

-- Order 2 items: MacBook Pro 14"
INSERT INTO order_items (id, order_id, product_id, quantity, unit_sale_price, total_price, created_at, updated_at) VALUES
('f1000000-0000-0000-0000-000000000003', 'e1000000-0000-0000-0000-000000000002', 'd2000000-0000-0000-0000-000000000001', 1, 52990000, 52990000, NOW() - INTERVAL '5 days', NOW() - INTERVAL '5 days')
ON CONFLICT (id) DO NOTHING;

-- Order 3 items: Samsung Galaxy S24 Ultra
INSERT INTO order_items (id, order_id, product_id, quantity, unit_sale_price, total_price, created_at, updated_at) VALUES
('f1000000-0000-0000-0000-000000000004', 'e1000000-0000-0000-0000-000000000003', 'd1000000-0000-0000-0000-000000000003', 1, 31990000, 31990000, NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days')
ON CONFLICT (id) DO NOTHING;

-- Order 4 items: Sony WH-1000XM5
INSERT INTO order_items (id, order_id, product_id, quantity, unit_sale_price, total_price, created_at, updated_at) VALUES
('f1000000-0000-0000-0000-000000000005', 'e1000000-0000-0000-0000-000000000004', 'd4000000-0000-0000-0000-000000000003', 1, 8990000, 8990000, NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- Order 5 items: Apple Watch Series 9
INSERT INTO order_items (id, order_id, product_id, quantity, unit_sale_price, total_price, created_at, updated_at) VALUES
('f1000000-0000-0000-0000-000000000006', 'e1000000-0000-0000-0000-000000000005', 'd5000000-0000-0000-0000-000000000001', 1, 11990000, 11990000, NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- Order 6 items: iPhone 15 Pro (historical order)
INSERT INTO order_items (id, order_id, product_id, quantity, unit_sale_price, total_price, created_at, updated_at) VALUES
('f1000000-0000-0000-0000-000000000007', 'e1000000-0000-0000-0000-000000000006', 'd1000000-0000-0000-0000-000000000001', 1, 29990000, 29990000, NOW() - INTERVAL '30 days', NOW() - INTERVAL '30 days')
ON CONFLICT (id) DO NOTHING;

-- ============================================
-- 8. CART ITEMS - Sample cart items
-- ============================================
INSERT INTO cart_items (id, user_id, product_id, quantity, created_at, updated_at) VALUES
('g1000000-0000-0000-0000-000000000001', 'c1000000-0000-0000-0000-000000000001', 'd3000000-0000-0000-0000-000000000001', 1, NOW(), NOW()),
('g1000000-0000-0000-0000-000000000002', 'c1000000-0000-0000-0000-000000000002', 'd4000000-0000-0000-0000-000000000002', 1, NOW(), NOW()),
('g1000000-0000-0000-0000-000000000003', 'c1000000-0000-0000-0000-000000000003', 'd6000000-0000-0000-0000-000000000001', 2, NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- ============================================
-- SUMMARY
-- ============================================
-- Categories: 6
-- Users: 9 (1 Admin, 3 Sales Agents, 5 Customers)
-- Products: 20 (across 6 categories)
-- Orders: 6 (various statuses)
-- Order Items: 7
-- Cart Items: 3
-- ============================================

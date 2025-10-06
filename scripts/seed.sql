-- Seed database with demo data

-- Insert Users (password: Password123!)
-- BCrypt hash: $2a$11$N9qo8uLOickgx2ZMRZoMye/JtjIa73cZIfuhmMkRS6YW2TlFVEpj6
-- UserRole enum: Customer=0, Seller=1, Admin=2
INSERT INTO "Users" ("Id", "Email", "PasswordHash", "FirstName", "LastName", "Role", "IsEmailConfirmed", "CreatedAt")
VALUES 
    (gen_random_uuid(), 'admin@shopstream.com', '$2a$11$N9qo8uLOickgx2ZMRZoMye/JtjIa73cZIfuhmMkRS6YW2TlFVEpj6', 'Admin', 'User', 2, true, NOW()),
    (gen_random_uuid(), 'customer@example.com', '$2a$11$N9qo8uLOickgx2ZMRZoMye/JtjIa73cZIfuhmMkRS6YW2TlFVEpj6', 'John', 'Doe', 0, true, NOW())
ON CONFLICT ("Email") DO NOTHING;

-- Insert Categories
INSERT INTO "Categories" ("Id", "Name", "Description", "CreatedAt")
VALUES 
    (gen_random_uuid(), 'Electronics', 'Electronic devices and accessories', NOW()),
    (gen_random_uuid(), 'Clothing', 'Apparel and fashion', NOW()),
    (gen_random_uuid(), 'Books', 'Books and literature', NOW())
ON CONFLICT DO NOTHING;

-- Insert Products
WITH cat AS (SELECT "Id", "Name" FROM "Categories")
INSERT INTO "Products" ("Id", "Name", "SKU", "Description", "Price", "StockQuantity", "CategoryId", "IsActive", "CreatedAt")
SELECT gen_random_uuid(), 'Laptop Pro 15', 'ELEC-LAP-001', 'High-performance laptop with 16GB RAM and 512GB SSD', 1299.99, 50, (SELECT "Id" FROM cat WHERE "Name" = 'Electronics'), true, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Products" WHERE "SKU" = 'ELEC-LAP-001')
UNION ALL
SELECT gen_random_uuid(), 'Wireless Mouse', 'ELEC-MOU-001', 'Ergonomic wireless mouse with precision tracking', 29.99, 200, (SELECT "Id" FROM cat WHERE "Name" = 'Electronics'), true, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Products" WHERE "SKU" = 'ELEC-MOU-001')
UNION ALL
SELECT gen_random_uuid(), 'Cotton T-Shirt', 'CLO-TSH-001', 'Comfortable 100% cotton t-shirt', 19.99, 150, (SELECT "Id" FROM cat WHERE "Name" = 'Clothing'), true, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Products" WHERE "SKU" = 'CLO-TSH-001')
UNION ALL
SELECT gen_random_uuid(), 'Programming Guide', 'BOOK-PRG-001', 'Comprehensive guide to modern programming', 49.99, 75, (SELECT "Id" FROM cat WHERE "Name" = 'Books'), true, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Products" WHERE "SKU" = 'BOOK-PRG-001');

-- Insert Product Images (using placeholder.com for demo)
INSERT INTO "ProductImages" ("Id", "ProductId", "Url", "AltText", "DisplayOrder", "CreatedAt")
SELECT gen_random_uuid(), p."Id", 'https://via.placeholder.com/400x400/FF9900/FFFFFF?text=' || REPLACE(p."Name", ' ', '+'), p."Name", 1, NOW()
FROM "Products" p
WHERE NOT EXISTS (SELECT 1 FROM "ProductImages" pi WHERE pi."ProductId" = p."Id");

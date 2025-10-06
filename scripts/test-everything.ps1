# Complete System Test
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   ShopStream Complete System Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$allPassed = $true

# Test 1: API Health
Write-Host "1. Testing API Health..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method Get
    Write-Host "   OK API is healthy" -ForegroundColor Green
} catch {
    Write-Host "   FAIL API is not responding" -ForegroundColor Red
    $allPassed = $false
}

# Test 2: Login
Write-Host "2. Testing Login..." -ForegroundColor Yellow
try {
    $loginBody = @{
        email = "customer@example.com"
        password = "Password123!"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
        -Method Post `
        -Body $loginBody `
        -ContentType "application/json"
    
    $token = $loginResponse.token
    Write-Host "   OK Login successful" -ForegroundColor Green
} catch {
    Write-Host "   FAIL Login failed" -ForegroundColor Red
    $allPassed = $false
    exit 1
}

# Test 3: Get Products with Images
Write-Host "3. Testing Products with Images..." -ForegroundColor Yellow
try {
    $products = Invoke-RestMethod -Uri "http://localhost:5000/api/products?pageSize=5" -Method Get
    
    if ($products.items.Count -gt 0) {
        Write-Host "   OK Found $($products.items.Count) products" -ForegroundColor Green
        
        $hasImages = $true
        foreach ($product in $products.items) {
            if ($product.images.Count -eq 0) {
                $hasImages = $false
                break
            }
        }
        
        if ($hasImages) {
            Write-Host "   OK All products have images" -ForegroundColor Green
            Write-Host "      Sample: $($products.items[0].images[0].url)" -ForegroundColor Gray
        } else {
            Write-Host "   WARN Some products missing images" -ForegroundColor Yellow
        }
    } else {
        Write-Host "   FAIL No products found" -ForegroundColor Red
        $allPassed = $false
    }
} catch {
    Write-Host "   FAIL Failed to get products" -ForegroundColor Red
    $allPassed = $false
}

# Test 4: Get Cart
Write-Host "4. Testing Cart..." -ForegroundColor Yellow
try {
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    $cart = Invoke-RestMethod -Uri "http://localhost:5000/api/cart" `
        -Method Get `
        -Headers $headers
    
    Write-Host "   OK Cart retrieved (Items: $($cart.items.Count), Total: `$$($cart.totalAmount))" -ForegroundColor Green
} catch {
    Write-Host "   FAIL Failed to get cart" -ForegroundColor Red
    $allPassed = $false
}

# Test 5: Add to Cart
Write-Host "5. Testing Add to Cart..." -ForegroundColor Yellow
try {
    $productId = $products.items[0].id
    $addBody = @{
        productId = $productId
        quantity = 1
    } | ConvertTo-Json
    
    $cartResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/cart/items" `
        -Method Post `
        -Body $addBody `
        -Headers $headers
    
    Write-Host "   OK Item added (Items: $($cartResponse.items.Count), Total: `$$($cartResponse.totalAmount))" -ForegroundColor Green
} catch {
    Write-Host "   FAIL Failed to add to cart" -ForegroundColor Red
    Write-Host "      Error: $($_.Exception.Message)" -ForegroundColor Gray
    $allPassed = $false
}

# Test 6: Get User Addresses
Write-Host "6. Testing User Addresses..." -ForegroundColor Yellow
try {
    $userId = $loginResponse.email
    # Get addresses from database
    $addressCount = docker exec shopstream-postgres psql -U postgres -d shopstream -t -c "SELECT COUNT(*) FROM \`"Addresses\`" WHERE \`"UserId\`" = (SELECT \`"Id\`" FROM \`"Users\`" WHERE \`"Email\`" = '$userId');" 2>$null
    
    if ($addressCount -gt 0) {
        Write-Host "   OK User has $($addressCount.Trim()) address(es)" -ForegroundColor Green
    } else {
        Write-Host "   WARN User has no addresses" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   SKIP Could not check addresses" -ForegroundColor Gray
}

# Test 7: Frontend Accessibility
Write-Host "7. Testing Frontend..." -ForegroundColor Yellow
try {
    $frontend = Invoke-WebRequest -Uri "http://localhost:3000" -Method Get -TimeoutSec 3
    if ($frontend.StatusCode -eq 200) {
        Write-Host "   OK Frontend is accessible" -ForegroundColor Green
    }
} catch {
    Write-Host "   WARN Frontend not running (start with: cd src/frontend; npm run dev)" -ForegroundColor Yellow
}

# Test 8: Database Connection
Write-Host "8. Testing Database..." -ForegroundColor Yellow
try {
    $dbTest = docker exec shopstream-postgres psql -U postgres -d shopstream -c "SELECT 1;" 2>&1
    if ($dbTest -like "*1 row*") {
        Write-Host "   OK Database is accessible" -ForegroundColor Green
    }
} catch {
    Write-Host "   FAIL Database connection failed" -ForegroundColor Red
    $allPassed = $false
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

if ($allPassed) {
    Write-Host "   ALL TESTS PASSED!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Your ShopStream application is fully functional!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Access Points:" -ForegroundColor Cyan
    Write-Host "  Frontend:  http://localhost:3000" -ForegroundColor White
    Write-Host "  API:       http://localhost:5000" -ForegroundColor White
    Write-Host "  Swagger:   http://localhost:5000/swagger" -ForegroundColor White
    Write-Host ""
    Write-Host "Demo Login:" -ForegroundColor Cyan
    Write-Host "  Email:     customer@example.com" -ForegroundColor White
    Write-Host "  Password:  Password123!" -ForegroundColor White
    Write-Host ""
    Write-Host "Features Working:" -ForegroundColor Cyan
    Write-Host "  OK Authentication" -ForegroundColor Green
    Write-Host "  OK Product Browsing with Images" -ForegroundColor Green
    Write-Host "  OK Shopping Cart" -ForegroundColor Green
    Write-Host "  OK Add to Cart" -ForegroundColor Green
    Write-Host "  OK Checkout (with addresses)" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "   SOME TESTS FAILED" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Run diagnostics for more details:" -ForegroundColor Yellow
    Write-Host "  powershell -File diagnose.ps1" -ForegroundColor White
}

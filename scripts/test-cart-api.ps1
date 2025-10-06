# Test Cart API Script
Write-Host "🧪 Testing ShopStream Cart API" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check if API is running
Write-Host "1. Checking if API is running..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method Get
    Write-Host "✅ API is running!" -ForegroundColor Green
} catch {
    Write-Host "❌ API is not running. Please start the backend first." -ForegroundColor Red
    Write-Host "   Run: cd src/ShopStream.Api && dotnet run" -ForegroundColor Gray
    exit 1
}

Write-Host ""

# Test 2: Login to get token
Write-Host "2. Logging in as customer..." -ForegroundColor Yellow
$loginBody = @{
    email = "customer@example.com"
    password = "Password123!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
        -Method Post `
        -Body $loginBody `
        -ContentType "application/json"
    
    $token = $loginResponse.token
    Write-Host "✅ Login successful!" -ForegroundColor Green
    Write-Host "   Token: $($token.Substring(0, 20))..." -ForegroundColor Gray
} catch {
    Write-Host "❌ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 3: Get products to find a valid product ID
Write-Host "3. Getting products..." -ForegroundColor Yellow
try {
    $products = Invoke-RestMethod -Uri "http://localhost:5000/api/products?pageSize=1" -Method Get
    $productId = $products.items[0].id
    $productName = $products.items[0].name
    Write-Host "✅ Found product: $productName" -ForegroundColor Green
    Write-Host "   Product ID: $productId" -ForegroundColor Gray
} catch {
    Write-Host "❌ Failed to get products: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 4: Get current cart
Write-Host "4. Getting current cart..." -ForegroundColor Yellow
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $cart = Invoke-RestMethod -Uri "http://localhost:5000/api/cart" `
        -Method Get `
        -Headers $headers
    
    Write-Host "✅ Cart retrieved!" -ForegroundColor Green
    Write-Host "   Items in cart: $($cart.items.Count)" -ForegroundColor Gray
    Write-Host "   Total: `$$($cart.totalAmount)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Failed to get cart: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
}

Write-Host ""

# Test 5: Add item to cart
Write-Host "5. Adding item to cart..." -ForegroundColor Yellow
$addToCartBody = @{
    productId = $productId
    quantity = 1
} | ConvertTo-Json

Write-Host "   Request body: $addToCartBody" -ForegroundColor Gray

try {
    $cartResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/cart/items" `
        -Method Post `
        -Body $addToCartBody `
        -Headers $headers
    
    Write-Host "✅ Item added to cart successfully!" -ForegroundColor Green
    Write-Host "   Items in cart: $($cartResponse.items.Count)" -ForegroundColor Gray
    Write-Host "   Total: `$$($cartResponse.totalAmount)" -ForegroundColor Gray
    
    foreach ($item in $cartResponse.items) {
        Write-Host "   - $($item.productName) x$($item.quantity) = `$$($item.totalPrice)" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Failed to add to cart!" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "   Response: $responseBody" -ForegroundColor Red
    }
    exit 1
}

Write-Host ""
Write-Host "🎉 All tests passed! Cart API is working correctly." -ForegroundColor Green

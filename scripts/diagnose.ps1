# ShopStream Diagnostic Script
Write-Host "ShopStream System Diagnostic" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

$allGood = $true

# Check 1: PostgreSQL
Write-Host "1. Checking PostgreSQL..." -ForegroundColor Yellow
try {
    $pgTest = docker ps --filter "name=shopstream-postgres" --format "{{.Status}}"
    if ($pgTest -like "*Up*") {
        Write-Host "   OK PostgreSQL is running" -ForegroundColor Green
    } else {
        Write-Host "   WARNING PostgreSQL container exists but not running" -ForegroundColor Yellow
        Write-Host "      Run: docker-compose up -d postgres" -ForegroundColor Gray
        $allGood = $false
    }
} catch {
    Write-Host "   ERROR PostgreSQL is not running" -ForegroundColor Red
    Write-Host "      Run: docker-compose up -d postgres" -ForegroundColor Gray
    $allGood = $false
}

# Check 2: Backend API
Write-Host "2. Checking Backend API..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -Method Get -TimeoutSec 2
    if ($response.StatusCode -eq 200) {
        Write-Host "   OK Backend API is running (http://localhost:5000)" -ForegroundColor Green
    }
} catch {
    Write-Host "   ERROR Backend API is not responding" -ForegroundColor Red
    Write-Host "      Run: cd src/ShopStream.Api; dotnet run" -ForegroundColor Gray
    $allGood = $false
}

# Check 3: Frontend Dev Server
Write-Host "3. Checking Frontend..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:3000" -Method Get -TimeoutSec 2
    if ($response.StatusCode -eq 200) {
        Write-Host "   OK Frontend is running (http://localhost:3000)" -ForegroundColor Green
    }
} catch {
    Write-Host "   WARNING Frontend is not running" -ForegroundColor Yellow
    Write-Host "      Run: cd src/frontend; npm run dev" -ForegroundColor Gray
}

# Check 4: Database Connection
Write-Host "4. Checking Database Connection..." -ForegroundColor Yellow
try {
    $dbTest = docker exec shopstream-postgres psql -U postgres -d shopstream -c "SELECT 1;" 2>&1
    $dbTestStr = $dbTest | Out-String
    if ($dbTestStr -match "1 row") {
        Write-Host "   OK Database connection successful" -ForegroundColor Green
    } else {
        Write-Host "   ERROR Database connection failed" -ForegroundColor Red
        $allGood = $false
    }
} catch {
    Write-Host "   ERROR Cannot connect to database" -ForegroundColor Red
    $allGood = $false
}

# Check 5: Products in Database
Write-Host "5. Checking Products..." -ForegroundColor Yellow
try {
    $productCount = docker exec shopstream-postgres psql -U postgres -d shopstream -t -c "SELECT COUNT(*) FROM products;" 2>&1
    $count = [int]($productCount.Trim())
    if ($count -gt 0) {
        Write-Host "   OK Found $count products in database" -ForegroundColor Green
    } else {
        Write-Host "   WARNING No products found in database" -ForegroundColor Yellow
        Write-Host "      Database may need to be seeded" -ForegroundColor Gray
    }
} catch {
    Write-Host "   WARNING Could not check products" -ForegroundColor Yellow
}

# Check 6: Test Login
Write-Host "6. Testing Authentication..." -ForegroundColor Yellow
if ($allGood) {
    try {
        $loginBody = @{
            email = "customer@example.com"
            password = "Password123!"
        } | ConvertTo-Json

        $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
            -Method Post `
            -Body $loginBody `
            -ContentType "application/json" `
            -TimeoutSec 5

        if ($loginResponse.token) {
            Write-Host "   OK Authentication working" -ForegroundColor Green
        }
    } catch {
        Write-Host "   ERROR Authentication failed" -ForegroundColor Red
        Write-Host "      Error: $($_.Exception.Message)" -ForegroundColor Gray
        $allGood = $false
    }
} else {
    Write-Host "   SKIPPED (backend not running)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan

if ($allGood) {
    Write-Host "All systems operational!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Access points:" -ForegroundColor Cyan
    Write-Host "  Frontend:  http://localhost:3000" -ForegroundColor White
    Write-Host "  API:       http://localhost:5000" -ForegroundColor White
    Write-Host "  Swagger:   http://localhost:5000/swagger" -ForegroundColor White
    Write-Host ""
    Write-Host "Demo accounts:" -ForegroundColor Cyan
    Write-Host "  Admin:     admin@shopstream.com / Password123!" -ForegroundColor White
    Write-Host "  Customer:  customer@example.com / Password123!" -ForegroundColor White
} else {
    Write-Host "Some issues detected" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Quick fix:" -ForegroundColor Cyan
    Write-Host "  1. Start database:  docker-compose up -d postgres" -ForegroundColor White
    Write-Host "  2. Start backend:   cd src/ShopStream.Api; dotnet run" -ForegroundColor White
    Write-Host "  3. Start frontend:  cd src/frontend; npm run dev" -ForegroundColor White
    Write-Host ""
    Write-Host "Or start everything with Docker:" -ForegroundColor Cyan
    Write-Host "  docker-compose up --build" -ForegroundColor White
}

Write-Host ""
Write-Host "For detailed troubleshooting, see TROUBLESHOOTING.md" -ForegroundColor Gray

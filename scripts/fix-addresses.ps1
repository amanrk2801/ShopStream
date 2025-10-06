# Fix Missing Addresses Issue
Write-Host "Fixing user addresses..." -ForegroundColor Yellow

# Add addresses for all users who don't have one
$sql = @"
INSERT INTO \"Addresses\" (\"Id\", \"UserId\", \"Street\", \"City\", \"State\", \"ZipCode\", \"Country\", \"IsDefault\", \"CreatedAt\")
SELECT gen_random_uuid(), u.\"Id\", '123 Main St', 'New York', 'NY', '10001', 'USA', true, NOW()
FROM \"Users\" u
WHERE NOT EXISTS (
    SELECT 1 FROM \"Addresses\" a WHERE a.\"UserId\" = u.\"Id\"
);
"@

docker exec shopstream-postgres psql -U postgres -d shopstream -c $sql

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Addresses fixed successfully!" -ForegroundColor Green
} else {
    Write-Host "❌ Failed to fix addresses" -ForegroundColor Red
}

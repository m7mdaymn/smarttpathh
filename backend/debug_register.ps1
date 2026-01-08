$baseUrl = "http://localhost:5078/api"
$email = "debug_merch_$(Get-Random)@test.com"
$body = @{
    name = "Debug Merchant"; businessName = "Debug Wash"; city = "Test City"; phone = "1234567890";
    email = $email; password = "Password123!";
    businessType = "Car Wash"; branchName = "Main Branch"; subscriptionType = "Pro"; paymentMethod = "Credit Card"
}
try {
    $res = Invoke-RestMethod -Uri "$baseUrl/auth/register/merchant" -Method POST -Body ($body | ConvertTo-Json) -ContentType "application/json"
    Write-Host "✅ Success: $($res | ConvertTo-Json -Depth 5)"
} catch {
    Write-Host "❌ Failed: $($_.ToString())"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader $_.Exception.Response.GetResponseStream()
        Write-Host "Body: $($reader.ReadToEnd())"
    }
}

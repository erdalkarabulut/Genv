# Test AlarmTemplates API
$base = "http://127.0.0.1:5278"
$body = '{"email":"admin@genvapi.com","password":"Admin123!"}'
$login = Invoke-RestMethod -Method Post -Uri "$base/api/Auth/Login" -ContentType "application/json" -Body $body
$token = $login.accessToken.token
Write-Host "Token OK"
$result = Invoke-RestMethod -Method Get -Uri "$base/api/AlarmTemplates" -Headers @{"Authorization"="Bearer $token"}
Write-Host "Items count: $($result.Items.Count)"
foreach ($item in $result.Items) {
    Write-Host "  - $($item.Name)"
}
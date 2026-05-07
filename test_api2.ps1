# Test AlarmTemplates raw response
$base = "http://127.0.0.1:5278"
$body = '{"email":"narch@kodlama.io","password":"Passw0rd123!"}'
$login = Invoke-RestMethod -Method Post -Uri "$base/api/Auth/Login" -ContentType "application/json" -Body $body
$token = $login.accessToken.token
$resp = Invoke-RestMethod -Method Get -Uri "$base/api/AlarmTemplates" -Headers @{"Authorization"="Bearer $token"} -ResponseHeadersVariable "headers"
Write-Host "Status code from Get:"
Write-Host "Items:" $resp.Items
Write-Host "Count:" $resp.Items.Count
Write-Host "Raw response:" $resp
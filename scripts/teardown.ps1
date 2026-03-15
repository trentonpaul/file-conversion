$ErrorActionPreference = "Stop"

Write-Host "==> Deleting Kind cluster..."
kind delete cluster --name fileconversion

Write-Host "==> Done!"
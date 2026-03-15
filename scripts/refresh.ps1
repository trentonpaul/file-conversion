$ErrorActionPreference = "Stop"

Write-Host "==> Rebuilding images..."
docker build -f docker/Dockerfile.api -t fileconversion-api:latest .
docker build -f docker/Dockerfile.worker -t fileconversion-worker:latest .

Write-Host "==> Loading images into Kind..."
kind load docker-image fileconversion-api:latest --name fileconversion
kind load docker-image fileconversion-worker:latest --name fileconversion

Write-Host "==> Restarting deployments..."
kubectl rollout restart deployment/api -n fileconversion
kubectl rollout restart deployment/worker -n fileconversion

Write-Host "==> Watching rollout..."
kubectl rollout status deployment/api -n fileconversion
kubectl rollout status deployment/worker -n fileconversion
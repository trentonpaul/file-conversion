$ErrorActionPreference = "Stop"

Write-Host "==> Creating Kind cluster..."
kind create cluster --name fileconversion --wait 60s

Write-Host "==> Installing KEDA..."
kubectl apply -f https://github.com/kedacore/keda/releases/download/v2.13.0/keda-2.13.0.yaml
kubectl wait --for=condition=ready pod -l app=keda-operator -n keda --timeout=120s

Write-Host "==> Building images..."
docker build -f docker/Dockerfile.api -t fileconversion-api:latest .
docker build -f docker/Dockerfile.worker -t fileconversion-worker:latest .

Write-Host "==> Loading images into Kind..."
kind load docker-image fileconversion-api:latest --name fileconversion
kind load docker-image fileconversion-worker:latest --name fileconversion

Write-Host "==> Applying manifests..."
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/secrets.yaml
kubectl apply -f k8s/postgres-deployment.yaml
kubectl apply -f k8s/rabbitmq-deployment.yaml
kubectl apply -f k8s/storage-pvc.yaml

Write-Host "==> Waiting for postgres to be ready..."
kubectl wait --for=condition=ready pod -l app=postgres -n fileconversion --timeout=120s

Write-Host "==> Waiting for rabbitmq to be ready..."
kubectl wait --for=condition=ready pod -l app=rabbitmq -n fileconversion --timeout=120s

Write-Host "==> Running migrations..."
kubectl delete job db-migrator -n fileconversion --ignore-not-found
kubectl apply -f k8s/migrator-job.yaml
kubectl wait --for=condition=complete job/db-migrator -n fileconversion --timeout=60s

Write-Host "==> Deploying API and Worker..."
kubectl apply -f k8s/api-deployment.yaml
kubectl apply -f k8s/worker-deployment.yaml
kubectl apply -f k8s/worker-triggerauthentication.yaml
kubectl apply -f k8s/worker-scaledobject.yaml

Write-Host "==> Waiting for API and Worker to be ready..."
kubectl wait --for=condition=ready pod -l app=api -n fileconversion --timeout=60s

Write-Host "==> All done!"
Write-Host ""
Write-Host "Prerequisites: Docker Desktop, Kind, kubectl"
Write-Host ""
Write-Host "To test the API:"
Write-Host "    kubectl port-forward service/api 8080:80 -n fileconversion"
Write-Host ""
Write-Host "To watch logs:"
Write-Host "    kubectl logs -f deployment/api -n fileconversion"
Write-Host "    kubectl logs -f deployment/worker -n fileconversion"
Write-Host ""
Write-Host "To watch autoscaling:"
Write-Host "    kubectl get pods -n fileconversion -w"
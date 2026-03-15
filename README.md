# File Conversion Service

A file conversion API built with .NET, RabbitMQ, and Kubernetes.
Supports converting images between formats (PNG, JPEG, WebP, etc).

This project is an exploration into container orchestration, horizontal scaling,
message queueing, 

## Architecture
- **.NET API** — accepts file uploads, queues conversion jobs
- **.NET Worker** — processes conversion jobs from the queue
- **RabbitMQ** — message queue for job distribution
- **PostgreSQL** — tracks job status
- **KEDA** — autoscales workers based on queue depth

## Prerequisites
- Docker Desktop
- [Kind](https://kind.sigs.k8s.io/docs/user/quick-start/#installation)
- [kubectl](https://kubernetes.io/docs/tasks/tools/)

## Setup
1. Copy secrets file and fill in your values:
```
cp k8s/secrets.example.yaml k8s/secrets.yaml
```
2. Deploy:
```
.\scripts\deploy.ps1
```
3. Forward the API port:
```
kubectl port-forward service/api 5000:80 -n fileconversion
```

## Scripts
- `deploy.ps1` — creates cluster and deploys everything from scratch
- `teardown.ps1` — deletes the entire cluster
- `refresh.ps1` — rebuilds and redeploys app code without touching infrastructure

## API
`POST /convert?to=webp` — upload a file and convert it to the specified format
`GET /convert/{jobId}` — get the status of a conversion job
`GET /convert/{jobId}/result` — get the resulting file if the job is complete
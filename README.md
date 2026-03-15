# File Conversion Service

Distributed image conversion API + autoscaled worker pipeline in .NET, RabbitMQ, PostgreSQL, and Kubernetes (Kind).

- Built to learn scalable, reliable microservices and end-to-end Kubernetes deployment.
- Implements async upload + queue + worker processing + job state tracking.
- Includes script-driven local deploy, zero-to-n scale via KEDA, and retries for resilience.

---

## System architecture

Client → .NET API → RabbitMQ → .NET Workers (KEDA autoscaling)
                ↕                      ↕
           PostgreSQL          Shared PersistentVolume

### Why this design
- Asynchronous processing decouples upload latency from conversion workloads.
- KEDA scales workers based on queue depth (idle to zero, bursts up to 10).
- Persistent job status and durable RabbitMQ make this resilient to crashes.

---

## Quick start

```bash
git clone https://github.com/trentonpaul/file-conversion.git
cd file-conversion
cp k8s/secrets.example.yaml k8s/secrets.yaml
# fill in k8s/secrets.yaml values
.\scripts\deploy.ps1
kubectl port-forward service/api 8080:80 -n fileconversion
```

---

## API endpoints

```http
POST /api/convert?to=webp          # upload file, returns jobId
GET  /api/convert/{jobId}          # poll status: Pending/InProgress/Completed/Failed
GET  /api/convert/{jobId}/result   # download converted file
POST /api/convert/batch            # upload multiple files
POST /api/convert/batch/status     # poll many job statuses
POST /api/convert/batch/results    # get download URLs
```

## Core tech

.NET 10 | RabbitMQ | PostgreSQL + EF Core | Kubernetes (Kind) | KEDA | Docker | Polly | ImageMagick

## Validation

```powershell
kubectl get pods -n fileconversion -w  # watch API and worker scaling
```

Use `Invoke-WebRequest -Uri http://localhost:8080/ping` to confirm API health.

## What I built

- API upload + queue enqueue + job status model
- Background worker conversion with retry/ack logic
- Autoscaling with KEDA based on RabbitMQ queue depth
- Declarative Kubernetes manifests and one-command deployment scripts

## What I learned

- Kubernetes was non-obvious at first, and I discovered that deployments + services + readiness/liveness checks are what make production-like pod behavior reliable.
- Running stateful services taught me why persistent volumes matter and how to keep Postgres data safe across restarts.
- I discovered that async queue-based workflows make API latency predictable and that durable acknowledgment semantics avoid lost jobs.
- Horizontal scaling became concrete when I saw KEDA spin workers from zero to ten and then safely scale back to zero.
- I found that explicit health checks and observability dramatically reduce debugging time in distributed systems.
- Separating API, worker, and infrastructure code proved essential for testability and incremental improvements without breaking the pipeline.

## Notes

This is a learning-focused project: engineered for distributed system reliability and scaling patterns, not a commercial production service.

## Author

**Trenton Paul**  ·  [GitHub](https://github.com/trentonpaul)  ·  [LinkedIn](https://www.linkedin.com/in/trentonpaul/)

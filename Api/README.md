# Edo
The front door of all our services

## Environment variables
- CONSUL_HTTP_ADDR
- CONSUL_HTTP_TOKEN
- GITHUB_TOKEN
- HTDC_REDIS_HOST
- HTDC_VAULT_ENDPOINT
- HTDC_VAULT_TOKEN

## Services
- Redis

`docker run -p 6379:6379 --name redis -d redis`

- MongoDb

`docker run -p 27017:27017 --name mongodb -e MONGO_INITDB_ROOT_USERNAME=admin -e MONGO_INITDB_ROOT_PASSWORD=password  mongo --auth`

- Nats

`docker run -d --name nats -p 4222:4222 -p 6222:6222 -p 8222:8222 -d nats`
# ECommerce - Microsserviços .NET 8

Projeto de e-commerce construído com microsserviços em .NET 8, utilizando Clean Architecture, CQRS, Domain Events, Outbox Pattern e comunicação assíncrona via Kafka.

> **Nota:** Este projeto foi construído sobre o [dotnet-scaffold](https://github.com/seu-usuario/dotnet-scaffold), um template base com Mediator próprio, Result Pattern, Notification Pattern e Outbox Pattern já configurados.

---

## Arquitetura

```
                        ┌─────────────────┐
                        │   API Gateway   │
                        │   YARP :5000    │
                        └────────┬────────┘
                                 │
          ┌──────────────────────┼──────────────────────┐
          │                      │                      │
 ┌────────▼────────┐   ┌────────▼────────┐   ┌────────▼─────────┐
 │     Usuarios    │   │     Produtos    │   │     Pedidos      │
 │      :5001      │   │      :5002      │   │      :5003       │
 │                 │   │                 │   │                  │
 │  postgres:5433  │   │  postgres:5434  │   │   postgres:5435  │
 └─────────────────┘   └────────┬────────┘   └─────────┬────────┘
                                │                      │
                                └──────────┬───────────┘
                                           │
                                    ┌──────▼──────┐
                                    │    Kafka    │
                                    │    :9092    │
                                    └──────┬──────┘
                                           │
                                  ┌────────▼────────┐
                                  │   Serviço IA    │
                                  │     :5004       │
                                  │   (em breve)    │
                                  └─────────────────┘
```

---

## Serviços

### API Gateway (:5000)
Ponto de entrada único da aplicação usando YARP. Valida o JWT e roteia as requisições para os serviços corretos. O cliente nunca acessa os serviços diretamente.

### Usuarios (:5001)
Responsável por autenticação e gerenciamento de usuários.

- Registro de usuários
- Login com JWT
- Refresh Token

### Produtos (:5002)
Responsável pelo catálogo e controle de estoque.

- CRUD de produtos
- Controle de estoque
- Filtragem por categoria
- Consome evento `PedidoCriado` do Kafka para atualizar estoque automaticamente

### Pedidos (:5003)
Responsável pelo gerenciamento de pedidos.

- Criação de pedidos
- Cancelamento de pedidos
- Histórico de pedidos por usuário
- Publica evento `PedidoCriado` no Kafka via Outbox Pattern

### Serviço IA (em breve)
Consultas em linguagem natural aos dados do sistema usando LLaMA via Ollama.

---

## Comunicação entre serviços

### Síncrona (HTTP)
Usada quando a resposta é necessária imediatamente.

### Assíncrona (Kafka)
Usada para eventos entre serviços. Garante consistência eventual sem acoplamento direto.

**Fluxo de um pedido:**
```
Cliente cria pedido via Gateway
      ↓
ServicoPedidos salva pedido + PedidoCriadoEvent no Outbox (mesma transação)
      ↓
OutboxProcessor publica PedidoCriadoEvent no Kafka (tópico: pedidocriado)
      ↓
PedidoCriadoConsumer do ServicoProdutos consome o evento
      ↓
Estoque atualizado automaticamente
```

**Tópicos Kafka:**
- `pedidocriado` → publicado pelo Pedidos, consumido pelo Produtos

---

## Stack

- **.NET 8** — framework principal
- **PostgreSQL** — banco de dados (um por serviço)
- **Kafka** — mensageria assíncrona
- **YARP** — API Gateway / Reverse Proxy
- **Entity Framework Core** — ORM
- **JWT** — autenticação
- **Docker** — infraestrutura local
- **Ollama + LLaMA** — IA local (em breve)

---

## Pré-requisitos

- .NET 8 SDK
- Docker Desktop
- Postman (para testar os endpoints)

---

## Como rodar

### 1. Clone o repositório

```bash
git clone https://github.com/seu-usuario/ecommerce-microservices.git
cd ecommerce-microservices
```

### 2. Suba a infraestrutura

```bash
docker-compose up -d
```

Isso vai subir:
- PostgreSQL para cada serviço (portas 5433, 5434, 5435)
- Kafka + Zookeeper (porta 9092)
- Kafka UI (http://localhost:8080)
- pgAdmin (http://localhost:5051)

Aguarde cerca de 30 segundos para todos os containers estarem prontos.

### 3. Configure o pgAdmin (opcional)

Acesse `http://localhost:5051` e adicione os servidores:

| Servidor | Host | Port | Database |
|---|---|---|---|
| Usuarios | postgres-usuarios | 5432 | ecommerce_usuarios |
| Produtos | postgres-produtos | 5432 | ecommerce_produtos |
| Pedidos | postgres-pedidos | 5432 | ecommerce_pedidos |

Usuário e senha: `postgres` / `postgres`

### 4. Rode as migrations

```bash
# Usuarios
cd Usuarios/ECommerce.Usuarios.Infrastructure
dotnet ef database update

# Produtos
cd ../../Produtos/ECommerce.Produtos.Infrastructure
dotnet ef database update

# Pedidos
cd ../../Pedidos/ECommerce.Pedidos.Infrastructure
dotnet ef database update

cd ../..
```

### 5. Rode os serviços

No Visual Studio, clique com o botão direito na solução → **Configurar Projetos de Inicialização** → **Vários projetos de inicialização** → marca todos como **Iniciar**.

Ou pelo terminal (um por janela):

```bash
# Terminal 1 - Gateway
dotnet run --project Gateway/ECommerce.Gateway/ECommerce.Gateway.csproj --launch-profile https

# Terminal 2 - Usuarios
dotnet run --project Usuarios/ECommerce.Usuarios.Api/ECommerce.Usuarios.Api.csproj --launch-profile https

# Terminal 3 - Produtos
dotnet run --project Produtos/ECommerce.Produtos.Api/ECommerce.Produtos.Api.csproj --launch-profile https

# Terminal 4 - Pedidos
dotnet run --project Pedidos/ECommerce.Pedidos.Api/ECommerce.Pedidos.Api.csproj --launch-profile https
```

---

## Testando com Postman

Todas as requisições passam pelo Gateway na porta `5100` (HTTP) ou `5000` (HTTPS).

> Dica: No Postman desabilite a verificação SSL em **Settings → General → SSL certificate verification → Off**

### 1. Registrar usuário

```
POST http://localhost:5100/api/usuario/registrar
Content-Type: application/json

{
  "nome": "João Silva",
  "email": "joao@email.com",
  "senha": "123456"
}
```

### 2. Login

```
POST http://localhost:5100/api/usuario/login
Content-Type: application/json

{
  "email": "joao@email.com",
  "senha": "123456"
}
```

Copie o `accessToken` retornado e use nos próximos requests no header:
```
Authorization: Bearer {accessToken}
```

### 3. Criar produto

```
POST http://localhost:5100/api/produtos
Authorization: Bearer {token}
Content-Type: application/json

{
  "nome": "Notebook Dell",
  "descricao": "Notebook Dell Inspiron 15",
  "preco": 3500.00,
  "estoque": 10,
  "categoria": "Informatica"
}
```

### 4. Listar produtos

```
GET http://localhost:5100/api/produtos
Authorization: Bearer {token}
```

### 5. Criar pedido

```
POST http://localhost:5100/api/pedidos
Authorization: Bearer {token}
Content-Type: application/json

{
  "itens": [
    {
      "produtoId": "{id-do-produto}",
      "nomeProduto": "Notebook Dell",
      "precoUnitario": 3500.00,
      "quantidade": 1
    }
  ]
}
```

Após criar o pedido, aguarde alguns segundos e liste os produtos novamente. O estoque será atualizado automaticamente via Kafka.

### 6. Listar meus pedidos

```
GET http://localhost:5100/api/pedidos
Authorization: Bearer {token}
```

### 7. Cancelar pedido

```
DELETE http://localhost:5100/api/pedidos/{id}
Authorization: Bearer {token}
```

### 8. Refresh Token

```
POST http://localhost:5100/api/usuario/refresh-token
Content-Type: application/json

{
  "accessToken": "{accessToken}",
  "refreshToken": "{refreshToken}"
}
```

---

## Monitoramento

### Kafka UI
Acesse `http://localhost:8080` para visualizar:
- Tópicos e mensagens publicadas
- Consumer groups e offsets
- Lag dos consumers

### Swaggers (acesso direto sem Gateway)
- Usuarios: https://localhost:5001/swagger
- Produtos: https://localhost:5002/swagger
- Pedidos: https://localhost:5003/swagger

---

## Estrutura de cada serviço

```
ECommerce.{Servico}
├── ECommerce.{Servico}.Api           → controllers, filtros, configuração
├── ECommerce.{Servico}.Application   → commands, queries, handlers
├── ECommerce.{Servico}.Domain        → entidades, eventos, interfaces
└── ECommerce.{Servico}.Infrastructure→ EF Core, repositórios, Kafka, Outbox
```

---

## Shared

```
Shared
├── ECommerce.Mediator    → Mediator próprio com CQRS, pipeline behaviors,
│                           Result Pattern e Notification Pattern
└── ECommerce.Contracts   → Contratos compartilhados entre serviços (eventos Kafka)
```

---

## Pendências

- [ ] Campo de role no registro de usuários
- [ ] Serviço de IA com LLaMA + Ollama
- [ ] Testes unitários

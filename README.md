# ECommerce - Microsserviços .NET 8

Projeto de e-commerce construído com microsserviços em .NET 8, utilizando Clean Architecture, CQRS, Domain Events, Outbox Pattern e comunicação assíncrona via Kafka.

> **Nota:** Este projeto foi construído sobre o [dotnet-scaffold](https://github.com/seu-usuario/dotnet-scaffold), um template base com Mediator próprio, Result Pattern, Notification Pattern e Outbox Pattern já configurados.

---

## Arquitetura

```
                        ┌─────────────────┐
                        │   API Gateway   │
                        │     (YARP)      │
                        └────────┬────────┘
                                 │
          ┌──────────────────────┼──────────────────────┐
          │                      │                      │
 ┌────────▼────────┐   ┌────────▼────────┐   ┌────────▼────────┐
 │    Usuarios     │   │    Produtos     │   │    Pedidos      │
 │    :5001        │   │    :5002        │   │    :5003        │
 │                 │   │                 │   │                 │
 │ postgres:5433   │   │ postgres:5434   │   │ postgres:5435   │
 └─────────────────┘   └────────┬────────┘   └────────┬────────┘
                                │                      │
                                └──────────┬───────────┘
                                           │
                                    ┌──────▼──────┐
                                    │    Kafka    │
                                    │   :9092     │
                                    └──────┬──────┘
                                           │
                                  ┌────────▼────────┐
                                  │   Serviço IA    │
                                  │    :5004        │
                                  │  (em breve)     │
                                  └─────────────────┘
```

## Serviços

### Usuarios (:5001)
Responsável por autenticação e gerenciamento de usuários.

- Registro de usuários
- Login com JWT
- Refresh Token
- Roles (Admin, User)

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

### API Gateway (em breve)
Ponto de entrada único da aplicação usando YARP.

- Roteamento para os serviços
- Validação do JWT centralizada

### Serviço IA (em breve)
Consultas em linguagem natural aos dados do sistema usando LLaMA via Ollama.

- "Quantos pedidos foram feitos essa semana?"
- "Quais produtos estão com estoque baixo?"
- "Me retorne todos os pedidos do usuário X"

---

## Comunicação entre serviços

### Síncrona (HTTP)
Usada quando a resposta é necessária imediatamente.

### Assíncrona (Kafka)
Usada para eventos entre serviços. Garante consistência eventual sem acoplamento direto.

**Fluxo de um pedido:**
```
Cliente cria pedido
      ↓
ServicoPedidos salva pedido + PedidoCriadoEvent no Outbox (mesma transação)
      ↓
OutboxProcessor publica PedidoCriadoEvent no Kafka
      ↓
ServicoProdutos consome o evento
      ↓
Estoque atualizado automaticamente
```

**Tópicos Kafka:**
- `pedidocriado` → publicado pelo Pedidos, consumido pelo Produtos
- `pedidocancelado` → publicado pelo Pedidos (em breve)

---

## Stack

- **.NET 8** — framework principal
- **PostgreSQL** — banco de dados (um por serviço)
- **Kafka** — mensageria assíncrona
- **Entity Framework Core** — ORM
- **JWT** — autenticação
- **Docker** — infraestrutura local
- **YARP** — API Gateway (em breve)
- **Ollama + LLaMA** — IA local (em breve)

---

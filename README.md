# ECommerce - Microsserviços .NET 8

Projeto de e-commerce construído com microsserviços em .NET 8, utilizando Clean Architecture, CQRS, Domain Events, Outbox Pattern, comunicação assíncrona via Kafka e consultas em linguagem natural via IA local.

> **Nota:** Este projeto foi construído sobre o [dotnet-scaffold](https://github.com/seu-usuario/dotnet-scaffold), um template base com Mediator próprio, Result Pattern, Notification Pattern e Outbox Pattern já configurados.

## Frontend

A interface web deste projeto está disponível em [alissonCandatem/ECommerce-Web](https://github.com/alissonCandatem/ECommerce-Web), construída com Next.js 16, TypeScript e shadcn/ui.

---

## Arquitetura

```
                        ┌─────────────────┐
                        │   API Gateway   │
                        │  YARP :5100     │
                        └────────┬────────┘
                                 │
          ┌──────────────────────┼──────────────────────┬──────────────────┐
          │                      │                      │                  │
 ┌────────▼────────┐   ┌────────▼────────┐   ┌────────▼────────┐ ┌───────▼────────┐
 │    Usuarios     │   │    Produtos     │   │     Pedidos     │ │       IA       │
 │     :5001       │   │     :5002       │   │     :5003       │ │     :5004      │
 │                 │   │                 │   │                 │ │                │
 │  postgres:5433  │   │  postgres:5434  │   │  postgres:5435  │ │  postgres:5436 │
 └─────────────────┘   └────────┬────────┘   └────────┬────────┘ └───────┬────────┘
                                │                     │                  │
                                └──────────┬──────────┘          ┌───────▼────────┐
                                           │                     │     Ollama     │
                                    ┌──────▼──────┐              │     :11434     │
                                    │    Kafka    │              │                │
                                    │   :9092     │              │ qwen2.5-coder  │
                                    └─────────────┘              │ nomic-embed    │
                                                                 └────────────────┘
```

---

## Serviços

### API Gateway (:5000)
Ponto de entrada único da aplicação usando YARP. Valida o JWT e roteia as requisições para os serviços corretos. O cliente nunca acessa os serviços diretamente.

### Usuarios (:5001)
Responsável por autenticação e gerenciamento de usuários.
- Registro de usuários (role padrão: User, Admin atribuído manualmente)
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

### Serviço IA (:5004)
Consultas em linguagem natural usando RAG (Retrieval Augmented Generation) com LLM local via Ollama.

**Como funciona:**
```
Usuário pergunta em linguagem natural
      ↓
Gera embedding da pergunta (nomic-embed-text)
      ↓
Busca schemas relevantes no pgvector por similaridade semântica
      ↓
Monta prompt com contexto dos schemas encontrados
      ↓
qwen2.5-coder gera o SQL
      ↓
Executa no banco via PostgreSQL FDW (suporta JOINs entre bancos)
      ↓
qwen2.5-coder formata a resposta em linguagem natural
```

**Exemplos de consultas:**
- "quantos usuários estão cadastrados?"
- "me traga os usuários e seus pedidos e os itens"
- "quantos pedidos foram feitos no último mês?"
- "quais produtos estão com estoque baixo?"

---

## Comunicação entre serviços

### Assíncrona (Kafka)
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
- **pgvector** — extensão PostgreSQL para busca vetorial (RAG)
- **PostgreSQL FDW** — Foreign Data Wrapper para JOINs entre bancos
- **Kafka** — mensageria assíncrona
- **YARP** — API Gateway / Reverse Proxy
- **Entity Framework Core** — ORM
- **JWT** — autenticação
- **Docker** — infraestrutura local
- **Ollama** — servidor de LLMs local
- **qwen2.5-coder** — geração de SQL e respostas
- **nomic-embed-text** — geração de embeddings para RAG

---

## Pré-requisitos

- .NET 8 SDK
- Docker Desktop com suporte a GPU NVIDIA (recomendado)
- Postman (para testar os endpoints)
- Placa de vídeo NVIDIA com pelo menos 6GB de VRAM (recomendado para melhor performance)

---

## Como rodar

### 1. Clone o repositório

```bash
git clone https://github.com/seu-usuario/ecommerce-microservices.git
cd ecommerce-microservices
```

### 2. Configure a variável de ambiente do Ollama

Para salvar os modelos fora do disco C (recomendado):

1. Abra **Painel de Controle → Sistema → Configurações avançadas do sistema → Variáveis de Ambiente**
2. Em **Variáveis do sistema** clique em **Novo**
3. Nome: `OLLAMA_MODELS`
4. Valor: `E:\Ollama\models` (ou o caminho de sua preferência)

### 3. Suba a infraestrutura

```bash
docker-compose up -d
```

Isso vai subir:
- PostgreSQL para cada serviço (portas 5433, 5434, 5435, 5436)
- Kafka + Zookeeper (porta 9092)
- Kafka UI (http://localhost:8080)
- pgAdmin (http://localhost:5051)
- Ollama com GPU (porta 11434)

Aguarde cerca de 30 segundos para todos os containers estarem prontos.

### 4. Baixe os modelos de IA

```bash
# modelo de embeddings para RAG
docker exec -it ecommerce-ollama ollama pull nomic-embed-text

# modelo para geração de SQL e respostas
docker exec -it ecommerce-ollama ollama pull qwen2.5-coder
```

### 5. Crie os bancos de dados

Acesse o pgAdmin em `http://localhost:5051` e adicione os servidores:

| Servidor | Host | Port | Database |
|---|---|---|---|
| Usuarios | postgres-usuarios | 5432 | ecommerce_usuarios |
| Produtos | postgres-produtos | 5432 | ecommerce_produtos |
| Pedidos | postgres-pedidos | 5432 | ecommerce_pedidos |
| IA | postgres-ia | 5432 | ecommerce_ia |

Usuário e senha: `postgres` / `postgres`

Crie os bancos executando em cada servidor:
```sql
CREATE DATABASE ecommerce_usuarios;
CREATE DATABASE ecommerce_produtos;
CREATE DATABASE ecommerce_pedidos;
CREATE DATABASE ecommerce_ia;
```

### 6. Rode as migrations

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

> O banco `ecommerce_ia` é configurado automaticamente pelo `FdwSetupService` na inicialização do serviço de IA.

### 7. Rode os serviços

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

# Terminal 5 - IA
dotnet run --project IA/ECommerce.IA.Api/ECommerce.IA.Api.csproj --launch-profile https
```

### 8. Indexe os schemas

Após todos os serviços estarem rodando, indexe os schemas para o RAG funcionar:

```
POST http://localhost:5100/api/ia/indexar
Authorization: Bearer {token}
```

> Esta etapa só precisa ser repetida quando houver mudanças no schema do banco de dados.

---

## Testando com Postman

Todas as requisições passam pelo Gateway na porta `5100` (HTTP) ou `5000` (HTTPS).

> Dica: No Postman desabilite a verificação SSL em **Settings → General → SSL certificate verification → Off**

### Autenticação

```
POST http://localhost:5100/api/usuario/registrar
Content-Type: application/json

{
  "nome": "João Silva",
  "email": "joao@email.com",
  "senha": "123456"
}
```

```
POST http://localhost:5100/api/usuario/login
Content-Type: application/json

{
  "email": "joao@email.com",
  "senha": "123456"
}
```

### Produtos

```
POST http://localhost:5100/api/produtos
Authorization: Bearer {token}

{
  "nome": "Notebook Dell",
  "descricao": "Notebook Dell Inspiron 15",
  "preco": 3500.00,
  "estoque": 10,
  "categoria": "Informatica"
}
```

```
GET http://localhost:5100/api/produtos
Authorization: Bearer {token}
```

### Pedidos

```
POST http://localhost:5100/api/pedidos
Authorization: Bearer {token}

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

Após criar o pedido, aguarde alguns segundos — o estoque será atualizado automaticamente via Kafka.

### Consultas com IA

```
POST http://localhost:5100/api/ia/consultar
Authorization: Bearer {token}

{
  "pergunta": "quantos usuários estão cadastrados?"
}
```

```
POST http://localhost:5100/api/ia/consultar
Authorization: Bearer {token}

{
  "pergunta": "me traga os usuários e seus pedidos e os itens"
}
```

```
POST http://localhost:5100/api/ia/consultar
Authorization: Bearer {token}

{
  "pergunta": "quantos pedidos foram feitos no último mês?"
}
```

---

## Monitoramento

### Kafka UI
Acesse `http://localhost:8080` para visualizar tópicos, mensagens, consumer groups e lag.

### Swaggers (acesso direto sem Gateway)
- Usuarios: https://localhost:5001/swagger
- Produtos: https://localhost:5002/swagger
- Pedidos: https://localhost:5003/swagger
- IA: https://localhost:5004/swagger

---

## Estrutura do projeto

```
ECommerce
├── Gateway
│   └── ECommerce.Gateway              → YARP Reverse Proxy
├── Usuarios
│   ├── ECommerce.Usuarios.Api
│   ├── ECommerce.Usuarios.Application
│   ├── ECommerce.Usuarios.Domain
│   └── ECommerce.Usuarios.Infrastructure
├── Produtos
│   ├── ECommerce.Produtos.Api
│   ├── ECommerce.Produtos.Application
│   ├── ECommerce.Produtos.Domain
│   └── ECommerce.Produtos.Infrastructure
├── Pedidos
│   ├── ECommerce.Pedidos.Api
│   ├── ECommerce.Pedidos.Application
│   ├── ECommerce.Pedidos.Domain
│   └── ECommerce.Pedidos.Infrastructure
├── IA
│   └── ECommerce.IA.Api               → RAG + FDW + Ollama
├── Shared
│   ├── ECommerce.Mediator             → Mediator próprio com CQRS e pipeline behaviors
│   └── ECommerce.Contracts            → Contratos compartilhados entre serviços
└── docker-compose.yml
```

---

## Pendências
- [ ] Testes unitários
- [ ] Feedback de falha de estoque via evento Kafka (PedidoFalhouEvent)

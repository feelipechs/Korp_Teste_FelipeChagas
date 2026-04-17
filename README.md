Processo Seletivo Korp ERP - Estágio Dev

# NFe System

Sistema de emissão de Notas Fiscais baseado em microsserviços, com separação de responsabilidades entre gerenciamento de estoque e faturamento.

## Tecnologias
### Backend
- ASP.NET Core 10 (Web API)
- Entity Framework Core 10 (Code First)
- PostgreSQL 16
- Microsoft.Extensions.Http.Resilience (retry e circuit breaker)
- Docker / Docker Compose
- Frontend
- Angular 21
- Angular Material
- RxJS

## Arquitetura
### Frontend (Angular) :4200
```text
├── inventory-service (produtos/estoque) :5001
│     └── postgres-inventory :5433
│
└── billing-service (notas fiscais) :5002
      └── postgres-billing :5434
```

### Como executar
1. Subir os serviços backend
```bash
docker compose up -d
```

2. Subir o frontend
``` bash
cd frontend
npm install
ng serve
```

3. Acessos

    ```Frontend: http://localhost:4200```

    ```Inventory API: http://localhost:5001```

    ```Billing API: http://localhost:5002```

## Funcionalidades
- Cadastro de produtos com código e estoque
- Listagem de produtos
- Criação de notas fiscais com múltiplos itens
- Numeração automática de notas fiscais
- Impressão de notas com alteração de status
- Dedução de estoque no serviço de inventário
- Bloqueio de reprocessamento de notas fechadas

## Requisitos técnicos
### Idempotência

O endpoint de criação de notas suporta o header:

```Idempotency-Key```

Evita criação duplicada de notas em chamadas repetidas
Garante consistência mesmo em cenários de concorrência
Requisições duplicadas retornam a mesma resposta já criada

### Concorrência

O sistema lida com concorrência através de:

- constraint única no banco de dados
- verificação de existência antes da criação
- tratamento de exceções em casos de conflito

### Resiliência entre serviços

Comunicação entre microsserviços utiliza:

- retry automático
- circuit breaker

via Microsoft.Extensions.Http.Resilience (implementado no billing-service para comunicação com inventory-service)

### Simulação de falha
``` bash
docker compose stop inventory-service
```

o sistema retorna erro tratado no frontend

```bash
docker compose start inventory-service
```

o sistema volta a funcionar normalmente

## Detalhamento técnico
- Angular
- ngOnInit para carregamento inicial de dados
- RxJS via HttpClient e Observables
- Angular Material para interface (tabelas, formulários, chips, snackbar, spinner)
- Backend (.NET)
- ASP.NET Core 10 Web API
- Entity Framework Core (Code First)
- LINQ para consultas e projeções
- Middleware global para tratamento de erros
- LINQ

Utilizado para:

- filtragem (Where)
- projeções (Select)
- consultas pontuais (FirstOrDefaultAsync)
- carregamento de relacionamentos (Include)
- agregações (MaxAsync)
- Tratamento de erros
- Middleware global padroniza exceções não tratadas
- Erros retornados em formato JSON consistente
- Frontend captura erros via RxJS e exibe feedback ao usuário

## Observações
- Projeto focado em arquitetura de microsserviços e comunicação entre serviços
- Ênfase em consistência de dados, concorrência e resiliência
- Interface construída com foco em simplicidade e fluxo de uso
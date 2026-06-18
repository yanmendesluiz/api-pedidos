# API de Gestão de Pedidos

Solução em **.NET 8** para gerenciamento de clientes, produtos, estoque, pedidos e histórico de status.

## Como executar localmente

Pré-requisitos:

- .NET SDK 8
- Git

```bash
git clone <url-do-seu-repositorio>
cd order-api-challenge
dotnet restore
dotnet run --project src/Pedidos.Api
```

A aplicação cria o banco automaticamente ao iniciar usando `EnsureCreated()`. O banco padrão é SQLite no arquivo `pedidos.db`.

Swagger:

```text
https://localhost:<porta>/swagger
```

## Como executar os testes

```bash
dotnet test
```

Os testes priorizam regras de negócio críticas: criação de pedidos, baixa transacional de estoque, bloqueio de cliente/produto inativo, falta de estoque, preservação de preço histórico, transições de status e retorno de estoque no cancelamento.

## Tecnologias

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- Swagger/OpenAPI
- xUnit

## Estrutura

```text
src/Pedidos.Api
  Controllers   Endpoints REST
  Data          DbContext e mapeamento EF Core
  Domain        Entidades e enum de status
  Models        DTOs, paginação e erros
  Services      Regras de negócio
  Utils         Datas e validação de CPF/CNPJ

tests/Pedidos.Api.Tests
  Testes automatizados das principais regras
```

## Endpoints

### Clientes

- `POST /api/clientes`
- `GET /api/clientes?page=1&pageSize=10`
- `GET /api/clientes/{id}`
- `PATCH /api/clientes/{id}/status`

### Produtos

- `POST /api/produtos`
- `GET /api/produtos?page=1&pageSize=10`
- `GET /api/produtos/{id}`
- `PUT /api/produtos/{id}`
- `PATCH /api/produtos/{id}/status`
- `PATCH /api/produtos/{id}/estoque`
- `PATCH /api/produtos/{id}/preco`

### Pedidos

- `POST /api/pedidos`
- `GET /api/pedidos?page=1&pageSize=10`
- `GET /api/pedidos/{id}`
- `PATCH /api/pedidos/{id}/status`

## Paginação

Endpoints de listagem aceitam:

- `page`: página atual, padrão `1`.
- `pageSize`: tamanho da página, padrão `10`, máximo `100`.

Resposta:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 10,
  "totalItems": 0,
  "totalPages": 0
}
```

## Regras de clientes

- Nome, e-mail e documento são obrigatórios.
- E-mail precisa ter formato válido.
- Não permite dois clientes ativos com o mesmo e-mail.
- Não permite dois clientes ativos com o mesmo documento.
- CPF/CNPJ, quando identificados por 11 ou 14 dígitos, são validados algoritmicamente.
- Cliente inativo continua consultável, mas não cria pedido.

## Regras de produtos

- Nome obrigatório.
- Preço maior que zero.
- Estoque não negativo.
- Produto inativo continua consultável, mas não entra em novos pedidos.
- Produto já usado em pedido não é removido fisicamente; o endpoint implementado apenas ativa/inativa.
- Alteração de preço não muda pedidos antigos, pois o item guarda `PrecoUnitario` no momento da criação.

## Estratégia de estoque e concorrência

A criação de pedido e a baixa de estoque acontecem dentro de uma transação do EF Core.

Fluxo:

1. Valida cliente ativo.
2. Carrega todos os produtos do pedido.
3. Valida produto existente, ativo e com estoque suficiente.
4. Debita estoque dos produtos.
5. Calcula preço dos itens e total do pedido no servidor.
6. Salva pedido, itens e histórico inicial.
7. Confirma a transação.

Se qualquer item falhar, a transação não é confirmada e nenhum estoque é debitado parcialmente.

Para produção com alta concorrência, a recomendação é usar banco como PostgreSQL ou SQL Server com isolamento adequado e controle otimista/pessimista para estoque, por exemplo `rowversion`, `xmin`, lock por linha ou update condicional `WHERE estoque >= quantidade`.

## Fluxo de status

Status:

- `Criado`
- `Pago`
- `Enviado`
- `Cancelado`

Transições permitidas:

- `Criado -> Pago`
- `Pago -> Enviado`
- `Criado -> Cancelado`
- `Pago -> Cancelado`

A transição `Pago -> Cancelado` foi incluída por interpretação do requisito de retorno de estoque quando o pedido ainda não foi enviado. Pedido `Enviado` não pode ser cancelado. Pedido `Cancelado` não pode mudar de status.

Toda alteração válida gera histórico. Status igual ao atual é tratado como erro `400` e não gera histórico, para evitar registros duplicados sem mudança real.

## Histórico de status

A criação do pedido gera um registro inicial no histórico com:

- `StatusAnterior = null`
- `NovoStatus = Criado`
- `Motivo = "Criação do pedido"`

Alterações inválidas não geram histórico.

## Valores monetários

Foi usado `decimal` com precisão `18,2`, por ser o tipo adequado para valores monetários em .NET, evitando problemas de ponto flutuante de `double`/`float`.

Arredondamento:

- Preços e totais são arredondados para 2 casas decimais.
- Estratégia: `MidpointRounding.ToEven`, conhecida como arredondamento bancário.

## Datas, UTC e America/Sao_Paulo

As entidades persistem datas em UTC nos campos `CriadoEmUtc`, `AtualizadoEmUtc` e `AlteradoEmUtc`.

As respostas convertem as datas para o fuso `America/Sao_Paulo` usando `TimeZoneInfo`.

Como a API atual não recebe datas de criação/alteração vindas do cliente, não há confiança em timestamps externos para auditoria.

## Validação

A validação foi feita em duas camadas:

- DTOs com DataAnnotations para contratos básicos.
- Services para regras de negócio e validações dependentes do banco.

A API não aceita `PrecoUnitario` nem `ValorTotal` enviados pelo cliente na criação do pedido. Esses valores são calculados exclusivamente pela aplicação.

## Persistência

Foi usado Entity Framework Core com SQLite para facilitar execução local.

A escolha mantém simplicidade para avaliação técnica, mas a arquitetura permite trocar o provider para SQL Server ou PostgreSQL com baixo impacto.

## Pontos fora do escopo e melhorias futuras

- Autenticação/autorização.
- Migrations geradas via CLI no repositório. A aplicação está preparada para EF Core, mas neste pacote não há SDK disponível para gerar migrations no ambiente de montagem.
- Controle avançado de concorrência por banco de produção.
- Observabilidade com logs estruturados, tracing e métricas.
- Pipeline CI/CD.
- Testes de integração HTTP ponta a ponta.
- Filtros avançados nas listagens.

## Histórico de commits sugerido

Este pacote já foi montado com commits incrementais no Git local. Caso você extraia o ZIP preservando a pasta `.git`, basta adicionar o remoto e fazer push.

```bash
git log --oneline
```

Para publicar em um repositório vazio:

```bash
git remote add origin https://github.com/<seu-usuario>/<seu-repositorio>.git
git branch -M main
git push -u origin main
```

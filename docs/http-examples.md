# Exemplos HTTP

## Criar cliente

```http
POST /api/clientes
Content-Type: application/json

{
  "nome": "Maria Silva",
  "email": "maria@email.com",
  "documento": "39053344705"
}
```

## Criar produto

```http
POST /api/produtos
Content-Type: application/json

{
  "nome": "Notebook",
  "descricao": "Notebook 16GB RAM",
  "preco": 4500.90,
  "estoqueDisponivel": 10
}
```

## Criar pedido

```http
POST /api/pedidos
Content-Type: application/json

{
  "clienteId": "00000000-0000-0000-0000-000000000000",
  "itens": [
    {
      "produtoId": "00000000-0000-0000-0000-000000000000",
      "quantidade": 2
    }
  ]
}
```

## Alterar status

```http
PATCH /api/pedidos/00000000-0000-0000-0000-000000000000/status
Content-Type: application/json

{
  "novoStatus": "Pago",
  "motivo": "Pagamento confirmado"
}
```

# GeoPharma PHP

Sistema pessoal de inteligência geográfica para prospecção de farmácias.

Tecnologias: PHP 8.2+, MySQL, PDO, AdminLTE 4, Bootstrap 5 e Bootstrap Icons.

## Executar

No VS Code, pressione `Ctrl+Shift+P`, escolha `Tasks: Run Task` e execute `▶ Iniciar GeoPharma`.

Acesse: http://localhost:8090

## Banco de dados

Copie `config/database.local.example.php` para `config/database.local.php` e
informe as credenciais locais. O arquivo com a senha não é versionado.

Para criar ou atualizar todas as tabelas:

```bash
php database/migrate.php
```

Cada mudança estrutural deve ser adicionada como uma nova migração em
`database/migrations`. Migrações já executadas não devem ser alteradas.

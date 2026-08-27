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

## Padrão obrigatório dos CRUDs

Todos os módulos de cadastro devem seguir a mesma identidade visual do CRUD de
usuários, usando componentes do AdminLTE e Bootstrap já incluídos no projeto.

- página de listagem em card com contorno verde, título com ícone e botão de novo cadastro;
- tabela no computador e no celular, com rolagem horizontal interna quando necessário;
- formulários responsivos, organizados em card e colunas que ocupam a largura total no celular;
- ações de editar e excluir com ícones, confirmação antes da exclusão e proteção CSRF;
- mensagens padronizadas de sucesso, aviso, validação e erro;
- spinner e bloqueio temporário em todos os botões que enviam formulários;
- controles com área de toque adequada e validação no servidor;
- testes obrigatórios nas larguras de 360 px e 390 px antes da publicação.
- listagens limitadas a 10 registros por página, com paginação ao ultrapassar esse total;
- máscaras de exibição para documentos, telefones e demais dados formatáveis.

## Cadastro de clientes

O cadastro de clientes trabalha diretamente com CNPJ e não possui campos de
tipo de pessoa ou WhatsApp. Ao completar um CNPJ, o sistema busca os dados públicos
na BrasilAPI e preenche automaticamente os campos cadastrais disponíveis. A
latitude e a longitude são obtidas automaticamente pela geolocalização do CEP. O
usuário deve conferir e pode editar as informações antes de salvar.

<?php

declare(strict_types=1);

require_once dirname(__DIR__) . '/app/bootstrap.php';

$pdo = Database::connection();
$pdo->exec(
    'CREATE TABLE IF NOT EXISTS schema_migrations (
        id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
        migration VARCHAR(255) NOT NULL UNIQUE,
        executada_em TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci'
);

$executadas = $pdo
    ->query('SELECT migration FROM schema_migrations')
    ->fetchAll(PDO::FETCH_COLUMN);
$executadas = array_fill_keys($executadas, true);

$arquivos = glob(__DIR__ . '/migrations/*.php') ?: [];
sort($arquivos, SORT_STRING);

$aplicadas = 0;
foreach ($arquivos as $arquivo) {
    $nome = basename($arquivo);
    if (isset($executadas[$nome])) {
        continue;
    }

    $migration = require $arquivo;
    if (!is_callable($migration)) {
        throw new RuntimeException("Migração inválida: {$nome}");
    }

    $migration($pdo);
    $registro = $pdo->prepare(
        'INSERT INTO schema_migrations (migration) VALUES (:migration)'
    );
    $registro->execute(['migration' => $nome]);

    echo "Aplicada: {$nome}" . PHP_EOL;
    $aplicadas++;
}

echo $aplicadas === 0
    ? 'Banco atualizado. Nenhuma migração pendente.' . PHP_EOL
    : "Banco atualizado. Migrações aplicadas: {$aplicadas}." . PHP_EOL;

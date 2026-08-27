<?php
declare(strict_types=1);
return static function (PDO $pdo): void {
    $pdo->exec("CREATE TABLE clientes (
        id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
        tipo ENUM('juridica','fisica') NOT NULL DEFAULT 'juridica',
        documento VARCHAR(14) NOT NULL,
        razao_social VARCHAR(180) NOT NULL,
        nome_fantasia VARCHAR(180) NULL,
        telefone VARCHAR(20) NULL,
        email VARCHAR(190) NULL,
        cep VARCHAR(8) NULL,
        logradouro VARCHAR(180) NULL,
        numero VARCHAR(20) NULL,
        complemento VARCHAR(100) NULL,
        bairro VARCHAR(100) NULL,
        cidade VARCHAR(100) NULL,
        estado CHAR(2) NULL,
        latitude DECIMAL(10,7) NULL,
        longitude DECIMAL(10,7) NULL,
        observacoes TEXT NULL,
        ativo TINYINT(1) NOT NULL DEFAULT 1,
        criado_em TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        atualizado_em TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
        UNIQUE KEY clientes_documento_unique (documento),
        KEY clientes_nome_index (razao_social,nome_fantasia),
        KEY clientes_localizacao_index (cidade,estado),
        KEY clientes_ativo_index (ativo)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci");
};

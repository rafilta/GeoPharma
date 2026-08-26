<?php

declare(strict_types=1);

return static function (PDO $pdo): void {
    $pdo->exec(
        "CREATE TABLE usuarios (
            id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
            nome VARCHAR(120) NOT NULL,
            email VARCHAR(190) NOT NULL,
            senha_hash VARCHAR(255) NOT NULL,
            perfil ENUM('administrador', 'gestor', 'vendedor') NOT NULL DEFAULT 'vendedor',
            ativo TINYINT(1) NOT NULL DEFAULT 1,
            ultimo_acesso_em DATETIME NULL,
            criado_em TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            atualizado_em TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            UNIQUE KEY usuarios_email_unique (email),
            KEY usuarios_ativo_perfil_index (ativo, perfil)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci"
    );
};

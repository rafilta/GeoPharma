<?php

declare(strict_types=1);

return static function (PDO $pdo): void {
    $pdo->exec(
        'ALTER TABLE usuarios
            ADD COLUMN login VARCHAR(60) NULL AFTER nome'
    );

    $pdo->exec(
        "UPDATE usuarios
         SET login = CONCAT('usuario', id)
         WHERE login IS NULL OR login = ''"
    );

    $pdo->exec(
        'ALTER TABLE usuarios
            MODIFY COLUMN login VARCHAR(60) NOT NULL,
            ADD UNIQUE KEY usuarios_login_unique (login)'
    );
};

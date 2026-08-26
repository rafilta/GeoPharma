<?php

declare(strict_types=1);

return static function (PDO $pdo): void {
    $senhaHash = password_hash('admin', PASSWORD_DEFAULT);
    if ($senhaHash === false) {
        throw new RuntimeException('Não foi possível proteger a senha inicial.');
    }

    $comando = $pdo->prepare(
        "INSERT INTO usuarios (
            nome,
            login,
            email,
            senha_hash,
            perfil,
            ativo
        ) VALUES (
            :nome,
            :login,
            :email,
            :senha_hash,
            'administrador',
            1
        )
        ON DUPLICATE KEY UPDATE
            nome = VALUES(nome),
            senha_hash = VALUES(senha_hash),
            perfil = 'administrador',
            ativo = 1"
    );

    $comando->execute([
        'nome' => 'Administrador',
        'login' => 'admin',
        'email' => 'admin@geopharma.local',
        'senha_hash' => $senhaHash,
    ]);
};

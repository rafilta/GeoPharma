<?php
declare(strict_types=1);
return static function(PDO $pdo):void{
    $pdo->exec("CREATE TABLE visitas (
        id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
        cliente_id BIGINT UNSIGNED NOT NULL,
        usuario_id BIGINT UNSIGNED NOT NULL,
        resultado VARCHAR(40) NOT NULL,
        observacoes TEXT NULL,
        proximo_retorno DATE NULL,
        latitude DECIMAL(10,7) NULL,
        longitude DECIMAL(10,7) NULL,
        visitado_em TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        criado_em TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT visitas_cliente_fk FOREIGN KEY (cliente_id) REFERENCES clientes(id) ON DELETE CASCADE,
        CONSTRAINT visitas_usuario_fk FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE RESTRICT,
        KEY visitas_cliente_data_index (cliente_id,visitado_em),
        KEY visitas_usuario_data_index (usuario_id,visitado_em),
        KEY visitas_retorno_index (proximo_retorno)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci");
};

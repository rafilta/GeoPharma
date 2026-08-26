<?php

declare(strict_types=1);

if (session_status() !== PHP_SESSION_ACTIVE) {
    session_set_cookie_params([
        'httponly' => true,
        'secure' => isset($_SERVER['HTTPS']) && $_SERVER['HTTPS'] !== 'off',
        'samesite' => 'Lax',
        'path' => '/',
    ]);
    session_start();
}

define('GEOPHARMA_ROOT', dirname(__DIR__));
define('GEOPHARMA_VERSION', '1.0.0');

date_default_timezone_set('America/Sao_Paulo');

spl_autoload_register(static function (string $class): void {
    $file = GEOPHARMA_ROOT . '/app/' . str_replace('\\', '/', $class) . '.php';
    if (is_file($file)) {
        require_once $file;
    }
});

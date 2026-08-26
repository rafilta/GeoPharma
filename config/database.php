<?php

declare(strict_types=1);

$config = [
    'host' => '127.0.0.1',
    'port' => 3306,
    'database' => 'geopharma',
    'username' => 'root',
    'password' => '',
    'charset' => 'utf8mb4',
];

$environment = [
    'host' => getenv('GEOPHARMA_DB_HOST') ?: null,
    'port' => getenv('GEOPHARMA_DB_PORT') ?: null,
    'database' => getenv('GEOPHARMA_DB_NAME') ?: null,
    'username' => getenv('GEOPHARMA_DB_USER') ?: null,
    'password' => getenv('GEOPHARMA_DB_PASSWORD') ?: null,
];

$config = array_replace($config, array_filter(
    $environment,
    static fn (mixed $value): bool => $value !== null
));
$config['port'] = (int) $config['port'];

$localFile = __DIR__ . '/database.local.php';
if (is_file($localFile)) {
    $local = require $localFile;
    if (is_array($local)) {
        $config = array_replace($config, $local);
    }
}

return $config;

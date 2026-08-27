<?php

Auth::requireLogin();

$pageTitle = $pageTitle ?? 'Dashboard';
$activeMenu = $activeMenu ?? 'dashboard';
$usuarioAtual = Auth::user();
?>
<!doctype html>
<html lang="pt-BR">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title><?= htmlspecialchars($pageTitle) ?> | GeoPharma</title>
    <link rel="stylesheet" href="/assets/vendor/bootstrap-icons/bootstrap-icons.min.css?v=<?= GEOPHARMA_VERSION ?>">
    <link rel="stylesheet" href="/assets/css/adminlte.min.css?v=<?= GEOPHARMA_VERSION ?>">
    <link rel="stylesheet" href="/assets/css/geopharma.css?v=<?= GEOPHARMA_VERSION ?>">
    <?php foreach(($extraStyles??[]) as $style):?><link rel="stylesheet" href="<?= htmlspecialchars($style) ?>"><?php endforeach;?>
</head>
<body class="layout-fixed sidebar-expand-lg bg-body-tertiary">
<div class="app-wrapper">
    <nav class="app-header navbar navbar-expand bg-body">
        <div class="container-fluid">
            <ul class="navbar-nav">
                <li class="nav-item">
                    <button class="nav-link btn" data-lte-toggle="sidebar" type="button" aria-label="Abrir ou fechar menu">
                        <i class="bi bi-list"></i>
                    </button>
                </li>
                <li class="nav-item d-none d-md-block"><a href="/" class="nav-link">Início</a></li>
            </ul>
            <ul class="navbar-nav ms-auto">
                <li class="nav-item dropdown">
                    <button class="nav-link btn dropdown-toggle" data-bs-toggle="dropdown" type="button">
                        <i class="bi bi-person-circle me-1"></i><?= htmlspecialchars($usuarioAtual['nome'] ?? 'Usuário') ?>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end">
                        <li><span class="dropdown-item-text small text-secondary">@<?= htmlspecialchars($usuarioAtual['login'] ?? '') ?></span></li>
                        <li><hr class="dropdown-divider"></li>
                        <li><a class="dropdown-item" href="/logout.php"><i class="bi bi-box-arrow-right me-2"></i>Sair</a></li>
                    </ul>
                </li>
            </ul>
        </div>
    </nav>
    <?php require __DIR__ . '/sidebar.php'; ?>

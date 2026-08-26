<?php

$pageTitle = $pageTitle ?? 'Dashboard';
$activeMenu = $activeMenu ?? 'dashboard';
?>
<!doctype html>
<html lang="pt-BR">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title><?= htmlspecialchars($pageTitle) ?> | GeoPharma</title>
    <link rel="stylesheet" href="/assets/vendor/bootstrap-icons/bootstrap-icons.min.css">
    <link rel="stylesheet" href="/assets/css/adminlte.min.css">
    <link rel="stylesheet" href="/assets/css/geopharma.css">
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
                <li class="nav-item"><span class="nav-link"><i class="bi bi-person-circle me-1"></i>Usuário</span></li>
            </ul>
        </div>
    </nav>
    <?php require __DIR__ . '/sidebar.php'; ?>


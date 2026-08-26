<?php

declare(strict_types=1);
require __DIR__ . '/app/bootstrap.php';

$pageTitle = 'Dashboard';
$activeMenu = 'dashboard';
require __DIR__ . '/includes/header.php';
require __DIR__ . '/includes/page-start.php';
?>
<div class="row g-4">
    <div class="col-lg-3 col-6"><div class="small-box text-bg-primary"><div class="inner"><h3>0</h3><p>Clientes</p></div><i class="small-box-icon bi bi-building"></i><a href="/clientes/" class="small-box-footer link-light">Consultar <i class="bi bi-arrow-right-circle"></i></a></div></div>
    <div class="col-lg-3 col-6"><div class="small-box text-bg-success"><div class="inner"><h3>0</h3><p>Leads capturados</p></div><i class="small-box-icon bi bi-bullseye"></i><a href="/leads/" class="small-box-footer link-light">Consultar <i class="bi bi-arrow-right-circle"></i></a></div></div>
    <div class="col-lg-3 col-6"><div class="small-box text-bg-warning"><div class="inner"><h3>0</h3><p>Em negociação</p></div><i class="small-box-icon bi bi-chat-dots"></i><a href="/leads/" class="small-box-footer link-dark">Consultar <i class="bi bi-arrow-right-circle"></i></a></div></div>
    <div class="col-lg-3 col-6"><div class="small-box text-bg-danger"><div class="inner"><h3>0</h3><p>Novas oportunidades</p></div><i class="small-box-icon bi bi-geo-alt"></i><a href="/mapa/" class="small-box-footer link-light">Abrir mapa <i class="bi bi-arrow-right-circle"></i></a></div></div>
</div>
<div class="card mt-4">
    <div class="card-header"><h3 class="card-title">Bem-vindo ao GeoPharma</h3></div>
    <div class="card-body"><p class="mb-0">Base PHP limpa e pronta para receber o mapa, clientes, leads e regras comerciais.</p></div>
</div>
<?php
require __DIR__ . '/includes/page-end.php';
require __DIR__ . '/includes/footer.php';


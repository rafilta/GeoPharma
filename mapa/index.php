<?php
declare(strict_types=1);
require dirname(__DIR__) . '/app/bootstrap.php';
$pageTitle = 'Mapa de oportunidades'; $activeMenu = 'mapa';
require GEOPHARMA_ROOT . '/includes/header.php'; require GEOPHARMA_ROOT . '/includes/page-start.php';
?>
<div class="card"><div class="card-header"><h3 class="card-title">Pesquisa geográfica</h3></div><div class="card-body"><div class="map-placeholder"><i class="bi bi-map"></i><strong>Laboratório do mapa</strong><span>Google Places e OpenStreetMap serão testados neste módulo.</span></div></div></div>
<?php require GEOPHARMA_ROOT . '/includes/page-end.php'; require GEOPHARMA_ROOT . '/includes/footer.php'; ?>


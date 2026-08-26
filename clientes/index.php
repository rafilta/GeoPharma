<?php
declare(strict_types=1);
require dirname(__DIR__) . '/app/bootstrap.php';
$pageTitle = 'Clientes'; $activeMenu = 'clientes';
require GEOPHARMA_ROOT . '/includes/header.php'; require GEOPHARMA_ROOT . '/includes/page-start.php';
?>
<div class="card"><div class="card-header d-flex align-items-center"><h3 class="card-title">Clientes cadastrados</h3><button class="btn btn-primary btn-sm ms-auto"><i class="bi bi-plus-lg me-1"></i>Novo cliente</button></div><div class="card-body"><p class="text-muted mb-0">O cadastro e a consulta de clientes serão implementados aqui.</p></div></div>
<?php require GEOPHARMA_ROOT . '/includes/page-end.php'; require GEOPHARMA_ROOT . '/includes/footer.php'; ?>


<?php if($totalPaginas>1):?>
<nav class="card-footer d-flex flex-column flex-sm-row align-items-center justify-content-between gap-2" aria-label="Paginação">
    <span class="text-secondary small">Página <?= $paginaAtual ?> de <?= $totalPaginas ?></span>
    <ul class="pagination pagination-sm mb-0">
        <li class="page-item <?= $paginaAtual<=1?'disabled':'' ?>"><a class="page-link" href="<?= $paginaAtual>1?htmlspecialchars(Pagination::url($paginaAtual-1)):'#' ?>" aria-label="Página anterior"><i class="bi bi-chevron-left"></i></a></li>
        <?php for($pagina=max(1,$paginaAtual-2);$pagina<=min($totalPaginas,$paginaAtual+2);$pagina++):?>
        <li class="page-item <?= $pagina===$paginaAtual?'active':'' ?>"><a class="page-link" href="<?= htmlspecialchars(Pagination::url($pagina)) ?>"><?= $pagina ?></a></li>
        <?php endfor;?>
        <li class="page-item <?= $paginaAtual>=$totalPaginas?'disabled':'' ?>"><a class="page-link" href="<?= $paginaAtual<$totalPaginas?htmlspecialchars(Pagination::url($paginaAtual+1)):'#' ?>" aria-label="Próxima página"><i class="bi bi-chevron-right"></i></a></li>
    </ul>
</nav>
<?php endif;?>

    <footer class="app-footer">
        <strong>GeoPharma</strong> — Sistema de inteligência geográfica para farmácias.
        <span class="float-end d-none d-sm-inline">Versão <?= htmlspecialchars(GEOPHARMA_VERSION) ?></span>
    </footer>
</div>
<script src="/assets/vendor/bootstrap/bootstrap.bundle.min.js?v=<?= GEOPHARMA_VERSION ?>"></script>
<script src="/assets/js/adminlte.min.js?v=<?= GEOPHARMA_VERSION ?>"></script>
<script src="/assets/js/geopharma.js?v=<?= GEOPHARMA_VERSION ?>"></script>
<?php foreach(($extraScripts??[]) as $script):?><script src="<?= htmlspecialchars($script) ?>"></script><?php endforeach;?>
</body>
</html>

<aside class="app-sidebar bg-dark shadow" data-bs-theme="dark">
    <div class="sidebar-brand">
        <a href="/" class="brand-link text-decoration-none">
            <span class="brand-symbol"><i class="bi bi-geo-alt-fill"></i></span>
            <span class="brand-text fw-semibold">GeoPharma</span>
        </a>
    </div>
    <div class="sidebar-wrapper">
        <nav class="mt-2">
            <ul class="nav sidebar-menu flex-column" data-lte-toggle="treeview" role="menu">
                <li class="nav-item">
                    <a href="/" class="nav-link <?= $activeMenu === 'dashboard' ? 'active' : '' ?>">
                        <i class="nav-icon bi bi-speedometer2"></i><p>Dashboard</p>
                    </a>
                </li>
                <li class="nav-header">COMERCIAL</li>
                <li class="nav-item">
                    <a href="/clientes/" class="nav-link <?= $activeMenu === 'clientes' ? 'active' : '' ?>">
                        <i class="nav-icon bi bi-building"></i><p>Clientes</p>
                    </a>
                </li>
                <li class="nav-item">
                    <a href="/mapa/" class="nav-link <?= $activeMenu === 'mapa' ? 'active' : '' ?>">
                        <i class="nav-icon bi bi-map"></i><p>Mapa</p>
                    </a>
                </li>
                <li class="nav-item">
                    <a href="/leads/" class="nav-link <?= $activeMenu === 'leads' ? 'active' : '' ?>">
                        <i class="nav-icon bi bi-bullseye"></i><p>Leads</p>
                    </a>
                </li>
                <?php if (Auth::isAdmin()): ?>
                    <li class="nav-header">ADMINISTRAÇÃO</li>
                    <li class="nav-item">
                        <a href="/usuarios/" class="nav-link <?= $activeMenu === 'usuarios' ? 'active' : '' ?>">
                            <i class="nav-icon bi bi-people"></i><p>Usuários</p>
                        </a>
                    </li>
                    <li class="nav-item">
                        <a href="/documentacao/" class="nav-link <?= $activeMenu === 'documentacao' ? 'active' : '' ?>">
                            <i class="nav-icon bi bi-journal-text"></i><p>Documentação</p>
                        </a>
                    </li>
                <?php endif; ?>
            </ul>
        </nav>
    </div>
</aside>

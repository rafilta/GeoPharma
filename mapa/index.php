<?php
declare(strict_types=1);
require dirname(__DIR__).'/app/bootstrap.php';
$pageTitle='Mapa de oportunidades';$activeMenu='mapa';
$extraStyles=['https://unpkg.com/leaflet@1.9.4/dist/leaflet.css','/assets/css/mapa.css?v='.GEOPHARMA_VERSION];
$extraScripts=['https://unpkg.com/leaflet@1.9.4/dist/leaflet.js','/assets/js/mapa.js?v='.GEOPHARMA_VERSION];
require GEOPHARMA_ROOT.'/includes/header.php';require GEOPHARMA_ROOT.'/includes/page-start.php';
?>
<div class="map-workspace card card-outline card-success shadow-sm" data-map-app data-csrf="<?= htmlspecialchars(Csrf::token()) ?>">
    <div class="map-toolbar">
        <div class="map-search"><i class="bi bi-search"></i><label for="map-search" class="visually-hidden">Pesquisar no mapa</label><input id="map-search" type="search" class="form-control" placeholder="Pesquisar cliente ou farmácia"></div>
        <label for="map-radius" class="visually-hidden">Raio de pesquisa</label>
        <select id="map-radius" class="form-select map-radius" title="Raio de pesquisa"><option value="1000">1 km</option><option value="5000" selected>5 km</option><option value="10000">10 km</option><option value="20000">20 km</option></select>
        <button type="button" class="btn btn-outline-success" data-map-locate aria-label="Usar minha localização"><i class="bi bi-crosshair me-1"></i><span>Minha localização</span></button>
        <button type="button" class="btn btn-success" data-map-opportunities aria-label="Buscar farmácias próximas"><i class="bi bi-buildings me-1"></i><span>Farmácias próximas</span></button>
    </div>
    <div class="map-status" data-map-status><span class="spinner-border spinner-border-sm" aria-hidden="true"></span>Carregando clientes reais...</div>
    <div id="geopharma-map" class="geopharma-map" aria-label="Mapa interativo de clientes e oportunidades"></div>
    <div class="map-legend"><span><i class="legend-dot legend-client"></i>Cliente</span><span><i class="legend-dot legend-opportunity"></i>Oportunidade OSM</span><span><i class="legend-dot legend-current"></i>Você</span></div>
    <section class="map-detail" data-map-detail hidden>
        <button type="button" class="map-detail-close" data-map-detail-close aria-label="Fechar detalhes"><i class="bi bi-x-lg"></i></button>
        <span class="badge text-bg-success mb-2" data-detail-type></span>
        <h3 data-detail-name></h3>
        <p class="text-secondary mb-2" data-detail-address></p>
        <dl class="map-detail-data"><div><dt>CNPJ</dt><dd data-detail-document>—</dd></div><div><dt>Telefone</dt><dd data-detail-phone>—</dd></div><div><dt>Última visita</dt><dd data-detail-visit>—</dd></div></dl>
        <div class="map-detail-actions">
            <a class="btn btn-success" target="_blank" rel="noopener" data-detail-route><i class="bi bi-sign-turn-right me-1"></i>Abrir rota</a>
            <a class="btn btn-outline-success" data-detail-call hidden><i class="bi bi-telephone me-1"></i>Ligar</a>
            <a class="btn btn-outline-primary" data-detail-edit hidden><i class="bi bi-pencil me-1"></i>Editar</a>
            <button type="button" class="btn btn-primary" data-detail-visit-button hidden><i class="bi bi-clipboard-check me-1"></i>Registrar visita</button>
        </div>
    </section>
</div>
<div class="modal fade" id="visitModal" tabindex="-1" aria-labelledby="visitModalLabel" aria-hidden="true">
 <div class="modal-dialog modal-dialog-centered"><form class="modal-content" data-visit-form>
  <div class="modal-header"><h2 class="modal-title fs-5" id="visitModalLabel">Registrar visita</h2><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Fechar"></button></div>
  <div class="modal-body"><input type="hidden" name="cliente_id"><div class="mb-3"><label for="visit-result" class="form-label">Resultado <span class="text-danger">*</span></label><select class="form-select" id="visit-result" name="resultado" required><option value="">Selecione</option><option value="realizada">Visita realizada</option><option value="pedido">Pedido realizado</option><option value="negociacao">Em negociação</option><option value="retorno">Retorno necessário</option><option value="sem_contato">Sem contato</option></select></div><div class="mb-3"><label for="visit-notes" class="form-label">Observações</label><textarea class="form-control" id="visit-notes" name="observacoes" rows="3" maxlength="3000"></textarea></div><div><label for="visit-return" class="form-label">Próximo retorno</label><input class="form-control" id="visit-return" type="date" name="proximo_retorno"></div><div class="alert alert-danger mt-3 mb-0" data-visit-error hidden></div></div>
  <div class="modal-footer"><button type="button" class="btn btn-default" data-bs-dismiss="modal">Cancelar</button><button type="submit" class="btn btn-success" data-loading-label="Salvando..."><i class="bi bi-check-lg me-1"></i>Salvar visita</button></div>
 </form></div>
</div>
<?php require GEOPHARMA_ROOT.'/includes/page-end.php';require GEOPHARMA_ROOT.'/includes/footer.php';?>

<?php
declare(strict_types=1);
require dirname(__DIR__).'/app/bootstrap.php';
Auth::requireLogin();

$busca=trim((string)($_GET['busca']??''));
$paginaAtual=Pagination::currentPage();
$pdo=Database::connection();
$where='';
$params=[];
if($busca!==''){
    $where=' WHERE razao_social LIKE :razao OR nome_fantasia LIKE :fantasia OR documento LIKE :documento';
    $params=['razao'=>'%'.$busca.'%','fantasia'=>'%'.$busca.'%','documento'=>'%'.(preg_replace('/\D+/','',$busca)??'').'%'];
}
$contar=$pdo->prepare('SELECT COUNT(*) FROM clientes'.$where);
$contar->execute($params);
$totalClientes=(int)$contar->fetchColumn();
$totalPaginas=Pagination::totalPages($totalClientes);
$paginaAtual=min($paginaAtual,$totalPaginas);
$listar=$pdo->prepare('SELECT id,documento,razao_social,nome_fantasia,telefone,cidade,estado,ativo FROM clientes'.$where.' ORDER BY razao_social LIMIT :limite OFFSET :inicio');
foreach($params as $chave=>$valor) $listar->bindValue(':'.$chave,$valor);
$listar->bindValue(':limite',Pagination::PER_PAGE,PDO::PARAM_INT);
$listar->bindValue(':inicio',Pagination::offset($paginaAtual),PDO::PARAM_INT);
$listar->execute();
$clientes=$listar->fetchAll();

$mensagens=['criado'=>['success','Cliente cadastrado com sucesso.'],'atualizado'=>['success','Cliente atualizado com sucesso.'],'excluido'=>['success','Cliente excluído com sucesso.'],'erro'=>['danger','Não foi possível concluir a operação.']];
$mensagem=$mensagens[$_GET['resultado']??'']??null;
$pageTitle='Clientes';$activeMenu='clientes';
require GEOPHARMA_ROOT.'/includes/header.php';require GEOPHARMA_ROOT.'/includes/page-start.php';
?>
<?php if($mensagem):?><div class="alert alert-<?= $mensagem[0] ?> alert-dismissible fade show" role="alert"><?= htmlspecialchars($mensagem[1]) ?><button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Fechar"></button></div><?php endif;?>
<div class="card card-outline card-success shadow-sm">
    <div class="card-header users-card-header d-flex flex-wrap align-items-center gap-2"><h3 class="card-title mb-0"><i class="bi bi-building me-2"></i>Clientes cadastrados</h3><a href="/clientes/create.php" class="btn btn-success users-add-button ms-auto"><i class="bi bi-plus-lg me-1"></i>Novo cliente</a></div>
    <div class="card-body border-bottom"><form method="get" class="row g-2"><div class="col-12 col-sm"><label for="busca" class="visually-hidden">Pesquisar clientes</label><div class="input-group"><span class="input-group-text"><i class="bi bi-search"></i></span><input type="search" class="form-control" id="busca" name="busca" value="<?= htmlspecialchars($busca) ?>" placeholder="Nome, nome fantasia ou CNPJ"></div></div><div class="col-12 col-sm-auto d-grid"><button type="submit" class="btn btn-outline-success" data-loading-label="Pesquisando...">Pesquisar</button></div><?php if($busca!==''):?><div class="col-12 col-sm-auto d-grid"><a href="/clientes/" class="btn btn-default">Limpar</a></div><?php endif;?></form></div>
    <?php if(!$clientes):?><div class="empty-state"><i class="bi bi-building"></i><h4>Nenhum cliente encontrado</h4><p class="mb-0">Cadastre um cliente ou altere a pesquisa.</p></div><?php else:?>
    <div class="table-responsive crud-table-wrapper"><table class="table table-hover align-middle mb-0 crud-table">
        <thead><tr><th>Cliente</th><th>CNPJ</th><th>Telefone</th><th>Localização</th><th>Status</th><th class="text-end">Ações</th></tr></thead>
        <tbody><?php foreach($clientes as $cliente):?><tr>
            <td><strong class="d-block"><?= htmlspecialchars($cliente['nome_fantasia']?:$cliente['razao_social']) ?></strong><?php if($cliente['nome_fantasia']):?><span class="text-secondary small"><?= htmlspecialchars($cliente['razao_social']) ?></span><?php endif;?></td>
            <td><?= htmlspecialchars(Formatter::cnpj($cliente['documento'])) ?></td>
            <td><?= htmlspecialchars(Formatter::telefone($cliente['telefone'])) ?></td>
            <td><?= htmlspecialchars(trim(($cliente['cidade']??'').(($cliente['cidade']&&$cliente['estado'])?' / ':'').($cliente['estado']??''))?:'—') ?></td>
            <td><span class="badge <?= $cliente['ativo']?'text-bg-success':'text-bg-secondary' ?>"><?= $cliente['ativo']?'Ativo':'Inativo' ?></span></td>
            <td><div class="d-flex justify-content-end gap-2 crud-actions"><a href="/clientes/edit.php?id=<?= (int)$cliente['id'] ?>" class="btn btn-sm btn-outline-primary"><i class="bi bi-pencil"></i><span class="ms-1">Editar</span></a><form method="post" action="/clientes/delete.php" onsubmit="return confirm('Excluir definitivamente este cliente?');"><input type="hidden" name="csrf_token" value="<?= htmlspecialchars(Csrf::token()) ?>"><input type="hidden" name="id" value="<?= (int)$cliente['id'] ?>"><button type="submit" class="btn btn-sm btn-outline-danger" data-loading-label="Excluindo..."><i class="bi bi-trash"></i><span class="ms-1">Excluir</span></button></form></div></td>
        </tr><?php endforeach;?></tbody>
    </table></div><?php endif;?>
    <div class="card-footer text-secondary small"><?= $totalClientes ?> cliente(s) encontrado(s)</div>
    <?php require GEOPHARMA_ROOT.'/includes/pagination.php';?>
</div>
<?php require GEOPHARMA_ROOT.'/includes/page-end.php';require GEOPHARMA_ROOT.'/includes/footer.php';?>

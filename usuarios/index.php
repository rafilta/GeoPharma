<?php
declare(strict_types=1);
require dirname(__DIR__) . '/app/bootstrap.php';
Auth::requireAdmin();
$pdo=Database::connection();
$paginaAtual=Pagination::currentPage();
$totalUsuarios=(int)$pdo->query('SELECT COUNT(*) FROM usuarios')->fetchColumn();
$totalPaginas=Pagination::totalPages($totalUsuarios);
$paginaAtual=min($paginaAtual,$totalPaginas);
$listar=$pdo->prepare('SELECT id,nome,login,email,perfil,ativo,ultimo_acesso_em FROM usuarios ORDER BY nome LIMIT :limite OFFSET :inicio');
$listar->bindValue(':limite',Pagination::PER_PAGE,PDO::PARAM_INT);
$listar->bindValue(':inicio',Pagination::offset($paginaAtual),PDO::PARAM_INT);
$listar->execute();
$usuarios=$listar->fetchAll();
$mensagens = ['criado'=>['success','Usuário cadastrado com sucesso.'],'atualizado'=>['success','Usuário atualizado com sucesso.'],'excluido'=>['success','Usuário excluído com sucesso.'],'proprio_usuario'=>['warning','Você não pode excluir a própria conta.'],'erro'=>['danger','Não foi possível concluir a operação.']];
$mensagem = $mensagens[$_GET['resultado'] ?? ''] ?? null;
$pageTitle='Usuários'; $activeMenu='usuarios';
require GEOPHARMA_ROOT.'/includes/header.php'; require GEOPHARMA_ROOT.'/includes/page-start.php';
?>
<?php if ($mensagem): ?><div class="alert alert-<?= $mensagem[0] ?> alert-dismissible fade show" role="alert"><?= htmlspecialchars($mensagem[1]) ?><button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Fechar"></button></div><?php endif; ?>
<div class="card card-outline card-success shadow-sm">
 <div class="card-header users-card-header d-flex flex-wrap align-items-center gap-2"><h3 class="card-title mb-0"><i class="bi bi-people me-2"></i>Usuários do sistema</h3><a href="/usuarios/create.php" class="btn btn-success users-add-button ms-auto"><i class="bi bi-person-plus me-1"></i>Novo usuário</a></div>
 <div class="card-body p-0">
 <?php if (!$usuarios): ?><div class="empty-state"><i class="bi bi-people"></i><h4>Nenhum usuário cadastrado</h4></div><?php else: ?>
 <div class="table-responsive users-table-wrapper"><table class="table table-hover align-middle mb-0 users-table"><thead><tr><th>Usuário</th><th>Perfil</th><th>Status</th><th>Último acesso</th><th class="text-end">Ações</th></tr></thead><tbody>
 <?php foreach($usuarios as $usuario): ?><tr>
  <td><div class="d-flex align-items-center gap-3"><span class="user-avatar"><?= htmlspecialchars(mb_strtoupper(mb_substr($usuario['nome'],0,1))) ?></span><div><strong class="d-block"><?= htmlspecialchars($usuario['nome']) ?></strong><span class="text-secondary small">@<?= htmlspecialchars($usuario['login']) ?> · <?= htmlspecialchars($usuario['email']) ?></span></div></div></td>
  <td data-label="Perfil"><span class="badge text-bg-light text-capitalize"><?= htmlspecialchars($usuario['perfil']) ?></span></td>
  <td data-label="Status"><span class="badge <?= $usuario['ativo']?'text-bg-success':'text-bg-secondary' ?>"><?= $usuario['ativo']?'Ativo':'Inativo' ?></span></td>
  <td data-label="Último acesso" class="text-secondary small"><?= $usuario['ultimo_acesso_em']?date('d/m/Y H:i',strtotime($usuario['ultimo_acesso_em'])):'Nunca acessou' ?></td>
  <td data-label="Ações"><div class="d-flex justify-content-end gap-2 users-actions"><a href="/usuarios/edit.php?id=<?= (int)$usuario['id'] ?>" class="btn btn-sm btn-outline-primary"><i class="bi bi-pencil"></i><span class="ms-1">Editar</span></a><form method="post" action="/usuarios/delete.php" onsubmit="return confirm('Excluir definitivamente este usuário?');"><input type="hidden" name="csrf_token" value="<?= htmlspecialchars(Csrf::token()) ?>"><input type="hidden" name="id" value="<?= (int)$usuario['id'] ?>"><button type="submit" class="btn btn-sm btn-outline-danger" data-loading-label="Excluindo..." <?= (int)$usuario['id']===(int)(Auth::user()['id']??0)?'disabled title="Você não pode excluir sua própria conta"':'' ?>><i class="bi bi-trash"></i><span class="ms-1">Excluir</span></button></form></div></td>
 </tr><?php endforeach; ?>
 </tbody></table></div><?php endif; ?>
 </div><div class="card-footer text-secondary small"><?= $totalUsuarios ?> usuário(s) cadastrado(s)</div>
 <?php require GEOPHARMA_ROOT.'/includes/pagination.php';?>
</div>
<?php require GEOPHARMA_ROOT.'/includes/page-end.php'; require GEOPHARMA_ROOT.'/includes/footer.php'; ?>

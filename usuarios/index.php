<?php
declare(strict_types=1);
require dirname(__DIR__) . '/app/bootstrap.php';
Auth::requireAdmin();
$usuarios = Database::connection()->query('SELECT id,nome,login,email,perfil,ativo,ultimo_acesso_em FROM usuarios ORDER BY nome')->fetchAll();
$mensagens = ['criado'=>['success','Usuário cadastrado com sucesso.'],'atualizado'=>['success','Usuário atualizado com sucesso.'],'status'=>['success','Status do usuário atualizado.'],'proprio_usuario'=>['warning','Você não pode desativar o próprio acesso.'],'erro'=>['danger','Não foi possível concluir a operação.']];
$mensagem = $mensagens[$_GET['resultado'] ?? ''] ?? null;
$pageTitle='Usuários'; $activeMenu='usuarios';
require GEOPHARMA_ROOT.'/includes/header.php'; require GEOPHARMA_ROOT.'/includes/page-start.php';
?>
<?php if ($mensagem): ?><div class="alert alert-<?= $mensagem[0] ?> alert-dismissible fade show" role="alert"><?= htmlspecialchars($mensagem[1]) ?><button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Fechar"></button></div><?php endif; ?>
<div class="card card-outline card-success shadow-sm">
 <div class="card-header d-flex flex-wrap align-items-center gap-2"><h3 class="card-title mb-0"><i class="bi bi-people me-2"></i>Usuários do sistema</h3><a href="/usuarios/create.php" class="btn btn-success ms-auto"><i class="bi bi-person-plus me-1"></i>Novo usuário</a></div>
 <div class="card-body p-0">
 <?php if (!$usuarios): ?><div class="empty-state"><i class="bi bi-people"></i><h4>Nenhum usuário cadastrado</h4></div><?php else: ?>
 <div class="table-responsive"><table class="table table-hover align-middle mb-0"><thead><tr><th>Usuário</th><th>Perfil</th><th>Status</th><th>Último acesso</th><th class="text-end">Ações</th></tr></thead><tbody>
 <?php foreach($usuarios as $usuario): ?><tr>
  <td><div class="d-flex align-items-center gap-3"><span class="user-avatar"><?= htmlspecialchars(mb_strtoupper(mb_substr($usuario['nome'],0,1))) ?></span><div><strong class="d-block"><?= htmlspecialchars($usuario['nome']) ?></strong><span class="text-secondary small">@<?= htmlspecialchars($usuario['login']) ?> · <?= htmlspecialchars($usuario['email']) ?></span></div></div></td>
  <td><span class="badge text-bg-light text-capitalize"><?= htmlspecialchars($usuario['perfil']) ?></span></td>
  <td><span class="badge <?= $usuario['ativo']?'text-bg-success':'text-bg-secondary' ?>"><?= $usuario['ativo']?'Ativo':'Inativo' ?></span></td>
  <td class="text-secondary small"><?= $usuario['ultimo_acesso_em']?date('d/m/Y H:i',strtotime($usuario['ultimo_acesso_em'])):'Nunca acessou' ?></td>
  <td><div class="d-flex justify-content-end gap-2"><a href="/usuarios/edit.php?id=<?= (int)$usuario['id'] ?>" class="btn btn-sm btn-outline-primary"><i class="bi bi-pencil"></i><span class="d-none d-lg-inline ms-1">Editar</span></a><form method="post" action="/usuarios/status.php"><input type="hidden" name="csrf_token" value="<?= htmlspecialchars(Csrf::token()) ?>"><input type="hidden" name="id" value="<?= (int)$usuario['id'] ?>"><button type="submit" class="btn btn-sm <?= $usuario['ativo']?'btn-outline-danger':'btn-outline-success' ?>" data-loading-label="Aguarde..." <?= (int)$usuario['id']===(int)(Auth::user()['id']??0)?'disabled title="Seu usuário atual"':'' ?>><i class="bi <?= $usuario['ativo']?'bi-person-x':'bi-person-check' ?>"></i><span class="d-none d-lg-inline ms-1"><?= $usuario['ativo']?'Desativar':'Ativar' ?></span></button></form></div></td>
 </tr><?php endforeach; ?>
 </tbody></table></div><?php endif; ?>
 </div><div class="card-footer text-secondary small"><?= count($usuarios) ?> usuário(s) cadastrado(s)</div>
</div>
<?php require GEOPHARMA_ROOT.'/includes/page-end.php'; require GEOPHARMA_ROOT.'/includes/footer.php'; ?>

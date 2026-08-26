<?php
$editando = isset($usuario['id']);
$perfis = ['administrador' => 'Administrador', 'gestor' => 'Gestor', 'vendedor' => 'Vendedor'];
?>
<?php if ($erros): ?>
<div class="alert alert-danger" role="alert"><strong><i class="bi bi-exclamation-triangle me-2"></i>Confira os campos:</strong><ul class="mb-0 mt-2"><?php foreach ($erros as $erro): ?><li><?= htmlspecialchars($erro) ?></li><?php endforeach; ?></ul></div>
<?php endif; ?>
<form method="post" class="card card-outline card-success shadow-sm">
    <input type="hidden" name="csrf_token" value="<?= htmlspecialchars(Csrf::token()) ?>">
    <div class="card-header"><h3 class="card-title"><i class="bi bi-person-vcard me-2"></i>Dados do usuário</h3></div>
    <div class="card-body"><div class="row g-3">
        <div class="col-12 col-lg-7"><label for="nome" class="form-label">Nome completo <span class="text-danger">*</span></label><input type="text" class="form-control" id="nome" name="nome" value="<?= htmlspecialchars($dados['nome']) ?>" maxlength="120" required autofocus></div>
        <div class="col-12 col-sm-6 col-lg-5"><label for="login" class="form-label">Login <span class="text-danger">*</span></label><div class="input-group"><span class="input-group-text">@</span><input type="text" class="form-control" id="login" name="login" value="<?= htmlspecialchars($dados['login']) ?>" maxlength="60" pattern="[A-Za-z0-9._-]+" required></div><div class="form-text">Use letras, números, ponto, traço ou sublinhado.</div></div>
        <div class="col-12 col-lg-7"><label for="email" class="form-label">E-mail <span class="text-danger">*</span></label><div class="input-group"><span class="input-group-text"><i class="bi bi-envelope"></i></span><input type="email" class="form-control" id="email" name="email" value="<?= htmlspecialchars($dados['email']) ?>" maxlength="190" required></div></div>
        <div class="col-12 col-sm-6 col-lg-5"><label for="perfil" class="form-label">Perfil de acesso <span class="text-danger">*</span></label><select class="form-select" id="perfil" name="perfil" required><?php foreach ($perfis as $valor => $rotulo): ?><option value="<?= $valor ?>" <?= $dados['perfil'] === $valor ? 'selected' : '' ?>><?= $rotulo ?></option><?php endforeach; ?></select></div>
        <div class="col-12 col-lg-7"><label for="senha" class="form-label">Senha <?= $editando ? '' : '<span class="text-danger">*</span>' ?></label><div class="input-group"><span class="input-group-text"><i class="bi bi-lock"></i></span><input type="password" class="form-control" id="senha" name="senha" minlength="8" autocomplete="new-password" <?= $editando ? '' : 'required' ?>></div><div class="form-text"><?= $editando ? 'Deixe em branco para manter a senha atual.' : 'Use pelo menos 8 caracteres.' ?></div></div>
        <div class="col-12 col-sm-6 col-lg-5 d-flex align-items-end"><div class="form-check form-switch mb-2"><input class="form-check-input" type="checkbox" role="switch" id="ativo" name="ativo" value="1" <?= $dados['ativo'] ? 'checked' : '' ?>><label class="form-check-label" for="ativo">Usuário ativo</label></div></div>
    </div></div>
    <div class="card-footer d-flex flex-column-reverse flex-sm-row justify-content-end gap-2"><a href="/usuarios/" class="btn btn-default">Cancelar</a><button type="submit" class="btn btn-success" data-loading-label="Salvando..."><i class="bi bi-check-lg me-1"></i>Salvar usuário</button></div>
</form>

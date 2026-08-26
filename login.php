<?php

declare(strict_types=1);

require __DIR__ . '/app/bootstrap.php';

if (Auth::check()) {
    header('Location: /');
    exit;
}

$_SESSION['csrf_login'] ??= bin2hex(random_bytes(32));
$erro = null;
$login = '';

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $login = trim((string) ($_POST['login'] ?? ''));
    $senha = (string) ($_POST['senha'] ?? '');
    $token = (string) ($_POST['csrf_token'] ?? '');

    if (!hash_equals($_SESSION['csrf_login'], $token)) {
        $erro = 'A sessão expirou. Atualize a página e tente novamente.';
    } elseif ($login === '' || $senha === '') {
        $erro = 'Informe o login e a senha.';
    } else {
        try {
            if (Auth::attempt($login, $senha)) {
                unset($_SESSION['csrf_login']);
                header('Location: /');
                exit;
            }
            $erro = 'Login ou senha inválidos.';
        } catch (Throwable) {
            $erro = 'Não foi possível acessar o sistema agora. Tente novamente em instantes.';
        }
    }
}
?>
<!doctype html>
<html lang="pt-BR">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover">
    <meta name="theme-color" content="#071b2c">
    <title>Entrar | GeoPharma</title>
    <link rel="stylesheet" href="/assets/vendor/bootstrap-icons/bootstrap-icons.min.css">
    <link rel="stylesheet" href="/assets/css/adminlte.min.css">
    <link rel="stylesheet" href="/assets/css/geopharma.css">
</head>
<body class="login-page-geopharma">
<main class="login-shell">
    <section class="login-brand-panel" aria-label="Apresentação do GeoPharma">
        <div class="login-brand-content">
            <img src="/assets/img/geopharma-logo.png" alt="Logo GeoPharma" class="login-hero-logo">
            <span class="login-eyebrow">Inteligência geográfica</span>
            <h1>Encontre oportunidades.<br><span>Conquiste territórios.</span></h1>
            <p>Dados, mapas e relacionamento comercial em uma experiência simples e inteligente.</p>
            <div class="login-brand-pills" aria-hidden="true">
                <span><i class="bi bi-geo-alt-fill"></i> Mapa comercial</span>
                <span><i class="bi bi-graph-up-arrow"></i> Novos leads</span>
            </div>
        </div>
    </section>

    <section class="login-form-panel">
        <div class="login-card">
            <div class="login-mobile-brand">
                <img src="/assets/img/geopharma-logo.png" alt="" aria-hidden="true">
                <strong>Geo<span>Pharma</span></strong>
            </div>
            <div class="login-heading">
                <span>Bem-vindo de volta</span>
                <h2>Acesse sua conta</h2>
                <p>Entre com seu usuário para continuar.</p>
            </div>

            <?php if ($erro): ?>
                <div class="alert alert-danger login-alert" role="alert">
                    <i class="bi bi-exclamation-circle-fill"></i>
                    <span><?= htmlspecialchars($erro) ?></span>
                </div>
            <?php endif; ?>

            <form method="post" autocomplete="on" class="login-form">
                <input type="hidden" name="csrf_token" value="<?= htmlspecialchars($_SESSION['csrf_login']) ?>">
                <div class="login-field">
                    <label for="login">Usuário</label>
                    <div class="login-input-wrap">
                        <i class="bi bi-person"></i>
                        <input id="login" name="login" type="text" value="<?= htmlspecialchars($login) ?>" autocomplete="username" placeholder="Digite seu usuário" maxlength="60" required autofocus>
                    </div>
                </div>
                <div class="login-field">
                    <div class="login-label-row">
                        <label for="senha">Senha</label>
                        <button class="login-forgot" type="button" title="Disponível em breve">Recuperar senha</button>
                    </div>
                    <div class="login-input-wrap">
                        <i class="bi bi-lock"></i>
                        <input id="senha" name="senha" type="password" autocomplete="current-password" placeholder="Digite sua senha" required>
                        <button type="button" class="login-password-toggle" aria-label="Mostrar senha" data-password-toggle>
                            <i class="bi bi-eye"></i>
                        </button>
                    </div>
                </div>
                <button type="submit" class="login-submit" data-loading-label="Entrando...">
                    <span>Entrar no GeoPharma</span>
                    <i class="bi bi-arrow-right"></i>
                </button>
            </form>
            <p class="login-support"><i class="bi bi-shield-check"></i> Acesso seguro e protegido</p>
        </div>
    </section>
    <div class="login-orb login-orb-one" aria-hidden="true"></div>
    <div class="login-orb login-orb-two" aria-hidden="true"></div>
</main>
<script>
document.querySelector('[data-password-toggle]')?.addEventListener('click', function () {
    const input = document.getElementById('senha');
    const showing = input.type === 'text';
    input.type = showing ? 'password' : 'text';
    this.setAttribute('aria-label', showing ? 'Mostrar senha' : 'Ocultar senha');
    this.querySelector('i').className = showing ? 'bi bi-eye' : 'bi bi-eye-slash';
});
</script>
<script src="/assets/js/geopharma.js"></script>
</body>
</html>

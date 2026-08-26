<?php

declare(strict_types=1);

final class Auth
{
    public static function attempt(string $login, string $senha): bool
    {
        $comando = Database::connection()->prepare(
            'SELECT id, nome, login, senha_hash, perfil
             FROM usuarios
             WHERE login = :login AND ativo = 1
             LIMIT 1'
        );
        $comando->execute(['login' => trim($login)]);
        $usuario = $comando->fetch();

        if (!$usuario || !password_verify($senha, $usuario['senha_hash'])) {
            return false;
        }

        session_regenerate_id(true);
        $_SESSION['usuario'] = [
            'id' => (int) $usuario['id'],
            'nome' => $usuario['nome'],
            'login' => $usuario['login'],
            'perfil' => $usuario['perfil'],
        ];

        $atualizar = Database::connection()->prepare(
            'UPDATE usuarios SET ultimo_acesso_em = NOW() WHERE id = :id'
        );
        $atualizar->execute(['id' => $usuario['id']]);

        return true;
    }

    public static function check(): bool
    {
        return isset($_SESSION['usuario']['id']);
    }

    public static function user(): ?array
    {
        return self::check() ? $_SESSION['usuario'] : null;
    }

    public static function requireLogin(): void
    {
        if (self::check()) {
            return;
        }

        header('Location: /login.php');
        exit;
    }

    public static function logout(): void
    {
        $_SESSION = [];
        if (ini_get('session.use_cookies')) {
            $parametros = session_get_cookie_params();
            setcookie(session_name(), '', time() - 42000, $parametros['path'], '', $parametros['secure'], $parametros['httponly']);
        }
        session_destroy();
    }
}

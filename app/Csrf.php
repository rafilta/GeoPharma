<?php
declare(strict_types=1);

final class Csrf
{
    public static function token(): string
    {
        $_SESSION['csrf_token'] ??= bin2hex(random_bytes(32));
        return $_SESSION['csrf_token'];
    }

    public static function validate(?string $token): bool
    {
        return is_string($token) && hash_equals(self::token(), $token);
    }
}

<?php
declare(strict_types=1);
final class Formatter
{
    public static function cnpj(?string $valor): string
    {
        $numero=preg_replace('/\D+/','',(string)$valor)??'';
        if(strlen($numero)!==14) return (string)$valor;
        return substr($numero,0,2).'.'.substr($numero,2,3).'.'.substr($numero,5,3).'/'.substr($numero,8,4).'-'.substr($numero,12,2);
    }

    public static function telefone(?string $valor): string
    {
        $numero=preg_replace('/\D+/','',(string)$valor)??'';
        if(strlen($numero)===10) return '('.substr($numero,0,2).') '.substr($numero,2,4).'-'.substr($numero,6,4);
        if(strlen($numero)===11) return '('.substr($numero,0,2).') '.substr($numero,2,5).'-'.substr($numero,7,4);
        return $valor?:'—';
    }
}

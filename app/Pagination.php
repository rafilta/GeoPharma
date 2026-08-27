<?php
declare(strict_types=1);
final class Pagination
{
    public const PER_PAGE=10;

    public static function currentPage(): int
    {
        return max(1,filter_input(INPUT_GET,'pagina',FILTER_VALIDATE_INT)?:1);
    }

    public static function totalPages(int $total): int
    {
        return max(1,(int)ceil($total/self::PER_PAGE));
    }

    public static function offset(int $page): int
    {
        return ($page-1)*self::PER_PAGE;
    }

    public static function url(int $page): string
    {
        $query=$_GET;$query['pagina']=$page;
        return '?'.http_build_query($query);
    }
}

<?php
declare(strict_types=1);
require dirname(__DIR__).'/app/bootstrap.php';
Auth::requireAdmin();

if($_SERVER['REQUEST_METHOD']!=='POST'||!Csrf::validate($_POST['csrf_token']??null)){
    header('Location: /usuarios/?resultado=erro');
    exit;
}

$id=filter_var($_POST['id']??null,FILTER_VALIDATE_INT)?:0;
if($id===(int)(Auth::user()['id']??0)){
    header('Location: /usuarios/?resultado=proprio_usuario');
    exit;
}

$excluir=Database::connection()->prepare('DELETE FROM usuarios WHERE id=:id');
$excluir->execute(['id'=>$id]);
header('Location: /usuarios/?resultado='.($excluir->rowCount()>0?'excluido':'erro'));
exit;

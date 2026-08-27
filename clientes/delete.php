<?php
declare(strict_types=1);
require dirname(__DIR__).'/app/bootstrap.php';Auth::requireLogin();if($_SERVER['REQUEST_METHOD']!=='POST'||!Csrf::validate($_POST['csrf_token']??null)){header('Location: /clientes/?resultado=erro');exit;}$id=filter_var($_POST['id']??null,FILTER_VALIDATE_INT)?:0;$q=Database::connection()->prepare('DELETE FROM clientes WHERE id=:id');$q->execute(['id'=>$id]);header('Location: /clientes/?resultado='.($q->rowCount()>0?'excluido':'erro'));exit;

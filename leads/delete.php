<?php
declare(strict_types=1);
require dirname(__DIR__).'/app/bootstrap.php';Auth::requireLogin();if($_SERVER['REQUEST_METHOD']!=='POST'||!Csrf::validate($_POST['csrf_token']??null)){header('Location: /leads/?resultado=erro');exit;}$id=filter_var($_POST['id']??null,FILTER_VALIDATE_INT)?:0;$q=Database::connection()->prepare('DELETE FROM leads WHERE id=:id AND cliente_id IS NULL');$q->execute(['id'=>$id]);header('Location: /leads/?resultado='.($q->rowCount()?'excluido':'erro'));exit;

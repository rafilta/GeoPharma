<?php
declare(strict_types=1);
require dirname(__DIR__).'/app/bootstrap.php';Auth::requireLogin();header('Content-Type: application/json; charset=utf-8');
if($_SERVER['REQUEST_METHOD']!=='POST'||!Csrf::validate($_POST['csrf_token']??null)){http_response_code(419);echo json_encode(['erro'=>'A sessão expirou. Atualize a página.'],JSON_UNESCAPED_UNICODE);exit;}
$clienteId=filter_var($_POST['cliente_id']??null,FILTER_VALIDATE_INT)?:0;$resultado=trim((string)($_POST['resultado']??''));$observacoes=trim((string)($_POST['observacoes']??''));$retorno=trim((string)($_POST['proximo_retorno']??''));$lat=filter_var($_POST['latitude']??null,FILTER_VALIDATE_FLOAT);$lng=filter_var($_POST['longitude']??null,FILTER_VALIDATE_FLOAT);$resultados=['realizada','pedido','negociacao','retorno','sem_contato'];
if(!$clienteId||!in_array($resultado,$resultados,true)){http_response_code(422);echo json_encode(['erro'=>'Informe um resultado válido.'],JSON_UNESCAPED_UNICODE);exit;}
$pdo=Database::connection();$existe=$pdo->prepare('SELECT COUNT(*) FROM clientes WHERE id=:id');$existe->execute(['id'=>$clienteId]);if(!(int)$existe->fetchColumn()){http_response_code(404);echo json_encode(['erro'=>'Cliente não encontrado.'],JSON_UNESCAPED_UNICODE);exit;}
$q=$pdo->prepare('INSERT INTO visitas(cliente_id,usuario_id,resultado,observacoes,proximo_retorno,latitude,longitude) VALUES(:cliente,:usuario,:resultado,:observacoes,:retorno,:latitude,:longitude)');
$q->execute(['cliente'=>$clienteId,'usuario'=>(int)Auth::user()['id'],'resultado'=>$resultado,'observacoes'=>$observacoes?:null,'retorno'=>$retorno?:null,'latitude'=>$lat===false?null:$lat,'longitude'=>$lng===false?null:$lng]);
echo json_encode(['sucesso'=>true,'mensagem'=>'Visita registrada com sucesso.'],JSON_UNESCAPED_UNICODE);

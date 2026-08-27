<?php
declare(strict_types=1);
require dirname(__DIR__).'/app/bootstrap.php';Auth::requireLogin();header('Content-Type: application/json; charset=utf-8');
$sql="SELECT c.id,c.documento,c.razao_social,c.nome_fantasia,c.telefone,c.email,c.logradouro,c.numero,c.bairro,c.cidade,c.estado,c.latitude,c.longitude,c.ativo,
    (SELECT MAX(v.visitado_em) FROM visitas v WHERE v.cliente_id=c.id) AS ultima_visita
    FROM clientes c WHERE c.latitude IS NOT NULL AND c.longitude IS NOT NULL ORDER BY c.razao_social";
$clientes=Database::connection()->query($sql)->fetchAll();
foreach($clientes as &$cliente){$cliente['id']=(int)$cliente['id'];$cliente['latitude']=(float)$cliente['latitude'];$cliente['longitude']=(float)$cliente['longitude'];$cliente['ativo']=(bool)$cliente['ativo'];$cliente['documento_formatado']=Formatter::cnpj($cliente['documento']);$cliente['telefone_formatado']=Formatter::telefone($cliente['telefone']);}
unset($cliente);
echo json_encode(['clientes'=>$clientes],JSON_UNESCAPED_UNICODE|JSON_UNESCAPED_SLASHES);

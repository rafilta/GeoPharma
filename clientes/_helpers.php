<?php
declare(strict_types=1);
function clienteDadosVazios(): array {
    return ['tipo'=>'juridica','documento'=>'','razao_social'=>'','nome_fantasia'=>'','telefone'=>'','email'=>'','cep'=>'','logradouro'=>'','numero'=>'','complemento'=>'','bairro'=>'','cidade'=>'','estado'=>'','latitude'=>'','longitude'=>'','observacoes'=>'','ativo'=>true];
}
function clienteDadosPost(): array {
    $t=static fn(string $c):string=>trim((string)($_POST[$c]??''));
    return ['tipo'=>$t('tipo'),'documento'=>preg_replace('/\\D+/','',$t('documento'))??'','razao_social'=>$t('razao_social'),'nome_fantasia'=>$t('nome_fantasia'),'telefone'=>$t('telefone'),'email'=>mb_strtolower($t('email')),'cep'=>preg_replace('/\\D+/','',$t('cep'))??'','logradouro'=>$t('logradouro'),'numero'=>$t('numero'),'complemento'=>$t('complemento'),'bairro'=>$t('bairro'),'cidade'=>$t('cidade'),'estado'=>mb_strtoupper($t('estado')),'latitude'=>str_replace(',','.',$t('latitude')),'longitude'=>str_replace(',','.',$t('longitude')),'observacoes'=>$t('observacoes'),'ativo'=>isset($_POST['ativo'])];
}
function clienteDocumentoValido(string $d,string $tipo):bool {
    if($tipo==='fisica') return strlen($d)===11;
    if(strlen($d)!==14||preg_match('/^(\\d)\\1{13}$/',$d)) return false;
    $calc=static function(string $base,array $pesos):int{$s=0;foreach($pesos as $i=>$p)$s+=(int)$base[$i]*$p;$r=$s%11;return $r<2?0:11-$r;};
    $a=$calc(substr($d,0,12),[5,4,3,2,9,8,7,6,5,4,3,2]);$b=$calc(substr($d,0,12).$a,[6,5,4,3,2,9,8,7,6,5,4,3,2]);return substr($d,-2)===(string)$a.$b;
}
function clienteErros(array $d):array {
    $e=[];if(!in_array($d['tipo'],['juridica','fisica'],true))$e[]='Selecione um tipo de cliente válido.';elseif(!clienteDocumentoValido($d['documento'],$d['tipo']))$e[]=$d['tipo']==='juridica'?'Informe um CNPJ válido.':'Informe um CPF com 11 dígitos.';
    if(mb_strlen($d['razao_social'])<3)$e[]='Informe o nome ou a razão social.';if($d['email']!==''&&!filter_var($d['email'],FILTER_VALIDATE_EMAIL))$e[]='Informe um e-mail válido.';if($d['cep']!==''&&strlen($d['cep'])!==8)$e[]='Informe um CEP com 8 dígitos.';if($d['estado']!==''&&!preg_match('/^[A-Z]{2}$/',$d['estado']))$e[]='Informe a UF com 2 letras.';
    foreach(['latitude'=>[-90,90],'longitude'=>[-180,180]] as $c=>[$min,$max])if($d[$c]!==''&&(!is_numeric($d[$c])||(float)$d[$c]<$min||(float)$d[$c]>$max))$e[]=ucfirst($c).' inválida.';return $e;
}
function clienteParametros(array $d):array {
    $d['ativo']=$d['ativo']?1:0;foreach(['nome_fantasia','telefone','email','cep','logradouro','numero','complemento','bairro','cidade','estado','latitude','longitude','observacoes'] as $c)if($d[$c]==='')$d[$c]=null;return $d;
}

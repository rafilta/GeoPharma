<?php
declare(strict_types=1);
function leadStatus():array{return ['novo'=>'Novo','contato'=>'Contato realizado','visita'=>'Visita agendada','negociacao'=>'Em negociação','convertido'=>'Convertido em cliente','perdido'=>'Perdido'];}
function leadBadge(string $status):string{return match($status){'contato'=>'text-bg-info','visita'=>'text-bg-primary','negociacao'=>'text-bg-warning','convertido'=>'text-bg-success','perdido'=>'text-bg-secondary',default=>'text-bg-light'};}
function leadCnpjValido(string $d):bool{if(strlen($d)!==14||preg_match('/^(\d)\1{13}$/',$d))return false;$calc=static function(string $b,array $p):int{$s=0;foreach($p as $i=>$v)$s+=(int)$b[$i]*$v;$r=$s%11;return $r<2?0:11-$r;};$a=$calc(substr($d,0,12),[5,4,3,2,9,8,7,6,5,4,3,2]);$b=$calc(substr($d,0,12).$a,[6,5,4,3,2,9,8,7,6,5,4,3,2]);return substr($d,-2)===(string)$a.$b;}

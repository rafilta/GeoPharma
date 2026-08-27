(() => {
    'use strict';
    const app=document.querySelector('[data-map-app]');
    if(!app||typeof L==='undefined') return;
    const status=app.querySelector('[data-map-status]');
    const search=app.querySelector('#map-search');
    const suggestions=app.querySelector('#map-suggestions');
    const radius=app.querySelector('#map-radius');
    const detail=app.querySelector('[data-map-detail]');
    const clientLayer=L.layerGroup();
    const opportunityLayer=L.layerGroup();
    const map=L.map('geopharma-map',{zoomControl:true}).setView([-22.9068,-43.1729],12);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png',{maxZoom:19,attribution:'&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'}).addTo(map);
    clientLayer.addTo(map);opportunityLayer.addTo(map);
    let currentPosition=null;let searchCenter=null;let currentMarker=null;let centerMarker=null;let radiusCircle=null;let selectedClient=null;let selectedOpportunity=null;let searchTimer=null;let searchRequest=0;const clientItems=[];const opportunityItems=[];
    const setStatus=(message,loading=false,error=false)=>{
        if(!message){status.hidden=true;return;}
        status.hidden=false;status.innerHTML='';
        if(loading){const spinner=document.createElement('span');spinner.className='spinner-border spinner-border-sm';spinner.setAttribute('aria-hidden','true');status.append(spinner);}
        const text=document.createTextNode(message);status.append(text);status.style.background=error?'rgba(153,27,27,.94)':'rgba(15,55,63,.9)';
    };
    const icon=(type)=>L.divIcon({className:'',html:`<span class="map-marker map-marker-${type}"><i class="bi ${type==='client'?'bi-building':'bi-capsule'}"></i></span>`,iconSize:[36,36],iconAnchor:[18,36]});
    const text=(selector,value='—')=>{detail.querySelector(selector).textContent=value||'—';};
    const formatCnpj=value=>{const digits=String(value||'').replace(/\D/g,'');return digits.length===14?digits.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/,'$1.$2.$3/$4-$5'):'';};
    const routeUrl=(lat,lng)=>`https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(lat+','+lng)}`;
    const openDetail=(item,type)=>{
        selectedClient=type==='client'?item:null;selectedOpportunity=type==='opportunity'?item:null;detail.hidden=false;
        text('[data-detail-type]',type==='client'?'Cliente cadastrado':'Oportunidade OpenStreetMap');
        text('[data-detail-name]',item.nome_fantasia||item.nome||item.razao_social);
        const address=item.endereco||[item.logradouro,item.numero,item.bairro,item.cidade,item.estado].filter(Boolean).join(', ');
        text('[data-detail-address]',address||'Endereço não informado na fonte');
        text('[data-detail-document]',item.documento_formatado||formatCnpj(item.cnpj)||(type==='opportunity'?'Ainda não informado':'—'));
        text('[data-detail-phone]',item.telefone_formatado||item.telefone);
        text('[data-detail-visit]',item.ultima_visita?new Date(item.ultima_visita.replace(' ','T')).toLocaleString('pt-BR'):'Nenhuma registrada');
        detail.querySelector('[data-detail-route]').href=routeUrl(item.latitude,item.longitude);
        const call=detail.querySelector('[data-detail-call]');const phone=(item.telefone||'').replace(/\D/g,'');call.hidden=!phone;call.href=phone?`tel:${phone}`:'#';
        const edit=detail.querySelector('[data-detail-edit]');edit.hidden=type!=='client';if(type==='client')edit.href=`/clientes/edit.php?id=${item.id}`;
        detail.querySelector('[data-detail-visit-button]').hidden=type!=='client';
        const capture=detail.querySelector('[data-detail-capture]');capture.hidden=type!=='opportunity';capture.disabled=Boolean(item.capturado);capture.innerHTML=item.capturado?'<i class="bi bi-check-lg me-1"></i>Lead já capturado':'<i class="bi bi-person-plus me-1"></i>Capturar lead';
    };
    const hideSuggestions=()=>{suggestions.hidden=true;suggestions.innerHTML='';search.setAttribute('aria-expanded','false');};
    const drawRadius=()=>{
        if(!searchCenter)return;
        if(radiusCircle)map.removeLayer(radiusCircle);
        radiusCircle=L.circle([searchCenter.latitude,searchCenter.longitude],{radius:Number(radius.value),color:'#0f9f82',weight:2,fillColor:'#20c997',fillOpacity:.1,dashArray:'7 6'}).addTo(map);
        map.fitBounds(radiusCircle.getBounds(),{padding:[30,30]});
    };
    const chooseAddress=(address,{load=true}={})=>{
        searchCenter={latitude:Number(address.latitude),longitude:Number(address.longitude)};search.value=address.rotulo;hideSuggestions();
        if(centerMarker)map.removeLayer(centerMarker);
        centerMarker=L.marker([searchCenter.latitude,searchCenter.longitude],{icon:L.divIcon({className:'',html:'<span class="map-center-marker"><i class="bi bi-geo-alt-fill"></i></span>',iconSize:[34,34],iconAnchor:[17,34]})}).addTo(map).bindTooltip('Centro da pesquisa');
        drawRadius();if(load)loadOpportunities();
    };
    const reverseAddress=async position=>{
        try{const response=await fetch(`/mapa/geocodificar.php?modo=reverso&lat=${position.latitude}&lng=${position.longitude}`,{headers:{Accept:'application/json'}});const data=await response.json();if(response.ok&&data.enderecos?.length)search.value=data.enderecos[0].rotulo;else search.value=`${position.latitude.toFixed(6)}, ${position.longitude.toFixed(6)}`;}
        catch(_error){search.value=`${position.latitude.toFixed(6)}, ${position.longitude.toFixed(6)}`;}
    };
    const renderSuggestions=items=>{
        suggestions.innerHTML='';if(!items.length){hideSuggestions();return;}
        items.forEach(address=>{const button=document.createElement('button');button.type='button';button.className='map-suggestion';button.setAttribute('role','option');const name=document.createElement('strong');name.textContent=address.nome;const detailText=document.createElement('small');detailText.textContent=address.rotulo;button.append(name,detailText);button.addEventListener('click',()=>chooseAddress(address));suggestions.append(button);});
        suggestions.hidden=false;search.setAttribute('aria-expanded','true');
    };
    const searchAddresses=async()=>{
        const query=search.value.trim();if(query.length<3){hideSuggestions();return;}const request=++searchRequest;
        try{const response=await fetch(`/mapa/geocodificar.php?q=${encodeURIComponent(query)}`,{headers:{Accept:'application/json'}});const data=await response.json();if(request!==searchRequest)return;if(!response.ok)throw new Error(data.erro||'Não foi possível buscar o endereço.');renderSuggestions(data.enderecos||[]);}
        catch(error){if(request===searchRequest)setStatus(error.message,false,true);}
    };
    const addItem=(item,type)=>{
        const marker=L.marker([item.latitude,item.longitude],{icon:icon(type),title:item.nome_fantasia||item.nome||item.razao_social});
        marker.on('click',()=>openDetail(item,type));(type==='client'?clientLayer:opportunityLayer).addLayer(marker);
        (type==='client'?clientItems:opportunityItems).push({marker,item,type});
        return marker;
    };
    const loadClients=async()=>{
        setStatus('Carregando clientes reais...',true);
        try{
            const response=await fetch('/mapa/dados.php',{headers:{Accept:'application/json'}});const data=await response.json();
            if(!response.ok)throw new Error(data.erro||'Não foi possível carregar os clientes.');
            const bounds=[];data.clientes.forEach(item=>{addItem(item,'client');bounds.push([item.latitude,item.longitude]);});
            if(bounds.length)map.fitBounds(bounds,{padding:[35,35],maxZoom:14});
            setStatus(data.clientes.length?`${data.clientes.length} cliente(s) real(is) carregado(s).`:'Nenhum cliente com coordenadas cadastrado.');
            window.setTimeout(()=>setStatus(''),2600);
        }catch(error){setStatus(error.message||'Erro ao carregar clientes.',false,true);}
    };
    const locate=(announce=true)=>new Promise((resolve,reject)=>{
        if(!navigator.geolocation){reject(new Error('Este aparelho não oferece geolocalização.'));return;}
        if(announce)setStatus('Obtendo sua localização real...',true);
        navigator.geolocation.getCurrentPosition(position=>{
            currentPosition={latitude:position.coords.latitude,longitude:position.coords.longitude};searchCenter={...currentPosition};
            if(currentMarker)map.removeLayer(currentMarker);
            currentMarker=L.circleMarker([currentPosition.latitude,currentPosition.longitude],{radius:9,color:'#fff',weight:3,fillColor:'#2563eb',fillOpacity:1}).addTo(map).bindTooltip('Sua localização');
            reverseAddress(currentPosition);drawRadius();if(announce){setStatus('Localização atualizada. Buscando farmácias no raio escolhido.');loadOpportunities();}resolve(currentPosition);
        },()=>{const error=new Error('Não foi possível acessar sua localização. Verifique a permissão do navegador.');if(announce)setStatus(error.message,false,true);reject(error);},{enableHighAccuracy:true,timeout:12000,maximumAge:60000});
    });
    const loadOpportunities=async()=>{
        try{
            const origin=searchCenter;if(!origin)throw new Error('Escolha um endereço ou toque em Minha localização.');drawRadius();
            setStatus('Buscando farmácias reais no OpenStreetMap...',true);opportunityLayer.clearLayers();opportunityItems.length=0;
            const meters=Number(radius.value);const response=await fetch(`/mapa/oportunidades.php?lat=${origin.latitude}&lng=${origin.longitude}&raio=${meters}`,{headers:{Accept:'application/json'}});const data=await response.json();
            if(!response.ok)throw new Error(data.erro||'Não foi possível buscar as farmácias.');
            data.oportunidades.forEach(item=>addItem(item,'opportunity'));
            setStatus(`${data.oportunidades.length} farmácia(s) real(is) encontrada(s) no OpenStreetMap.`);window.setTimeout(()=>setStatus(''),3200);
        }catch(error){setStatus(error.message||'Erro ao buscar oportunidades.',false,true);}
    };
    app.querySelector('[data-map-locate]').addEventListener('click',()=>locate());
    app.querySelector('[data-map-opportunities]').addEventListener('click',loadOpportunities);
    app.querySelector('[data-map-detail-close]').addEventListener('click',()=>{detail.hidden=true;});
    search.addEventListener('input',()=>{searchCenter=null;window.clearTimeout(searchTimer);searchTimer=window.setTimeout(searchAddresses,350);});
    search.addEventListener('keydown',event=>{if(event.key==='Escape')hideSuggestions();});
    radius.addEventListener('change',()=>{if(searchCenter){drawRadius();loadOpportunities();}});
    document.addEventListener('click',event=>{if(!event.target.closest('.map-search'))hideSuggestions();});
    const modalElement=document.querySelector('#visitModal');const visitModal=bootstrap.Modal.getOrCreateInstance(modalElement);const visitForm=modalElement.querySelector('[data-visit-form]');
    detail.querySelector('[data-detail-visit-button]').addEventListener('click',()=>{
        if(!selectedClient)return;
        const clientId=String(selectedClient.id);
        visitForm.reset();
        const clientInput=visitForm.querySelector('[name="cliente_id"]');
        clientInput.value=clientId;
        clientInput.setAttribute('value',clientId);
        visitForm.querySelector('[data-visit-error]').hidden=true;
        visitModal.show();
    });
    detail.querySelector('[data-detail-capture]').addEventListener('click',async event=>{
        if(!selectedOpportunity)return;const button=event.currentTarget;const original=button.innerHTML;button.disabled=true;button.setAttribute('aria-busy','true');button.innerHTML='<span class="spinner-border spinner-border-sm" aria-hidden="true"></span> Capturando...';
        const data=new FormData();data.append('csrf_token',app.dataset.csrf);data.append('osm_id',selectedOpportunity.osm_id||'');data.append('nome',selectedOpportunity.nome||'');data.append('cnpj',selectedOpportunity.cnpj||'');data.append('telefone',selectedOpportunity.telefone||'');data.append('endereco',selectedOpportunity.endereco||'');data.append('latitude',selectedOpportunity.latitude);data.append('longitude',selectedOpportunity.longitude);
        try{const response=await fetch('/mapa/capturar-lead.php',{method:'POST',body:data,headers:{Accept:'application/json'}});const result=await response.json();if(!response.ok)throw new Error(result.erro||'Não foi possível capturar o lead.');selectedOpportunity.capturado=true;button.innerHTML='<i class="bi bi-check-lg me-1"></i>Lead capturado';setStatus(result.mensagem);window.setTimeout(()=>setStatus(''),2500);}
        catch(error){button.innerHTML=original;button.disabled=false;setStatus(error.message||'Erro ao capturar lead.',false,true);}
        finally{button.removeAttribute('aria-busy');}
    });
    visitForm.addEventListener('submit',async event=>{
        event.preventDefault();const errorBox=visitForm.querySelector('[data-visit-error]');errorBox.hidden=true;const data=new FormData(visitForm);data.set('cliente_id',String(selectedClient?.id||''));data.append('csrf_token',app.dataset.csrf);
        if(currentPosition){data.append('latitude',currentPosition.latitude);data.append('longitude',currentPosition.longitude);}
        try{const response=await fetch('/mapa/registrar-visita.php',{method:'POST',body:data,headers:{Accept:'application/json'}});const result=await response.json();if(!response.ok)throw new Error(result.erro||'Não foi possível registrar a visita.');visitModal.hide();setStatus(result.mensagem);window.setTimeout(()=>setStatus(''),2500);}
        catch(error){errorBox.textContent=error.message||'Erro ao registrar visita.';errorBox.hidden=false;}
        finally{const button=visitForm.querySelector('button[type="submit"]');if(button.dataset.originalContent)button.innerHTML=button.dataset.originalContent;button.disabled=false;button.removeAttribute('aria-busy');button.dataset.loadingActive='false';}
    });
    loadClients();
})();

(() => {
    'use strict';
    const app=document.querySelector('[data-map-app]');
    if(!app||typeof L==='undefined') return;
    const status=app.querySelector('[data-map-status]');
    const search=app.querySelector('#map-search');
    const radius=app.querySelector('#map-radius');
    const detail=app.querySelector('[data-map-detail]');
    const clientLayer=L.layerGroup();
    const opportunityLayer=L.layerGroup();
    const map=L.map('geopharma-map',{zoomControl:true}).setView([-22.9068,-43.1729],12);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png',{maxZoom:19,attribution:'&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'}).addTo(map);
    clientLayer.addTo(map);opportunityLayer.addTo(map);
    let currentPosition=null;let currentMarker=null;let selectedClient=null;const clientItems=[];const opportunityItems=[];
    const setStatus=(message,loading=false,error=false)=>{
        if(!message){status.hidden=true;return;}
        status.hidden=false;status.innerHTML='';
        if(loading){const spinner=document.createElement('span');spinner.className='spinner-border spinner-border-sm';spinner.setAttribute('aria-hidden','true');status.append(spinner);}
        const text=document.createTextNode(message);status.append(text);status.style.background=error?'rgba(153,27,27,.94)':'rgba(15,55,63,.9)';
    };
    const icon=(type)=>L.divIcon({className:'',html:`<span class="map-marker map-marker-${type}"><i class="bi ${type==='client'?'bi-building':'bi-capsule'}"></i></span>`,iconSize:[36,36],iconAnchor:[18,36]});
    const text=(selector,value='—')=>{detail.querySelector(selector).textContent=value||'—';};
    const routeUrl=(lat,lng)=>`https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(lat+','+lng)}`;
    const openDetail=(item,type)=>{
        selectedClient=type==='client'?item:null;detail.hidden=false;
        text('[data-detail-type]',type==='client'?'Cliente cadastrado':'Oportunidade OpenStreetMap');
        text('[data-detail-name]',item.nome_fantasia||item.nome||item.razao_social);
        const address=item.endereco||[item.logradouro,item.numero,item.bairro,item.cidade,item.estado].filter(Boolean).join(', ');
        text('[data-detail-address]',address||'Endereço não informado na fonte');
        text('[data-detail-document]',item.documento_formatado);
        text('[data-detail-phone]',item.telefone_formatado||item.telefone);
        text('[data-detail-visit]',item.ultima_visita?new Date(item.ultima_visita.replace(' ','T')).toLocaleString('pt-BR'):'Nenhuma registrada');
        detail.querySelector('[data-detail-route]').href=routeUrl(item.latitude,item.longitude);
        const call=detail.querySelector('[data-detail-call]');const phone=(item.telefone||'').replace(/\D/g,'');call.hidden=!phone;call.href=phone?`tel:${phone}`:'#';
        const edit=detail.querySelector('[data-detail-edit]');edit.hidden=type!=='client';if(type==='client')edit.href=`/clientes/edit.php?id=${item.id}`;
        detail.querySelector('[data-detail-visit-button]').hidden=type!=='client';
    };
    const applySearch=()=>{
        const term=search.value.trim().toLocaleLowerCase('pt-BR');
        [...clientItems,...opportunityItems].forEach(({marker,item,type})=>{
            const haystack=[item.nome,item.nome_fantasia,item.razao_social,item.documento_formatado,item.cidade].filter(Boolean).join(' ').toLocaleLowerCase('pt-BR');
            const layer=type==='client'?clientLayer:opportunityLayer;
            if(!term||haystack.includes(term)){if(!layer.hasLayer(marker))layer.addLayer(marker);}else if(layer.hasLayer(marker))layer.removeLayer(marker);
        });
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
            currentPosition={latitude:position.coords.latitude,longitude:position.coords.longitude};
            if(currentMarker)map.removeLayer(currentMarker);
            currentMarker=L.circleMarker([currentPosition.latitude,currentPosition.longitude],{radius:9,color:'#fff',weight:3,fillColor:'#2563eb',fillOpacity:1}).addTo(map).bindTooltip('Sua localização');
            map.setView([currentPosition.latitude,currentPosition.longitude],15);if(announce){setStatus('Localização atualizada.');window.setTimeout(()=>setStatus(''),1800);}resolve(currentPosition);
        },()=>{const error=new Error('Não foi possível acessar sua localização. Verifique a permissão do navegador.');if(announce)setStatus(error.message,false,true);reject(error);},{enableHighAccuracy:true,timeout:12000,maximumAge:60000});
    });
    const loadOpportunities=async()=>{
        try{
            let origin=currentPosition;if(!origin){const center=map.getCenter();origin={latitude:center.lat,longitude:center.lng};}
            setStatus('Buscando farmácias reais no OpenStreetMap...',true);opportunityLayer.clearLayers();opportunityItems.length=0;
            const meters=Number(radius.value);const response=await fetch(`/mapa/oportunidades.php?lat=${origin.latitude}&lng=${origin.longitude}&raio=${meters}`,{headers:{Accept:'application/json'}});const data=await response.json();
            if(!response.ok)throw new Error(data.erro||'Não foi possível buscar as farmácias.');
            data.oportunidades.forEach(item=>addItem(item,'opportunity'));applySearch();
            setStatus(`${data.oportunidades.length} farmácia(s) real(is) encontrada(s) no OpenStreetMap.`);window.setTimeout(()=>setStatus(''),3200);
        }catch(error){setStatus(error.message||'Erro ao buscar oportunidades.',false,true);}
    };
    app.querySelector('[data-map-locate]').addEventListener('click',()=>locate());
    app.querySelector('[data-map-opportunities]').addEventListener('click',loadOpportunities);
    app.querySelector('[data-map-detail-close]').addEventListener('click',()=>{detail.hidden=true;});
    search.addEventListener('input',applySearch);
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
    visitForm.addEventListener('submit',async event=>{
        event.preventDefault();const errorBox=visitForm.querySelector('[data-visit-error]');errorBox.hidden=true;const data=new FormData(visitForm);data.set('cliente_id',String(selectedClient?.id||''));data.append('csrf_token',app.dataset.csrf);
        if(currentPosition){data.append('latitude',currentPosition.latitude);data.append('longitude',currentPosition.longitude);}
        try{const response=await fetch('/mapa/registrar-visita.php',{method:'POST',body:data,headers:{Accept:'application/json'}});const result=await response.json();if(!response.ok)throw new Error(result.erro||'Não foi possível registrar a visita.');visitModal.hide();setStatus(result.mensagem);window.setTimeout(()=>setStatus(''),2500);}
        catch(error){errorBox.textContent=error.message||'Erro ao registrar visita.';errorBox.hidden=false;}
        finally{const button=visitForm.querySelector('button[type="submit"]');if(button.dataset.originalContent)button.innerHTML=button.dataset.originalContent;button.disabled=false;button.removeAttribute('aria-busy');button.dataset.loadingActive='false';}
    });
    loadClients();
})();

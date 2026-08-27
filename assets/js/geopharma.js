(() => {
    'use strict';

    const startLoading = (button) => {
        if (!button || button.dataset.loadingActive === 'true') {
            return;
        }

        button.dataset.loadingActive = 'true';
        button.dataset.originalContent = button.innerHTML;
        button.setAttribute('aria-busy', 'true');
        button.disabled = true;

        const label = button.dataset.loadingLabel || 'Carregando...';
        button.innerHTML = `
            <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
            <span>${label}</span>
        `;
    };

    document.addEventListener('submit', (event) => {
        const form = event.target;
        if (!(form instanceof HTMLFormElement)) {
            return;
        }

        const submitter = event.submitter || form.querySelector('button[type="submit"], input[type="submit"]');
        if (submitter instanceof HTMLButtonElement) {
            startLoading(submitter);
        }
    });

    document.addEventListener('click', (event) => {
        const button = event.target.closest('[data-loading-on-click]');
        if (button instanceof HTMLButtonElement) {
            startLoading(button);
        }
    });

    const clienteForm = document.querySelector('[data-cliente-form]');
    if (clienteForm instanceof HTMLFormElement) {
        const documento = clienteForm.querySelector('#documento');
        const status = clienteForm.querySelector('[data-cnpj-status]');
        let ultimoCnpj = '';
        let consultando = false;

        const somenteNumeros = (valor) => valor.replace(/\D/g, '');
        const formatarDocumento = (valor) => somenteNumeros(valor).slice(0, 14)
            .replace(/^(\d{2})(\d)/, '$1.$2')
            .replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3')
            .replace(/\.(\d{3})(\d)/, '.$1/$2')
            .replace(/(\/\d{4})(\d)/, '$1-$2');

        const consultar = async () => {
            const cnpj = somenteNumeros(documento.value);
            if (cnpj.length !== 14 || consultando || cnpj === ultimoCnpj) return;
            consultando = true;
            status.innerHTML = '<span class="spinner-border spinner-border-sm me-1" aria-hidden="true"></span>Buscando os dados públicos e a localização...';
            status.className = 'form-text text-success';
            try {
                const resposta = await fetch('/clientes/consultar-cnpj.php?cnpj=' + encodeURIComponent(cnpj), {headers: {'Accept': 'application/json'}});
                const dados = await resposta.json();
                if (!resposta.ok) throw new Error(dados.erro || 'Não foi possível consultar o CNPJ.');
                ['razao_social','nome_fantasia','telefone','email','cep','logradouro','numero','complemento','bairro','cidade','estado','latitude','longitude'].forEach((campo) => {
                    const input = clienteForm.elements.namedItem(campo);
                    if (input instanceof HTMLInputElement && dados[campo]) input.value = dados[campo];
                });
                ultimoCnpj = cnpj;
                status.textContent = 'Dados preenchidos. Confira as informações antes de salvar.';
                status.className = 'form-text text-success fw-semibold';
            } catch (erro) {
                status.textContent = erro instanceof Error ? erro.message : 'Não foi possível consultar o CNPJ.';
                status.className = 'form-text text-danger fw-semibold';
            } finally {
                consultando = false;
            }
        };

        documento.addEventListener('input', () => {
            documento.value = formatarDocumento(documento.value);
            if (somenteNumeros(documento.value).length === 14) consultar();
        });
        documento.value = formatarDocumento(documento.value);
    }

    const leadForm = document.querySelector('[data-lead-form]');
    if (leadForm instanceof HTMLFormElement) {
        const documento = leadForm.querySelector('#documento');
        const status = leadForm.querySelector('[data-cnpj-status]');
        let ultimoCnpj = '';
        let consultando = false;
        const somenteNumeros = (valor) => valor.replace(/\D/g, '');
        const formatarDocumento = (valor) => somenteNumeros(valor).slice(0, 14).replace(/^(\d{2})(\d)/, '$1.$2').replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3').replace(/\.(\d{3})(\d)/, '.$1/$2').replace(/(\/\d{4})(\d)/, '$1-$2');
        const consultar = async () => {
            const cnpj = somenteNumeros(documento.value);if(cnpj.length!==14||consultando||cnpj===ultimoCnpj)return;consultando=true;status.innerHTML='<span class="spinner-border spinner-border-sm me-1" aria-hidden="true"></span>Consultando dados oficiais...';status.className='form-text text-success';
            try{const resposta=await fetch('/clientes/consultar-cnpj.php?cnpj='+encodeURIComponent(cnpj),{headers:{Accept:'application/json'}});const dados=await resposta.json();if(!resposta.ok)throw new Error(dados.erro||'Não foi possível consultar o CNPJ.');['razao_social','nome_fantasia','telefone','email','cep','logradouro','numero','complemento','bairro','cidade','estado'].forEach(campo=>{const input=leadForm.elements.namedItem(campo);if(input instanceof HTMLInputElement&&dados[campo])input.value=dados[campo];});ultimoCnpj=cnpj;status.textContent='Dados oficiais preenchidos. Confira antes de salvar.';status.className='form-text text-success fw-semibold';}
            catch(erro){status.textContent=erro instanceof Error?erro.message:'Não foi possível consultar o CNPJ.';status.className='form-text text-danger fw-semibold';}finally{consultando=false;}
        };
        documento.addEventListener('input',()=>{documento.value=formatarDocumento(documento.value);if(somenteNumeros(documento.value).length===14)consultar();});documento.value=formatarDocumento(documento.value);
    }
})();

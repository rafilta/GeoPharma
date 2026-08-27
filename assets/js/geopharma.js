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
        const tipo = clienteForm.querySelector('#tipo');
        const documento = clienteForm.querySelector('#documento');
        const botao = clienteForm.querySelector('[data-consultar-cnpj]');
        const rotulo = clienteForm.querySelector('[data-documento-label]');
        const status = clienteForm.querySelector('[data-cnpj-status]');
        let ultimoCnpj = '';
        let consultando = false;

        const somenteNumeros = (valor) => valor.replace(/\D/g, '');
        const formatarDocumento = (valor, juridica) => {
            const numeros = somenteNumeros(valor).slice(0, juridica ? 14 : 11);
            if (juridica) {
                return numeros.replace(/^(\d{2})(\d)/, '$1.$2').replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3').replace(/\.(\d{3})(\d)/, '.$1/$2').replace(/(\/\d{4})(\d)/, '$1-$2');
            }
            return numeros.replace(/^(\d{3})(\d)/, '$1.$2').replace(/\.(\d{3})(\d)/, '.$1.$2').replace(/(\.\d{3})(\d)/, '$1-$2');
        };

        const alternarTipo = () => {
            const juridica = tipo.value === 'juridica';
            rotulo.textContent = juridica ? 'CNPJ' : 'CPF';
            botao.classList.toggle('d-none', !juridica);
            documento.maxLength = juridica ? 18 : 14;
            documento.value = formatarDocumento(documento.value, juridica);
            status.textContent = juridica ? 'Ao completar o CNPJ, os dados públicos serão preenchidos automaticamente.' : 'Informe o CPF do cliente.';
            status.className = 'form-text';
        };

        const consultar = async () => {
            const cnpj = somenteNumeros(documento.value);
            if (tipo.value !== 'juridica' || cnpj.length !== 14 || consultando || cnpj === ultimoCnpj) return;
            consultando = true;
            botao.disabled = true;
            botao.innerHTML = '<span class="spinner-border spinner-border-sm me-1" aria-hidden="true"></span>Consultando...';
            status.textContent = 'Buscando os dados públicos do CNPJ...';
            status.className = 'form-text text-success';
            try {
                const resposta = await fetch('/clientes/consultar-cnpj.php?cnpj=' + encodeURIComponent(cnpj), {headers: {'Accept': 'application/json'}});
                const dados = await resposta.json();
                if (!resposta.ok) throw new Error(dados.erro || 'Não foi possível consultar o CNPJ.');
                ['razao_social','nome_fantasia','telefone','email','cep','logradouro','numero','complemento','bairro','cidade','estado'].forEach((campo) => {
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
                botao.disabled = false;
                botao.innerHTML = '<i class="bi bi-search me-1"></i>Consultar CNPJ';
            }
        };

        tipo.addEventListener('change', alternarTipo);
        documento.addEventListener('input', () => {
            documento.value = formatarDocumento(documento.value, tipo.value === 'juridica');
            if (somenteNumeros(documento.value).length === 14) consultar();
        });
        botao.addEventListener('click', consultar);
        alternarTipo();
    }
})();

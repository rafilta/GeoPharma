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
})();

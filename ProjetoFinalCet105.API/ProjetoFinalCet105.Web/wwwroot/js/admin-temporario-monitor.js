(() => {
    const intervaloVerificacao = 10000; // 10 segundos
    let estadoAnterior = null;
    let timeoutExpiracao = null;

    async function obterEstado() {
        try {
            const response = await fetch(
                '/EstadoAdminTemporario/MeuEstado',
                {
                    method: 'GET',
                    credentials: 'same-origin',
                    cache: 'no-store'
                }
            );

            if (!response.ok)
                return null;

            return await response.json();
        }
        catch (error) {
            console.error('Erro ao verificar acesso administrativo temporário:', error );

            return null;
        }
    }

    function programarExpiracao(dataFim) {
        if (timeoutExpiracao) {
            clearTimeout(timeoutExpiracao);
            timeoutExpiracao = null;
        }

        if (!dataFim)
            return;

        const milissegundos = new Date(dataFim).getTime() - Date.now();

        if (milissegundos <= 0)
            return;

        timeoutExpiracao = setTimeout(() => {
            verificarEstado();
        }, milissegundos + 500);
    }

    async function verificarEstado() {
        const estado = await obterEstado();

        if (!estado)
            return;

        const ativo = estado.ativo === true;

        if (estadoAnterior === null) {
            estadoAnterior = ativo;

            atualizarInterface(ativo, estado.dataFim);
            if (ativo)
                programarExpiracao(estado.dataFim);

            console.log(
                '[Admin Temporário] Estado inicial:',
                ativo ? 'ATIVO' : 'INATIVO'
            );

            return;
        }

        if (ativo !== estadoAnterior) {
            estadoAnterior = ativo;
            atualizarInterface(ativo, estado.dataFim);
            if (ativo) {
                programarExpiracao(estado.dataFim);

                console.log('[Admin Temporário] ACESSO CONCEDIDO',  estado);
            }
            else {
                if (timeoutExpiracao) {
                    clearTimeout(timeoutExpiracao);
                    timeoutExpiracao = null;
                }

                console.log('[Admin Temporário] ACESSO TERMINADO/REVOGADO');
            }
        }
    }

    function atualizarInterface(ativo, dataFim = null) {
        const menu = document.getElementById('adminTemporarioMenu');
        const tempo = document.getElementById('adminTemporarioTempo');

        if (!menu)
            return;

        menu.style.display = ativo ? 'block' : 'none';

        if (!ativo || !dataFim) {
            if (tempo)
                tempo.textContent = '';

            return;
        }

        if (tempo) {
            const fim = new Date(dataFim);

            tempo.textContent =
                `até ${fim.toLocaleTimeString('pt-PT', {
                    hour: '2-digit',
                    minute: '2-digit'
                })}`;
        }
    }

    verificarEstado();

    setInterval(
        verificarEstado,
        intervaloVerificacao
    );
})();
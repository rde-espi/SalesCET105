(function () {

    const intervaloVerificacao = 10000;

    async function atualizarContadorNotificacoes() {
        try {
            const response = await fetch(
                "/Notificacoes/ContadorNaoLidas",
                {
                    method: "GET",
                    cache: "no-store"
                }
            );

            if (!response.ok) {
                return;
            }

            const data = await response.json();

            const badge =
                document.getElementById("notificationBadge");

            if (!badge) {
                return;
            }

            const contador = Number(data.contador ?? 0);

            if (contador > 0) {
                badge.textContent =
                    contador > 99 ? "99+" : contador.toString();

                badge.style.display = "inline-flex";
            }
            else {
                badge.textContent = "0";
                badge.style.display = "none";
            }
        }
        catch (error) {
            console.error(
                "Erro ao atualizar contador de notificações:",
                error
            );
        }
    }

    atualizarContadorNotificacoes();

    setInterval(
        atualizarContadorNotificacoes,
        intervaloVerificacao
    );

})();
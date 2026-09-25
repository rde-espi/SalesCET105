(function () {

    const intervaloVerificacao = 10000;

    async function atualizarContadorMensagens() {
        try {
            const response = await fetch(
                "/MensagensCliente/ContadorNaoLidas",
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
                document.getElementById("mensagensNaoLidasBadge");

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
                "Erro ao atualizar contador de mensagens:",
                error
            );
        }
    }

    async function atualizarMensagens() {
    await atualizarContadorMensagens();
    await atualizarConversasNaoLidas();
}

atualizarMensagens();

setInterval(atualizarMensagens, intervaloVerificacao);

    async function atualizarConversasNaoLidas() {

    const conversas = document.querySelectorAll( ".ib-mensagens-func-item[data-conversa-id]");

    if (!conversas.length) {
        return;
    }

    for (const conversa of conversas) {

        const conversaId =
            conversa.dataset.conversaId;

        try {
            const response = await fetch(
                `/MensagensCliente/ContadorNaoLidasConversa?id=${conversaId}`,
                {
                    method: "GET",
                    cache: "no-store"
                }
            );

            if (!response.ok) {
                continue;
            }

            const data = await response.json();

            const contador =
                Number(data.contador ?? 0);

            const badge =
                conversa.querySelector(
                    "[data-unread-badge]"
                );

            if (contador > 0) {

                conversa.classList.add(
                    "ib-mensagens-func-item-unread"
                );

                if (badge) {
                    badge.textContent =
                        contador > 99
                            ? "99+"
                            : contador.toString();

                    badge.style.display = "inline-flex";
                }
            }
            else {

                conversa.classList.remove(
                    "ib-mensagens-func-item-unread"
                );

                if (badge) {
                    badge.textContent = "0";
                    badge.style.display = "none";
                }
            }
        }
        catch (error) {
            console.error(
                `Erro ao atualizar conversa ${conversaId}:`,
                error
            );
        }
    }
}

})();
// Biblioteca · comportamiento de la interfaz.
// Todo se activa con atributos data-*, así que en las páginas que no los tienen no hace nada.
(function () {
    'use strict';

    var sinMovimiento = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // ------------------------------------------------------------------
    // Aviso (toast) con el TempData["Mensaje"]: se oculta solo a los 4 s,
    // se pausa mientras el puntero o el foco están encima.
    // ------------------------------------------------------------------
    function iniciarAvisos() {
        document.querySelectorAll('[data-aviso]').forEach(function (aviso) {
            var zona = aviso.parentElement;
            var cerrar = aviso.querySelector('[data-aviso-cerrar]');
            var restante = 4000;
            var inicio = 0;
            var temporizador = null;
            var pausado = false;
            var cerrado = false;

            function ocultar() {
                if (cerrado) return;
                cerrado = true;
                clearTimeout(temporizador);

                // Si el foco estaba en el aviso, lo devolvemos al contenido principal.
                var devolverFoco = aviso.contains(document.activeElement);
                aviso.classList.add('saliendo');

                setTimeout(function () {
                    zona.remove();
                    if (devolverFoco) {
                        var principal = document.getElementById('contenido');
                        if (principal) principal.focus();
                    }
                }, sinMovimiento ? 0 : 250);
            }

            function reanudar() {
                if (!pausado || cerrado) return;
                pausado = false;
                aviso.classList.remove('pausado');
                inicio = Date.now();
                temporizador = setTimeout(ocultar, restante);
            }

            function pausar() {
                if (pausado || cerrado) return;
                pausado = true;
                clearTimeout(temporizador);
                restante = Math.max(800, restante - (Date.now() - inicio));
                aviso.classList.add('pausado');
            }

            if (cerrar) cerrar.addEventListener('click', ocultar);

            aviso.addEventListener('mouseenter', pausar);
            aviso.addEventListener('focusin', pausar);
            aviso.addEventListener('mouseleave', function () {
                if (!aviso.contains(document.activeElement)) reanudar();
            });
            aviso.addEventListener('focusout', function (e) {
                if (!aviso.contains(e.relatedTarget) && !aviso.matches(':hover')) reanudar();
            });

            document.addEventListener('keydown', function (e) {
                if (e.key === 'Escape') ocultar();
            });

            inicio = Date.now();
            temporizador = setTimeout(ocultar, restante);
        });
    }

    // ------------------------------------------------------------------
    // Búsqueda en vivo de libros.
    // El formulario funciona sin JS (GET a /Libros?titulo=...). Con JS se pide
    // el mismo Index con X-Requested-With y el controller devuelve solo las filas.
    // ------------------------------------------------------------------
    function iniciarBusquedaViva() {
        var form = document.querySelector('[data-busqueda-form]');
        if (!form || !window.fetch || !window.AbortController) return;

        var input = form.querySelector('[data-busqueda-input]');
        var destino = document.querySelector('[data-busqueda-resultados]');
        if (!input || !destino) return;

        var conteo = document.querySelector('[data-busqueda-conteo]');
        var etiqueta = document.querySelector('[data-busqueda-etiqueta]');
        var limpiar = form.querySelector('[data-busqueda-limpiar]');

        var espera = null;
        var controlador = null;
        var ultimo = input.value.trim();

        function actualizarConteo() {
            var total = destino.querySelectorAll('tr[data-libro]').length;
            if (conteo) conteo.textContent = total;
            if (etiqueta) etiqueta.textContent = total === 1 ? 'libro' : 'libros';
        }

        function buscar(forzar) {
            var texto = input.value.trim();
            if (!forzar && texto === ultimo) return;
            ultimo = texto;

            // Cancela la petición anterior si todavía no respondió.
            if (controlador) controlador.abort();
            var actual = new AbortController();
            controlador = actual;

            var url = new URL(form.action, window.location.href);
            url.search = '';
            if (texto) url.searchParams.set('titulo', texto);

            destino.setAttribute('aria-busy', 'true');

            // no-store: la respuesta parcial comparte URL con la página completa;
            // así el navegador no la reutiliza al volver atrás o recargar.
            fetch(url.toString(), {
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                credentials: 'same-origin',
                cache: 'no-store',
                signal: actual.signal
            })
                .then(function (respuesta) {
                    if (!respuesta.ok) throw new Error('HTTP ' + respuesta.status);
                    return respuesta.text();
                })
                .then(function (html) {
                    destino.innerHTML = html;
                    actualizarConteo();
                    if (limpiar) limpiar.hidden = !texto;
                    if (window.history && history.replaceState) {
                        history.replaceState(null, '', url.pathname + url.search);
                    }
                })
                .catch(function (error) {
                    if (error.name === 'AbortError') return;
                    // Si algo falla, el próximo intento vuelve a consultar.
                    ultimo = null;
                    console.warn('No se pudo actualizar la búsqueda:', error);
                })
                .then(function () {
                    if (controlador === actual) destino.removeAttribute('aria-busy');
                });
        }

        input.addEventListener('input', function () {
            clearTimeout(espera);
            espera = setTimeout(buscar, 250);
        });

        form.addEventListener('submit', function (e) {
            e.preventDefault();
            clearTimeout(espera);
            buscar(true);
        });

        if (limpiar) {
            limpiar.addEventListener('click', function (e) {
                e.preventDefault();
                input.value = '';
                clearTimeout(espera);
                buscar(true);
                input.focus();
            });
        }
    }

    // ------------------------------------------------------------------
    // Normaliza campos: el ISBN se guarda sin guiones ni espacios y el DNI solo con dígitos.
    // ------------------------------------------------------------------
    function iniciarNormalizacion() {
        document.querySelectorAll('[data-normalizar]').forEach(function (campo) {
            campo.addEventListener('change', function () {
                var tipo = campo.getAttribute('data-normalizar');
                var valor = campo.value;
                if (tipo === 'isbn') valor = valor.replace(/[\s-]/g, '').toUpperCase();
                if (tipo === 'dni') valor = valor.replace(/\D/g, '');
                if (valor !== campo.value) campo.value = valor;
            });
        });
    }

    function iniciar() {
        iniciarAvisos();
        iniciarBusquedaViva();
        iniciarNormalizacion();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', iniciar);
    } else {
        iniciar();
    }
})();

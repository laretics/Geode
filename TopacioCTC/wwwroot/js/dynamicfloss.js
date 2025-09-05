window.setGroupColor = (groupId, color) => {
    const element = document.getElementById(groupId);
    if (element) {
        element.querySelectorAll('[stroke], line, path, rect, ellipse, polyline, poligon').forEach(el => {
            el.setAttribute('stroke', color);
        });
    }
}

window.setGroupVisibility = (groupId, visible) => {
    const element = document.getElementById(groupId);
    if (element) {
        element.style.visibility = visible ? "visible" : "hidden";
    }
}

//Handler para capturar las pulsaciones del usuario sobre los circuitos de vía
//en el esquema del enclavamiento.
window.svgLtyClickInterop = {
    addLtyClickHandlers: function (svgSelector, dotNetHelper) {
        const svg = document.querySelector(svgSelector);
        if (!svg) return;
        // Selecciona todos los grupos con id que empiece con "lty_"
        svg.querySelectorAll('g[id^="lty_"]').forEach(g => {
            if (g._hasalreadyhandler) return; //Ya tengo el handler... no tengo que hacer nada.
            g._hasalreadyhandler = true;
            g.style.cursor = "pointer";
            g.addEventListener('click', function (e) {
                console.log("Click en", g.id);
                //Invocación del método C# con el id del circuito
                dotNetHelper.invokeMethodAsync('OnCircuitClick', g.id);
                e.stopPropagation();
            });
        });
    }
};

function dynaChangeStroke(elementId, color) {
    var element = document.getElementById(elementId);
    if (element) {
        element.style.stroke = color;
    }
};
function dynaChangeFill(elementId, color) {
    var element = document.getElementById(elementId);
    if (element) {
        element.style.fill = color;
    }
};
function dynaChangeVisible(elementId, visible) {
    var element = document.getElementById(elementId);
    if (element) {
        element.style.visibility = visible ? 'visible' : 'hidden';
    }
};



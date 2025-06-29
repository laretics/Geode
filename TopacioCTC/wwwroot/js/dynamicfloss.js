function dynaChangeStroke(elementId, color) {
    var element = document.getElementById(elementId);
    if (element) {
        element.style.stroke = color;
    }
}
function dynaChangeFill(elementId, color) {
    var element = document.getElementById(elementId);
    if (element) {
        element.style.fill = color;
    }
}
function dynaChangeVisible(elementId, visible) {
    var element = document.getElementById(elementId);
    if (element) {
        element.style.visibility = visible ? 'visible' : 'hidden';
    }
}

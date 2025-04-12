function scrollToEnd(element) {
    element.scrollTop = element.scrollHeight;
}

// Ajoute la classe 'hide' à l'élément
function addHideClass(element) {
    if (element) {
        element.classList.add('hide');
    }
}

// Retire la classe 'hide' de l'élément
function removeHideClass(element) {
    if (element) {
        element.classList.remove('hide');
    }
}
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

// Désactive le champ de saisie en ajoutant l'attribut 'disabled'
function disableInput(element) {
    if (element) {
        element.setAttribute('disabled', 'disabled');
    }
}

// Active le champ de saisie en supprimant l'attribut 'disabled'
function enableInput(element) {
    if (element) {
        element.removeAttribute('disabled');
    }
}
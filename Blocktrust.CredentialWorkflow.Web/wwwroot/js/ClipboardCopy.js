window.clipboardCopy = {
    copyText: function(text) {
        navigator.clipboard.writeText(text).then(function () {
        })
            .catch(function (error) {
                alert(error);
            });
    }
};

window.focusElement = (element) => {
    var elementToFocus=document.getElementById(element);
    elementToFocus.focus();
}
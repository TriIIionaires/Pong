window.SetFocusToElement = function(id) {
    document.getElementById(id).focus();
}

window.MovePaddle = function(id, position) {
    document.getElementById(id).style.top = position.toString() + "%";
}

window.MoveBall = function (x, y) {
    document.getElementById("ball").style.top = y.toString() + "%";
    document.getElementById("ball").style.left = x.toString() + "%";
}
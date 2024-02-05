function getHostname(href){
    return href.substring(href.indexOf('/'))
}

function refreshPage(href){
    window.location.href = href
}
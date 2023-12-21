function setupTooltips() {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    })
};

function showModal(modalElement) {
    var modal = new bootstrap.Modal(modalElement);
    modal.show();
};

async function downloadFileFromStream(fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);

    const url = URL.createObjectURL(blob);

    triggerFileDownload(fileName, url);

    URL.revokeObjectURL(url);
}

function triggerFileDownload(fileName, url) {
    const anchorElement = document.createElement('a');
    anchorElement.href = url;

    if (fileName) {
        anchorElement.download = fileName;
    }

    anchorElement.click();
    anchorElement.remove();
}
function validateNumericInput(e) {
    var allowedChars = '0123456789.,-eE ';
    function contains(stringValue, charValue) {
        return stringValue.indexOf(charValue) > -1;
    }
    var invalidKey = e.key.length === 1 && !contains(allowedChars, e.key)
        || e.key === '.' && contains(e.target.value, '.');
    invalidKey && e.preventDefault();
};

function setNumericOnly() {
    var numericOnlyElements = [].slice.call(document.querySelectorAll('.numeric-only'))
    numericOnlyElements.map(function (numericOnlyElement) {
        numericOnlyElement.addEventListener('keypress', validateNumericInput);
    });
};

function setIntegerOnly() {
    var numericOnlyElements = [].slice.call(document.querySelectorAll('.integer-only'))
    numericOnlyElements.map(function (numericOnlyElement) {
        numericOnlyElement.addEventListener('keypress', validateNumericInput);
    });
};

function validateIntegerInput(e) {
    var allowedChars = '0123456789';
    function contains(stringValue, charValue) {
        return stringValue.indexOf(charValue) > -1;
    }
    var invalidKey = e.key.length === 1 && !contains(allowedChars, e.key)
        || e.key === '.' && contains(e.target.value, '.');
    invalidKey && e.preventDefault();
};

function setEnterNext() {
    var enterNextElements = [].slice.call(document.querySelectorAll('.enter-next'))
    enterNextElements.map(function (enterNextElement) {
        enterNextElement.addEventListener('keyup', focusNext);
    });
};

function focusNext(event) {
    if (event.key != 'Enter' ||
        event.shiftKey == true ||
        event.altKey == true ||
        event.metaKey == true)
        return;

    //Note that this doesn't honour tab-indexes
    event.preventDefault();
    //Isolate the node that we're after
    const currentNode = event.target;
    //find all tab-able elements
    const allElements = document.querySelectorAll('input, button, a, area, object, select, textarea, [contenteditable]');
    //Find the current tab index.
    const currentIndex = [...allElements].findIndex(el => currentNode.isEqualNode(el))
    //focus the following element
    const targetIndex = (currentIndex + 1) % allElements.length;
    allElements[targetIndex].focus();
};

function expand(elementId) {
    var element = document.getElementById(elementId);
    var bootstrapInstance = bootstrap.Collapse.getOrCreateInstance(element);
    bootstrapInstance.show();
}
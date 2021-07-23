function RemoveCommitButtonFromMudTable() {
    let mudTable = document.getElementById('status_subdivisionTable');
    let mudRows = mudTable.querySelector('.mud-table-row');
    let mudRowHeaders = mudRows.querySelectorAll('.mud-table-cell');
    let mudBody = mudTable.querySelector('.mud-table-body');
    let mudCommitButton = Array.from(mudBody.querySelectorAll('[data-label]')).filter(el => el.textContent === '');
    
    if (mudRowHeaders[3] !== undefined) {
        mudRowHeaders[3].remove();
    }
    if (mudCommitButton.length > 0) {
        mudCommitButton.forEach(mcb => mcb.remove());
    }
}

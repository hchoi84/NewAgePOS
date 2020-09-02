function confirmTransfer(id) {
  var xferElemId = ['confirmTransfer', id].join('_');
  var xferBtnId = ['confirmTransferButton', id].join('_');

  var xferElem = document.getElementById(xferElemId);
  var xferBtn = document.getElementById(xferBtnId);

  if (xferElem.style.display == 'none') {
    xferElem.style.display = 'block';
    xferBtn.className = 'btn btn-primary border-warning';
  }
  else {
    xferElem.style.display = 'none';
    xferBtn.className = 'btn btn-primary';
  }
}
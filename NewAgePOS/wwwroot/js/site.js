function confirmRemove(id) {
  var remElemId = ['confirmRemove', id].join('_');
  var remBtnId = ['confirmRemoveButton', id].join('_');
  var xferElemId = ['confirmTransfer', id].join('_');
  var xferBtnId = ['confirmTransferButton', id].join('_');

  var removeElem = document.getElementById(remElemId);
  var removeBtn = document.getElementById(remBtnId);
  var xferElem = document.getElementById(xferElemId);
  var xferBtn = document.getElementById(xferBtnId);

  if (removeElem.style.display == 'none') {
    removeElem.style.display = 'block';
    removeBtn.className = 'btn btn-primary border-warning';
    xferElem.style.display = 'none';
    xferBtn.className = 'btn btn-primary';
  }
  else {
    removeElem.style.display = 'none';
    removeBtn.className = 'btn btn-primary';
  }
}

function confirmTransfer(id) {
  var remElemId = ['confirmRemove', id].join('_');
  var remBtnId = ['confirmRemoveButton', id].join('_');
  var xferElemId = ['confirmTransfer', id].join('_');
  var xferBtnId = ['confirmTransferButton', id].join('_');

  var removeElem = document.getElementById(remElemId);
  var removeBtn = document.getElementById(remBtnId);
  var xferElem = document.getElementById(xferElemId);
  var xferBtn = document.getElementById(xferBtnId);

  if (xferElem.style.display == 'none') {
    removeElem.style.display = 'none';
    removeBtn.className = 'btn btn-primary';
    xferElem.style.display = 'block';
    xferBtn.className = 'btn btn-primary border-warning';
  }
  else {
    xferElem.style.display = 'none';
    xferBtn.className = 'btn btn-primary';
  }
}
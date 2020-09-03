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

function searchQueryMsg(id) {
  var selElem = document.getElementById('SearchMethod');
  var optVal = selElem.options[selElem.selectedIndex].value;

  var msgElem = document.getElementById(id);

  if (optVal == 'EmailAddress') {
    msgElem.innerText = 'ex: email@domain.com';
  }
  else if (optVal == 'PhoneNumber') {
    msgElem.innerText = '(ex: 1234567890) 10 digit with no dashes';
  }
  else if (optVal == 'SaleId') {
    msgElem.innerText = 'Numbers only';
  }
  else if (optVal == 'LastName') {
    msgElem.innerText = 'Last name only. Please do not use full name';
  }
}
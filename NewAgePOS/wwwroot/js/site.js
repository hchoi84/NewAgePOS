function confirmTransfer(id) {
  var xferElemId = ['confirmTransfer', id].join('_');
  var xferBtnId = ['confirmTransferButton', id].join('_');

  var xferElem = document.getElementById(xferElemId);
  var xferBtn = document.getElementById(xferBtnId);

  if (xferElem.style.display == 'none') {
    xferElem.style.display = 'block';
    xferBtn.className = 'btn btn-primary border-warning';
  } else {
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
  } else if (optVal == 'PhoneNumber') {
    msgElem.innerText = '(ex: 1234567890) 10 digit with no dashes';
  } else if (optVal == 'SaleId') {
    msgElem.innerText = 'Numbers only';
  } else if (optVal == 'LastName') {
    msgElem.innerText = 'Last name only. Please do not use full name';
  }
}

function refundMethod(id) {
  var selectElem = document.getElementById('RefundMethod');
  var optVal = selectElem.options[selectElem.selectedIndex].value;

  var giftCardCodeInputBox = document.getElementById(id);

  if (optVal == 'GiftCard') {
    giftCardCodeInputBox.style.display = 'block';
  } else {
    giftCardCodeInputBox.style.display = 'none';
  }
}

function addRemove(thisId, thatId, whatId) {
  var thisButton = document.getElementById(thisId);
  var thisDivId = thisId.replace("Button", "");
  var thisDiv = document.getElementById(thisDivId);

  var thatButton = document.getElementById(thatId);
  var thatDivId = thatId.replace("Button", "");
  var thatDiv = document.getElementById(thatDivId);

  var whatButton = document.getElementById(whatId);
  var whatDivId = whatId.replace("Button", "");
  var whatDiv = document.getElementById(whatDivId);

  if (thisDiv.style.display == 'none') {
    thisDiv.style.display = 'block';
    thatDiv.style.display = 'none';
    whatDiv.style.display = 'none';
    thisButton.className = 'btn btn-primary border-warning';
    thatButton.className = 'btn btn-primary';
    whatButton.className = 'btn btn-primary';
  }
  else {
    thisDiv.style.display = 'none';
    thatDiv.style.display = 'none';
    whatDiv.style.display = 'none';
    thisButton.className = 'btn btn-primary';
    thatButton.className = 'btn btn-primary';
    whatButton.className = 'btn btn-primary';
  }
}

function confirm(id) {
  var divId = id.replace("Btn", "");
  var div = document.getElementById(divId);

  if (div.style.display == "none") {
    div.style.display = "block";
  }
  else {
    div.style.display = "none";
  }
}
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

function confirm(thisId) {
  var thisButton = document.getElementById(thisId);
  var thisDivId = thisId.replace("Btn", "");
  var thisDiv = document.getElementById(thisDivId);

  if (thisDiv.style.display == 'none') {
    thisDiv.style.display = 'block';
    thisButton.className = 'btn btn-primary border-warning';
  }
  else {
    thisDiv.style.display = 'none';
    thisButton.className = 'btn btn-primary';
  }
}

function confirm2Btns(thisId, thatId) {
  var thisBtn = document.getElementById(thisId);
  var thisDivId = thisId.replace("Btn", "");
  var thisDiv = document.getElementById(thisDivId);

  var thatBtn = document.getElementById(thatId);
  var thatDivId = thatId.replace("Btn", "");
  var thatDiv = document.getElementById(thatDivId);

  if (thisDiv.style.display == 'none') {
    thisDiv.style.display = 'block';
    thatDiv.style.display = 'none';
    thisBtn.className = 'btn btn-primary border-warning';
    thatBtn.className = 'btn btn-primary';
  }
  else {
    thisDiv.style.display = 'none';
    thatDiv.style.display = 'none';
    thisBtn.className = 'btn btn-primary';
    thatBtn.className = 'btn btn-primary';
  }
}

function confirm3Btns(thisId, thatId, whatId) {
  var thisBtn = document.getElementById(thisId);
  var thisDivId = thisId.replace("Btn", "");
  var thisDiv = document.getElementById(thisDivId);

  var thatBtn = document.getElementById(thatId);
  var thatDivId = thatId.replace("Btn", "");
  var thatDiv = document.getElementById(thatDivId);

  var whatBtn = document.getElementById(whatId);
  var whatDivId = whatId.replace("Btn", "");
  var whatDiv = document.getElementById(whatDivId);

  if (thisDiv.style.display == 'none') {
    thisDiv.style.display = 'block';
    thatDiv.style.display = 'none';
    whatDiv.style.display = 'none';
    thisBtn.className = 'btn btn-primary border-warning';
    thatBtn.className = 'btn btn-primary';
    whatBtn.className = 'btn btn-primary';
  }
  else {
    thisDiv.style.display = 'none';
    thatDiv.style.display = 'none';
    whatDiv.style.display = 'none';
    thisBtn.className = 'btn btn-primary';
    thatBtn.className = 'btn btn-primary';
    whatBtn.className = 'btn btn-primary';
  }
}

function countChars(inputId, maxChar) {
  var input = document.getElementById(inputId);
  var display = document.getElementById(inputId + 'CountDisplay');
  var button = document.getElementById(inputId + 'SubmitButton');
  var count = input.value.length;
  display.innerText = count + ' / ' + maxChar;

  if (count > maxChar) {
    display.className = "text-danger";
    button.disabled = true;
  }
  else {
    display.className = "text-info";
    button.disabled = false;
  }
}

function countChars2Inputs(inputId1, inputId2, buttonId, maxChar1, maxChar2) {
  var input1 = document.getElementById(inputId1);
  var display1 = document.getElementById(inputId1 + 'CountDisplay');
  var count1 = input1.value.length;
  display1.innerText = count1 + ' / ' + maxChar1;

  var input2 = document.getElementById(inputId2);
  var display2 = document.getElementById(inputId2 + 'CountDisplay');
  var count2 = input2.value.length;
  display2.innerText = count2 + ' / ' + maxChar2;

  var button = document.getElementById(buttonId);

  if (count1 > maxChar1) {
    display1.className = 'text-danger';
  } else {
    display1.className = 'text-info';
  }

  if (count2 > maxChar2) {
    display2.className = 'text-danger';
  } else {
    display2.className = 'text-info';
  }

  if (count1 > maxChar1 || count2 > maxChar2) {
    button.disabled = true;
  }
}
chrome.browserAction.onClicked.addListener(function(tab) {
  chrome.tabs.create({
    url: '<the url of the extension here>'
  });
});
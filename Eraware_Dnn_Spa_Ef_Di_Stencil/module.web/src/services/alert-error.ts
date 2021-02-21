export default function alertError(reason: any) {
  if (typeof reason === "string") {
    alert(reason);
  }

  if (typeof reason === "object") {
    if (reason.hasOwnProperty("ModelState")) {
      var modelState = reason.ModelState;
      for (var key in modelState) {
        var message = [""];
        var item = modelState[key];
        for (var key in item) {
          message.push(item[key]);
          alert(message.join("\n"));
          return;
        }
      }
    }
    debugger;
    if (reason.hasOwnProperty("isApiException") && reason.isApiException) {
      var response = JSON.parse(reason.response);
      alert(response.Message);
      return;
    }
    if (reason.hasOwnProperty("Message")) {
      alert(reason.Message);
    }
    else {
      alert(JSON.stringify(reason));
    }
  }
}

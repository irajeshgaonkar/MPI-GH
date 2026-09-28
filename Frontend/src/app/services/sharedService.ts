import { Injectable } from "@angular/core";

@Injectable({
  providedIn: "root",
})
export class SharedService {
  constructor() {}
  ngOnInit() {}

  showToast(message: unknown) {
    let x = document.getElementById("snackbar") as HTMLElement;
    x.innerHTML = this.normalizeMessage(message);
    x.className = "show";
    setTimeout(function () {
      x.className = x.className.replace("show", "");
    }, 2000);
  }

  private normalizeMessage(message: unknown): string {
    if (typeof message === "string" && message.trim()) {
      return message;
    }

    if (message && typeof message === "object") {
      const errorObject = message as Record<string, unknown>;
      const nestedMessage = errorObject["message"] || errorObject["error"] || errorObject["title"] || errorObject["detail"];
      if (typeof nestedMessage === "string" && nestedMessage.trim()) {
        return nestedMessage;
      }

      try {
        return JSON.stringify(message);
      } catch {
        return "An unexpected error occurred";
      }
    }

    return "An unexpected error occurred";
  }
}

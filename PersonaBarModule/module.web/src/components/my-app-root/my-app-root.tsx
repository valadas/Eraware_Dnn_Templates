import { Component, Host, h, Prop, State } from "@stencil/core";
import type { IDnnPersonaBarUtilities } from "../../dnn/types";
import { EchoDto, EchoClient } from "../../services/services";

@Component({
  tag: "my-app-root",
  styleUrl: "my-app-root.scss",
  shadow: true,
})
export class MyAppRoot {
  /** The wrapping html element. */
  @Prop() wrapper: HTMLElement;

  /** The persona bar utilites. */
  @Prop() util: IDnnPersonaBarUtilities;

  @State() notificationMessage: string =  "Test notification message...";
  @State() clickToClose: boolean;
  @State() closeButtonText: string = "Close";
  @State() size: "" | "large" = "";
  @State() timeout: number = 2000;
  @State() notificationType: "notify" | "error" = "notify";
  
  private readonly echoClient: EchoClient;

  constructor() {
    this.echoClient = new EchoClient({ sf: this.util.sf });
  }
  
  private moduleName = "MyPersonaBarModule";

  private save(): void {
    this.util.confirm(
      "Are you sure you want to save?",
      "Yes",
      "No",
      () => {
        void(async () => {
          try {
            const dto = new EchoDto({
              message: "Hello, World!",
            });

            const result = await this.echoClient.echo(dto);

            if (result?.message != null) {
              this.util.notify(`${result.message} was saved`, { type: "notify" });
            }
          } catch (error) {
            this.util.notify(`Error: ${error}`, { type: "error" });
          }
        })();
      },
      () => {
        this.util.notify("Not saved!", { type: "error" });
      }
    );
  }

  render() {
    return (
      <Host>
        <header>
          <h3 class="title">Title will go here...</h3>
          <dnn-button
            reversed
            size="large"
            onClick={() => this.util.closePersonaBar()}
          >
            {this.util.getResx(this.moduleName, "Cancel")}
          </dnn-button>
          <dnn-button
            size="large"
            onClick={() => this.save()}
          >
            {this.util.getResx(this.moduleName, "Save")}
          </dnn-button>
          <button class="close" onClick={() => this.util.closePersonaBar()}>X</button>
        </header>
        <div class="container">
          <div class="content">
            <p>Current culture: {this.util.getCulture()}</p>
            <p>Application root path: {this.util.getApplicationRootPath()}</p>
            <p>Module loaded at: {this.util.dayjs().format("dddd DD MMMM YYYY HH:mm:ss")}</p>
            <dnn-button
              onClick={() => this.util.loadPanel("Dnn.Pages", {})}
            >
              Open pages module
            </dnn-button>
            <fieldset>
              <legend>Notification</legend>
              <dnn-input
                label="Message"
                onValueChange={e => this.notificationMessage = e.detail as string}
                value={this.notificationMessage}
              />
              <label>
                Click to close: 
                <dnn-checkbox
                  checked={this.clickToClose ? "checked" : "unchecked"}
                  onClick={() => this.clickToClose = !this.clickToClose}
                />
              </label>
              {!this.clickToClose &&
                <dnn-input
                  label="Timeout"
                  helpText="In miliseconds, only useful if click to close is disabled."
                  type="number"
                  onValueChange={e => this.timeout = e.detail as number}
                  value={this.timeout}
                />
              }
              {this.clickToClose &&
                <dnn-input
                  label="Close button text"
                  helpText="Only useful if click to close is enabled."
                  onValueChange={e => this.closeButtonText = e.detail as string}
                  value={this.closeButtonText}
                />
              }
              <dnn-select
                label="Size"
                value={this.size}
                helpText="Can be used to make the notification slightly larger."
                onValueChange={e => this.size = e.detail as ""}
              >
                <option value="">Normal</option>
                <option value="large">Large</option>
              </dnn-select>
              <dnn-select
                label="Type"
                value={this.notificationType}
                helpText="Can be used to make the notification more important."
                onValueChange={e => this.notificationType = e.detail as "notify" | "error"}
              >
                <option value="notify">Notify</option>
                <option value="error">Error</option>
              </dnn-select>
              <dnn-button
                onClick={
                  () => this.util.notify(
                    this.notificationMessage,
                  {
                    clickToClose: this.clickToClose,
                    closeButtonText: this.closeButtonText,
                    size: this.size,
                    timeout: this.timeout,
                    type: this.notificationType,
                  })
                }
              >
                Notify
              </dnn-button>
            </fieldset>
          </div>
        </div>
      </Host>
    );
  }
}

import { Component, h, Prop, Host, Listen } from "@stencil/core";
import { ItemClient, LocalizationClient, LocalizationViewModel } from "../../services/services";
import state, { localizationState } from "../../store/state";
import alertError from "../../services/alert-error";

@Component({
  tag: 'my-component',
  styleUrl: 'my-component.scss',
  shadow: true
})
export class MyComponent {
  private service: ItemClient;
  private localizationService: LocalizationClient;
  private resx: LocalizationViewModel;
  private modal: HTMLDnnModalElement;
  private editForm: HTMLMyEditElement;

  constructor() {
    this.service = new ItemClient({ moduleId: this.moduleId });
    state.moduleId = this.moduleId;
    this.localizationService = new LocalizationClient({ moduleId: this.moduleId });
  }

  /** The Dnn module id, required in order to access web services. */
  @Prop() moduleId!: number;

  componentWillLoad() {
    return new Promise<void>((resolve, reject) => {
      this.localizationService.getLocalization()
        .then(vm => {
          localizationState.viewModel = vm!;
          this.resx = localizationState.viewModel;
          resolve();
        })
        .catch(reason => {
          alertError(reason);
          reject();
        });
    })
  }

  componentDidLoad(): void {
    this.service.userCanEdit().then(canEdit => state.userCanEdit = canEdit);
  }

  @Listen("itemCreated")
  handleItemCreated() {
    state.searchQuery = "";
  }

  render() {
    return <Host>
      <div class="header">
        <dnn-searchbox placeholder={this.resx?.uI?.searchPlaceholder || "Search"} onQueryChanged={e => state.searchQuery = e.detail} />
        {state.userCanEdit &&
          <dnn-button
            class="add"
            onClick={() => this.modal.show().then(() => this.editForm.setFocus())}
          >
            {this.resx?.uI?.addItem}
          </dnn-button>
        }
      </div>
      <my-items-list />
      <dnn-modal
        ref={e => this.modal = e!}
        showCloseButton={false}
        backdropDismiss={false}
      >
        <my-edit ref={e => this.editForm = e!} item={
          {
            id: -1,
            name: "",
            description: "",
          }
        } />
      </dnn-modal>
    </Host>;
  }
}

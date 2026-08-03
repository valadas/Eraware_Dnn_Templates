import { Component, h, Prop, Host, Listen } from "@stencil/core";
import { ItemClient, LocalizationClient, LocalizationViewModel } from "../../services/services";
import state, { localizationState } from "../../store/state";

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

  async componentWillLoad() {

    const vm = await this.localizationService.getLocalization();
    localizationState.viewModel = vm!;
    this.resx = localizationState.viewModel;
  }

  async componentDidLoad() {
    state.userCanEdit = await this.service.userCanEdit();
  }

  @Listen("itemCreated")
  handleItemCreated() {
    state.searchQuery = "";
  }

  private async handleAdd() {
    await this.modal.show();
    await this.editForm.setFocus();
  }

  render() {
    return <Host>
      <div class="header">
        <dnn-searchbox placeholder={this.resx?.uI?.searchPlaceholder || "Search"} onQueryChanged={e => state.searchQuery = e.detail} />
        {state.userCanEdit &&
          <dnn-button
            class="add"
            onClick={() => void this.handleAdd()}
          >
            {this.resx?.uI?.addItem}
          </dnn-button>
        }
      </div>
      <my-items-list />
      <dnn-modal
        ref={e => this.modal = e!}
        hideCloseButton
        perventBackdropDismiss
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

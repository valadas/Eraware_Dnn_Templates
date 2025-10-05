import {
  Component, Host, h, Element, Method,
  Event, EventEmitter, Prop
} from '@stencil/core';
import { CreateItemDTO, IItemViewModel, ItemClient, UIInfo, UpdateItemDTO } from '../../services/services';
import state, { store, localizationState } from "../../store/state";
import alertError from "../../services/alert-error";

@Component({
  tag: 'my-edit',
  styleUrl: 'my-edit.scss',
  shadow: true,
})
export class MyEdit {
  /** The item to create or edit. */
  @Prop({ mutable: true }) item: IItemViewModel;

  @Element() el!: HTMLMyEditElement;

  private nameInput!: HTMLDnnInputElement;
  private itemClient!: ItemClient;
  private resx: UIInfo | undefined;

  constructor() {
    this.itemClient = new ItemClient({
      moduleId: state.moduleId,
    });
    this.resx = localizationState.viewModel.uI;
  }

  /** Sets focus on the first form element */
  @Method()
  public async setFocus() {
    setTimeout(() => {
      this.nameInput.focus();
    }, 500);
  }

  /** Resets the form to insert a new item. */
  @Method()
  public async resetForm() {
    this.item = {
      id: -1,
      name: "",
      description: "",
    }
  }

  /** Fires up when an item got created. */
  @Event() itemCreated: EventEmitter

  componentWillLoad() {
    if (this.item == undefined) {
      this.resetForm();
    }
  }

  private hideModal(): void {
    this.el.closest("dnn-modal")?.hide();
  }

  private saveItem(): void {
    if (this.item.id! < 1) {
      const createItemDTO = new CreateItemDTO({
        name: this.item.name,
        description: this.item.description,
      });
      this.itemClient.createItem(createItemDTO)
        .then(() => {
          this.itemCreated.emit();
          this.hideModal();
        },
          reason => alertError(reason))
        .catch(reason => alertError(reason));
    }
    else {
      const updateItemDTO = new UpdateItemDTO({
        id: this.item.id,
        name: this.item.name,
        description: this.item.description,
      });
      this.itemClient.updateItem(updateItemDTO)
        .then(() => {
          this.hideModal();
          state.items = state.items.map(i => i.id == this.item.id ? this.item : i);
        }, reason => alert(reason))
        .catch(reason => alert(reason));
    }
    const oldCanEdit = state.userCanEdit;
    store.reset();
    state.userCanEdit = oldCanEdit;
  }

  render() {
    return (
      <Host>
        <form
          onSubmit={e => {
            e.preventDefault();
            this.saveItem();
          }}
        >
          <dnn-input
            label={this.resx?.name}
            type="text"
            value={this.item.name}
            required
            ref={e => this.nameInput = e!}
            onValueInput={e => this.item = { ...this.item, name: e.detail as string }}
          />
          <dnn-textarea
            label={this.resx?.description}
            value={this.item.description}
            onValueInput={e => this.item = { ...this.item, description: e.detail as string }}
          />

          <div class="controls">
            <dnn-button
              reversed
              onClick={() => this.hideModal()}
            >
              {this.resx?.cancel}
            </dnn-button>
            <dnn-button
              formButtonType="submit"
            >
              {this.item.id! < 1 ? this.resx?.create : this.resx?.save}
            </dnn-button>
          </div>
        </form>
      </Host>
    );
  }
}

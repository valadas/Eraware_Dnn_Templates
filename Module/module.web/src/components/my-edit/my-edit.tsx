import {
  Component, Host, h, Element, Method,
  Event, EventEmitter, Prop
} from '@stencil/core';
import { CreateItemDTO, IItemViewModel, ItemClient, UIInfo, UpdateItemDTO } from '../../services/services';
import state, { store, localizationState } from "../../store/state";

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
    return Promise.resolve();
  }

  /** Resets the form to insert a new item. */
  @Method()
  public async resetForm() {
    this.item = {
      id: -1,
      name: "",
      description: "",
    }
    return Promise.resolve();
  }

  /** Fires up when an item got created. */
  @Event() itemCreated: EventEmitter

  componentWillLoad() {
    if (this.item == undefined) {
      void this.resetForm();
    }
  }

  private async hideModal() {
    await this.el.closest("dnn-modal")?.hide();
  }

  private async saveItem() {
    if (this.item.id! < 1) {
      const createItemDTO = new CreateItemDTO({
        name: this.item.name,
        description: this.item.description,
      });
      await this.itemClient.createItem(createItemDTO);
      this.itemCreated.emit();
      await this.hideModal();
    }
    else {
      const updateItemDTO = new UpdateItemDTO({
        id: this.item.id,
        name: this.item.name,
        description: this.item.description,
      });

      await this.itemClient.updateItem(updateItemDTO);
      await this.hideModal();
      state.items = state.items.map(i => i.id == this.item.id ? this.item : i);
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
            void this.saveItem();
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
            onValueInput={e => this.item = { ...this.item, description: e.detail }}
          />

          <div class="controls">
            <dnn-button
              reversed
              onClick={() => void this.hideModal()}
            >
              {this.resx?.cancel}
            </dnn-button>
            <dnn-button
              type="submit"
            >
              {this.item.id! < 1 ? this.resx?.create : this.resx?.save}
            </dnn-button>
          </div>
        </form>
      </Host>
    );
  }
}


import { Component, Host, h, Prop } from '@stencil/core';
import { IItemViewModel } from '../../services/services';
import state from '../../store/state';

@Component({
  tag: 'my-item-details',
  styleUrl: 'my-item-details.scss',
  shadow: true,
})
export class MyItemDetails {
  /** The item to display */
  @Prop() item!: IItemViewModel;

  private modal!: HTMLDnnModalElement;
  private editForm!: HTMLMyEditElement;

  render() {
    return (
      <Host>
      <div class= "item-details" >
      { this.item.description }
      < /div>
    {
      state.userCanEdit &&
      <div class="controls" >
        <dnn-button
      type = "primary"
      onClick = {() => this.modal.show().then(() => this.editForm.setFocus())
    }
            >
      Edit
      < /dnn-button>
      < dnn - modal
    ref = { e => this.modal = e }
    showCloseButton = { false}
    backdropDismiss = { false}
      >
      <my-edit ref = { e => this.editForm = e } item = { this.item } />
        </dnn-modal>
        < /div>
  }
      </Host>
    );
  }
}

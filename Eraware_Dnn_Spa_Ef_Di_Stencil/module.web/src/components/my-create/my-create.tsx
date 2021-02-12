import { Component, Host, h } from '@stencil/core';

@Component({
  tag: 'my-create',
  styleUrl: 'my-create.scss',
  shadow: true,
})
export class MyCreate {

  private modal!: HTMLDnnModalElement;
  private editForm!: HTMLMyEditElement;

  render() {
    return (
      <Host>
        <dnn-button
          type="primary"
          onClick={() => this.modal.show().then(() => this.editForm.setFocus())}
        >
          Add Item
        </dnn-button>
        <dnn-modal
          ref={e => this.modal = e}
          showCloseButton={false}
          backdropDismiss={false}
        >
          <my-edit ref={e => this.editForm = e} />
        </dnn-modal>
      </Host>
    );
  }
}

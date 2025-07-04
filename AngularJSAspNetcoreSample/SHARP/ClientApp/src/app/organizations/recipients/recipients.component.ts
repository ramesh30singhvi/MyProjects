import { Component, Input } from '@angular/core';
import { RecipientModel } from '../../models/organizations/recipient.model';

@Component({
  selector: "app-recipients",
  templateUrl: "./recipients.component.html",
  styleUrls: ["./recipients.component.scss"],
})
export class RecipientsComponent {
  @Input()
  public recipients: RecipientModel[];
  public typingRecipient: string;

  public createRecipient(): void {
    const recipient = { recipient: this.typingRecipient };
    this.recipients.push(recipient);
    this.typingRecipient = null;
  }

  public deleteRecipient(toDelete: string): void {
    const index = this.recipients.findIndex(
      ({ recipient }) => recipient == toDelete
    );
    this.recipients.splice(index, 1);
  }
}

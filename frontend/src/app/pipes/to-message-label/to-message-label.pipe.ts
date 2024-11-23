import { Pipe, PipeTransform } from '@angular/core';
import { toReadableFormat } from '../../util';
import { Message, MessageType, Subscription, SubscriptionType } from '../../schemas/subscription.schema';

@Pipe({
  name: 'toMessageLabel',
  standalone: true
})
export class ToMessageLabelPipe implements PipeTransform {

  transform(value: Message['type'], subscriptionType: Subscription['subscriptionType']): string {
    return toReadableFormat(MessageType[value] as keyof typeof MessageType, subscriptionType === SubscriptionType.Twitch ? "Twitch" : "YouTube");
  }

}

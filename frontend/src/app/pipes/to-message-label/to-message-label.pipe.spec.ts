import { ToMessageLabelPipe } from "./to-message-label.pipe";

describe('ToMessageLabelPipe', () => {
  it('create an instance', () => {
    const pipe = new ToMessageLabelPipe();
    expect(pipe).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmbedFieldEditorComponent } from './embed-field-editor.component';

describe('EmbedFieldEditorComponent', () => {
  let component: EmbedFieldEditorComponent;
  let fixture: ComponentFixture<EmbedFieldEditorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmbedFieldEditorComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EmbedFieldEditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
